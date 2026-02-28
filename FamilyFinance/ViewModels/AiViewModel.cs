using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.ViewModels;

public partial class AiViewModel : ObservableObject
{
    private readonly ChatGPTService _chatGpt;
    private readonly ISpeechToText _speechToText;
    private CancellationTokenSource? _cts;

    public AiViewModel(ChatGPTService chatGpt, ISpeechToText speechToText)
    {
        _chatGpt = chatGpt;
        _speechToText = speechToText;
    }

    [ObservableProperty]
    private ObservableCollection<ChatMessage> messages = new();

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private bool isRecording;

    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsProcessing)
            return;

        var prompt = UserInput.Trim();
        UserInput = string.Empty;

        Messages.Add(new ChatMessage { Content = prompt, IsUser = true });

        IsProcessing = true;
        try
        {
            var (message, recordsCreated) = await _chatGpt.ProcessPromptAsync(prompt);

            var responseText = message;
            if (recordsCreated > 0)
                responseText += $"\n\n({recordsCreated} record(s) created)";

            Messages.Add(new ChatMessage { Content = responseText, IsUser = false });
        }
        catch (HttpRequestException ex)
        {
            Messages.Add(new ChatMessage
            {
                Content = $"Network error: {ex.Message}",
                IsUser = false
            });
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatMessage
            {
                Content = $"Error: {ex.Message}",
                IsUser = false
            });
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    public async Task ToggleRecordingAsync()
    {
        if (IsProcessing)
            return;

        if (IsRecording)
        {
            // Stop recording
            _cts?.Cancel();
            IsRecording = false;
            return;
        }

        // Request permissions
        var isGranted = await _speechToText.RequestPermissions();
        if (!isGranted)
        {
            await Shell.Current.DisplayAlert(
                "Permission Required",
                "Microphone permission is needed for voice input.",
                "OK");
            return;
        }

        IsRecording = true;
        _cts = new CancellationTokenSource();

        try
        {
            var result = await _speechToText.ListenAsync(
                CultureInfo.CurrentCulture,
                new Progress<string>(partial =>
                {
                    UserInput = partial;
                }),
                _cts.Token);

            IsRecording = false;

            if (result.IsSuccessful && !string.IsNullOrWhiteSpace(result.Text))
            {
                UserInput = result.Text;
                await SendMessageAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled recording — send whatever was captured
            if (!string.IsNullOrWhiteSpace(UserInput))
                await SendMessageAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Speech recognition failed: {ex.Message}", "OK");
        }
        finally
        {
            IsRecording = false;
        }
    }
}
