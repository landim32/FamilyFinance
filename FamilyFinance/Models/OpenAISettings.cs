namespace FamilyFinance.Models;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string WhisperModel { get; set; } = "whisper-1";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}
