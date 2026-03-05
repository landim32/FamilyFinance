namespace FamilyFinance.Services;

public interface IChatGPTService
{
    Task<bool> IsConfiguredAsync();
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName);
    Task<(string message, int recordsCreated)> ProcessPromptAsync(string userPrompt);
}
