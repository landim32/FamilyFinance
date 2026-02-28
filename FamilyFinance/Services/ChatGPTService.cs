using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyFinance.Models;

namespace FamilyFinance.Services;

public class ChatGPTService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseService _db;
    private OpenAISettings? _settings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string SystemPrompt = """
        You are a financial assistant for the "Family Finance" app.
        Your job is to help users create financial records based on natural language input.

        The app has the following entities:
        - Account: title (required), amount (required, > 0), isCredit (true = income/credit/received, false = expense/debit/paid), notes (optional), personName (optional), accountTypeName (optional)
        - Person: name (required), phone (optional), email (optional)
        - AccountType: name (required), description (optional)

        When the user describes a financial transaction or asks to create a record, respond ONLY with a JSON object in this exact format:
        {
          "actions": [
            {
              "type": "create_account",
              "title": "string",
              "amount": number,
              "isCredit": boolean,
              "notes": "string or null",
              "personName": "string or null",
              "accountTypeName": "string or null"
            }
          ],
          "message": "A friendly message in the user's language describing what was created"
        }

        Other action types you can use:
        - { "type": "create_person", "name": "string", "phone": "string or null", "email": "string or null" }
        - { "type": "create_account_type", "name": "string", "description": "string or null" }

        Rules:
        - If a person is mentioned, include personName in the account action (the app will find or create them automatically)
        - If an account type is mentioned, include accountTypeName (the app will find or create it automatically)
        - Amount must always be positive
        - Determine credit/debit from context: paid, spent, bought, expense = debit (isCredit: false); received, earned, sold, income = credit (isCredit: true)
        - If the user is just chatting or asking questions (not requesting record creation), respond with: { "actions": [], "message": "your response" }
        - ALWAYS respond with valid JSON only. No markdown, no code blocks, no extra text.
        - Respond in the same language as the user
        """;

    public ChatGPTService(DatabaseService db)
    {
        _db = db;
        _httpClient = new HttpClient();
    }

    private async Task<OpenAISettings> GetSettingsAsync()
    {
        if (_settings is not null)
            return _settings;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var doc = JsonDocument.Parse(json);
            var section = doc.RootElement.GetProperty("OpenAI");

            _settings = new OpenAISettings
            {
                ApiKey = section.TryGetProperty("ApiKey", out var k) ? k.GetString() ?? "" : "",
                Model = section.TryGetProperty("Model", out var m) ? m.GetString() ?? "gpt-4o-mini" : "gpt-4o-mini",
                WhisperModel = section.TryGetProperty("WhisperModel", out var w) ? w.GetString() ?? "whisper-1" : "whisper-1",
                BaseUrl = section.TryGetProperty("BaseUrl", out var b) ? b.GetString() ?? "https://api.openai.com/v1" : "https://api.openai.com/v1"
            };
        }
        catch
        {
            _settings = new OpenAISettings();
        }

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        return _settings;
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var settings = await GetSettingsAsync();
        return !string.IsNullOrWhiteSpace(settings.ApiKey) &&
               !settings.ApiKey.StartsWith("sk-your");
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        var settings = await GetSettingsAsync();

        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(audioStream), "file", fileName);
        content.Add(new StringContent(settings.WhisperModel), "model");

        var response = await _httpClient.PostAsync(
            $"{settings.BaseUrl}/audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("text").GetString() ?? "";
    }

    public async Task<(string message, int recordsCreated)> ProcessPromptAsync(string userPrompt)
    {
        var settings = await GetSettingsAsync();

        if (!await IsConfiguredAsync())
        {
            return ("Please configure your OpenAI API key in Resources/Raw/appsettings.json and rebuild the app.", 0);
        }

        var requestBody = new
        {
            model = settings.Model,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.3,
            max_tokens = 1024
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"{settings.BaseUrl}/chat/completions", jsonContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseJson);
        var assistantMessage = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        return await ProcessAiResponseAsync(assistantMessage);
    }

    private async Task<(string message, int recordsCreated)> ProcessAiResponseAsync(string aiResponse)
    {
        try
        {
            var cleanJson = aiResponse.Trim();

            // Remove markdown code blocks if ChatGPT wraps them
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson[(cleanJson.IndexOf('\n') + 1)..];
                var lastBacktick = cleanJson.LastIndexOf("```", StringComparison.Ordinal);
                if (lastBacktick >= 0)
                    cleanJson = cleanJson[..lastBacktick];
                cleanJson = cleanJson.Trim();
            }

            var result = JsonSerializer.Deserialize<AiResponse>(cleanJson, JsonOptions);
            if (result is null)
                return ("I couldn't process the response. Please try again.", 0);

            var recordsCreated = 0;

            foreach (var action in result.Actions ?? [])
            {
                switch (action.Type)
                {
                    case "create_account":
                        await CreateAccountFromActionAsync(action);
                        recordsCreated++;
                        break;
                    case "create_person":
                        await CreatePersonFromActionAsync(action);
                        recordsCreated++;
                        break;
                    case "create_account_type":
                        await CreateAccountTypeFromActionAsync(action);
                        recordsCreated++;
                        break;
                }
            }

            return (result.Message ?? "Done!", recordsCreated);
        }
        catch (JsonException)
        {
            // If ChatGPT returned plain text instead of JSON
            return (aiResponse, 0);
        }
    }

    private async Task CreateAccountFromActionAsync(AiAction action)
    {
        int? personId = null;
        int? accountTypeId = null;

        // Find or create person
        if (!string.IsNullOrWhiteSpace(action.PersonName))
        {
            var people = await _db.GetPeopleAsync();
            var person = people.FirstOrDefault(p =>
                p.Name.Equals(action.PersonName, StringComparison.OrdinalIgnoreCase));

            if (person is null)
            {
                person = new Person { Name = action.PersonName };
                await _db.SavePersonAsync(person);
                people = await _db.GetPeopleAsync();
                person = people.Last(p =>
                    p.Name.Equals(action.PersonName, StringComparison.OrdinalIgnoreCase));
            }

            personId = person.Id;
        }

        // Find or create account type
        if (!string.IsNullOrWhiteSpace(action.AccountTypeName))
        {
            var types = await _db.GetAccountTypesAsync();
            var accountType = types.FirstOrDefault(t =>
                t.Name.Equals(action.AccountTypeName, StringComparison.OrdinalIgnoreCase));

            if (accountType is null)
            {
                accountType = new AccountType { Name = action.AccountTypeName };
                await _db.SaveAccountTypeAsync(accountType);
                types = await _db.GetAccountTypesAsync();
                accountType = types.Last(t =>
                    t.Name.Equals(action.AccountTypeName, StringComparison.OrdinalIgnoreCase));
            }

            accountTypeId = accountType.Id;
        }

        var account = new Account
        {
            Title = action.Title ?? "Untitled",
            Amount = action.Amount ?? 0,
            IsCredit = action.IsCredit ?? false,
            Notes = action.Notes,
            PersonId = personId,
            AccountTypeId = accountTypeId,
            CreatedAt = DateTime.Now
        };

        await _db.SaveAccountAsync(account);
    }

    private async Task CreatePersonFromActionAsync(AiAction action)
    {
        var person = new Person
        {
            Name = action.Name ?? "Unknown",
            Phone = action.Phone,
            Email = action.Email
        };
        await _db.SavePersonAsync(person);
    }

    private async Task CreateAccountTypeFromActionAsync(AiAction action)
    {
        var accountType = new AccountType
        {
            Name = action.Name ?? "Unknown",
            Description = action.Description
        };
        await _db.SaveAccountTypeAsync(accountType);
    }
}

// --- AI Response Models ---

public class AiResponse
{
    public List<AiAction>? Actions { get; set; }
    public string? Message { get; set; }
}

public class AiAction
{
    public string? Type { get; set; }

    // Account fields
    public string? Title { get; set; }
    public decimal? Amount { get; set; }
    public bool? IsCredit { get; set; }
    public string? Notes { get; set; }
    public string? PersonName { get; set; }
    public string? AccountTypeName { get; set; }

    // Person fields
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // AccountType fields
    public string? Description { get; set; }
}
