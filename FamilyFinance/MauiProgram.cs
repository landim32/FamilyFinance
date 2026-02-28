using CommunityToolkit.Maui;
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

        // ViewModels
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddTransient<PersonViewModel>();
        builder.Services.AddTransient<AccountTypeViewModel>();
        builder.Services.AddTransient<ExportViewModel>();

        // Pages
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<AccountFormPage>();
        builder.Services.AddTransient<PersonPage>();
        builder.Services.AddTransient<PersonFormPage>();
        builder.Services.AddTransient<AccountTypePage>();
        builder.Services.AddTransient<ExportPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
