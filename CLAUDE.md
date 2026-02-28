# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Restore packages
dotnet restore

# Build (all platforms)
dotnet build

# Build for specific platform
dotnet build -f net8.0-android
dotnet build -f net8.0-windows10.0.19041.0
dotnet build -f net8.0-ios
dotnet build -f net8.0-maccatalyst

# Run on Windows
dotnet build -f net8.0-windows10.0.19041.0 -t:Run
```

No test project exists yet. The MAUI workload must be installed: `dotnet workload install maui`.

## Architecture

This is a **.NET 8 MAUI** cross-platform mobile/desktop app for personal finance management, targeting Android 21+, iOS 15+, macOS Catalyst 15+, and Windows 10+.

### MVVM Pattern with CommunityToolkit.Mvvm

The app uses strict MVVM with source generators:

- **Models** (`FamilyFinance/Models/`): Plain C# classes with `[Table]`, `[PrimaryKey]`, `[AutoIncrement]` SQLite attributes. Three entities: `Account`, `Person`, `AccountType`.
- **ViewModels** (`FamilyFinance/ViewModels/`): Decorated with `[ObservableObject]`, use `[ObservableProperty]` for bindable fields and `[RelayCommand]` for commands. All ViewModels are registered as **transient** in DI.
- **Views** (`FamilyFinance/Views/`): XAML ContentPages that receive their ViewModel via constructor injection and set `BindingContext`. Each page has a `.xaml` + `.xaml.cs` pair.

### Services

All services are registered as **singletons** in `MauiProgram.cs`:

- **DatabaseService**: SQLite via sqlite-net-pcl. Database at `FileSystem.AppDataDirectory/familyfinance.db`. Lazy-initialized on first access. All operations are async. Manually populates navigation properties (Person, AccountType on Account).
- **ChatGPTService**: OpenAI API integration for natural language financial record creation. Sends structured system prompts, parses JSON responses to auto-create Account/Person/AccountType entities. Also handles voice transcription via Whisper API.
- **MigrationService**: Exports person financial data to JSON format.

### Navigation

`AppShell.xaml` defines a flyout (hamburger menu) with routes: AccountPage, PersonPage, AccountTypePage, ExportPage, AiPage. Navigation uses Shell routing with query parameters.

### Configuration

`FamilyFinance/Resources/Raw/appsettings.json` contains OpenAI settings (API key, model names, base URL). Loaded at runtime via `OpenAISettings` model.

### Key Dependencies

- `sqlite-net-pcl` / `SQLitePCLRaw.bundle_green` — SQLite ORM
- `CommunityToolkit.Mvvm` — MVVM source generators (`[ObservableProperty]`, `[RelayCommand]`)
- `CommunityToolkit.Maui` — Speech-to-text, media picker utilities

### Conventions

- ViewModels use an `IsProcessing` boolean flag to guard against double-submissions during async operations.
- Person deletion unlinks associated accounts (sets `PersonId = null`) rather than cascading deletes.
- Profile photos are stored as Base64 strings in the database, converted for display via `Base64ToImageConverter` (in `Helpers/`).
- The AI assistant prompt instructs GPT to return structured JSON with `person_name`, `account_type`, `title`, `amount`, `is_credit`, `notes` fields.
