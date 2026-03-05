using System.Text.Json;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using FamilyFinance.AppConfiguration;
using FamilyFinance.Data;
using FamilyFinance.Models;
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

        // AutoMapper — scans Infra assembly for all Profiles
        builder.Services.AddAutoMapper(typeof(AppDatabase).Assembly);

        // Application Services (Domain Services — registered via Application project)
        builder.Services.AddApplicationServices();

        // Database
        builder.Services.AddSingleton(sp =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "familyfinance.db");
            var db = new AppDatabase(dbPath);
            db.InitializeAsync().GetAwaiter().GetResult();
            return db;
        });

        // Repositories
        builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
        builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
        builder.Services.AddSingleton<IAccountTypeRepository, AccountTypeRepository>();

        // AppServices
        builder.Services.AddSingleton<IChatGPTService>(sp =>
        {
            var service = new ChatGPTService(
                sp.GetRequiredService<IPersonRepository>(),
                sp.GetRequiredService<IAccountTypeRepository>(),
                sp.GetRequiredService<IAccountRepository>());

            service.SetSettingsLoader(async () =>
            {
                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.json");
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();
                    var doc = JsonDocument.Parse(json);
                    var section = doc.RootElement.GetProperty("OpenAI");

                    return new OpenAISettings
                    {
                        ApiKey = section.TryGetProperty("ApiKey", out var k) ? k.GetString() ?? "" : "",
                        Model = section.TryGetProperty("Model", out var m) ? m.GetString() ?? "gpt-4o-mini" : "gpt-4o-mini",
                        WhisperModel = section.TryGetProperty("WhisperModel", out var w) ? w.GetString() ?? "whisper-1" : "whisper-1",
                        BaseUrl = section.TryGetProperty("BaseUrl", out var b) ? b.GetString() ?? "https://api.openai.com/v1" : "https://api.openai.com/v1"
                    };
                }
                catch
                {
                    return new OpenAISettings();
                }
            });

            return service;
        });

        builder.Services.AddSingleton<IMigrationService>(sp =>
            new MigrationService(
                sp.GetRequiredService<IPersonRepository>(),
                sp.GetRequiredService<IAccountRepository>(),
                sp.GetRequiredService<IAccountTypeRepository>(),
                () => FileSystem.AppDataDirectory));

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
