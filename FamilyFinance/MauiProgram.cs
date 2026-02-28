using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using FamilyFinance.Services;
using FamilyFinance.ViewModels;
using FamilyFinance.Views;
using Microsoft.Extensions.Logging;

namespace FamilyFinance;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MigrationService>();
        builder.Services.AddSingleton<ChatGPTService>();
        builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);

        // ViewModels
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddTransient<PersonViewModel>();
        builder.Services.AddTransient<AccountTypeViewModel>();
        builder.Services.AddTransient<ExportViewModel>();
        builder.Services.AddTransient<AiViewModel>();

        // Pages
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<AccountFormPage>();
        builder.Services.AddTransient<PersonPage>();
        builder.Services.AddTransient<PersonFormPage>();
        builder.Services.AddTransient<AccountTypePage>();
        builder.Services.AddTransient<ExportPage>();
        builder.Services.AddTransient<AiPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
