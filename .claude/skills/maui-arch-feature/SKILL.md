---
name: maui-arch-feature
description: Guides the implementation of a new feature (entity with full CRUD) in this .NET MAUI app following the established MVVM architecture. Covers Model, ViewModel, Views (list + form), Service layer, DI registration, navigation, and AppShell integration. Use when adding new screens, entities, or CRUD features.
allowed-tools: Read, Grep, Glob, Bash, Write, Edit, Task
---

# .NET MAUI — Feature Implementation Guide (MVVM + SQLite)

You are an expert assistant that helps developers create new features following the exact architecture patterns of this FamilyFinance .NET MAUI project. You guide the user through ALL required layers.

## Input

The user will describe the feature or entity to create: `$ARGUMENTS`

Before generating code, read existing files (use `Account` as primary reference for a full CRUD entity, `AccountType` for an inline-form entity) to match current patterns exactly.

---

## Architecture & Data Flow

```
View (XAML + Code-Behind) → ViewModel (MVVM) → Service (SQLite) → SQLite DB
```

**Pattern:** MVVM with CommunityToolkit.Mvvm source generators
**Database:** SQLite via sqlite-net-pcl (async, lazy-initialized)
**Navigation:** Shell flyout + programmatic sub-routes
**DI:** Services as singletons, ViewModels and Pages as transient

**Single project structure:**
```
FamilyFinance/
├── Models/          — SQLite entities (plain C# + attributes)
├── ViewModels/      — ObservableObject with source generators
├── Views/           — XAML ContentPages + code-behind
├── Services/        — Singleton services (DatabaseService, etc.)
├── Helpers/         — Value converters
├── Resources/       — Images, Fonts, Styles, Raw assets
└── Platforms/       — Platform-specific code
```

---

## Step-by-Step Implementation

### Step 1: Model — `FamilyFinance/Models/{Entity}.cs`

Use sqlite-net-pcl attributes directly on the class. No base class. Follow the existing pattern from `Account.cs` and `Person.cs`.

```csharp
using SQLite;

namespace FamilyFinance.Models
{
    [Table("{Entity}")]
    public class {Entity}
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;

        // Optional fields — use nullable types
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign keys — nullable int
        public int? RelatedEntityId { get; set; }

        // Navigation properties — mark with [Ignore]
        [Ignore]
        public RelatedEntity? RelatedEntity { get; set; }
    }
}
```

**Conventions:**
- `[Table("TableName")]` on class
- `[PrimaryKey, AutoIncrement]` on `Id`
- `[NotNull]` on required fields
- `[Ignore]` on navigation properties (sqlite-net-pcl does not support joins)
- Default values: `string.Empty` for strings, `DateTime.Now` for timestamps
- Foreign keys as `int?` (nullable to allow unlinking)

---

### Step 2: DatabaseService — Modify `FamilyFinance/Services/DatabaseService.cs`

Add table creation, CRUD methods, and navigation property population.

#### 2a. Table Creation — Add to `InitAsync()`

```csharp
await db.CreateTableAsync<{Entity}>();
```

#### 2b. CRUD Methods

Follow the exact pattern from existing methods (`GetAccountsAsync`, `SaveAccountAsync`, `DeleteAccountAsync`):

```csharp
// GET ALL
public async Task<List<{Entity}>> Get{Entity}sAsync()
{
    var db = await GetDatabaseAsync();
    return await db.Table<{Entity}>().ToListAsync();
}

// GET BY ID
public async Task<{Entity}?> Get{Entity}ByIdAsync(int id)
{
    var db = await GetDatabaseAsync();
    return await db.Table<{Entity}>().Where(x => x.Id == id).FirstOrDefaultAsync();
}

// SAVE (Insert or Update based on Id)
public async Task<int> Save{Entity}Async({Entity} entity)
{
    var db = await GetDatabaseAsync();
    if (entity.Id == 0)
        return await db.InsertAsync(entity);
    else
        return await db.UpdateAsync(entity);
}

// DELETE
public async Task<int> Delete{Entity}Async({Entity} entity)
{
    var db = await GetDatabaseAsync();
    // If this entity has dependents, unlink them first:
    // var dependents = await db.Table<Dependent>().Where(x => x.{Entity}Id == entity.Id).ToListAsync();
    // foreach (var dep in dependents)
    // {
    //     dep.{Entity}Id = null;
    //     await db.UpdateAsync(dep);
    // }
    return await db.DeleteAsync(entity);
}

// COUNT (for display purposes)
public async Task<int> GetAccountCountBy{Entity}Async(int entityId)
{
    var db = await GetDatabaseAsync();
    return await db.Table<Account>().Where(a => a.{Entity}Id == entityId).CountAsync();
}
```

#### 2c. Navigation Property Population (if entity has FKs)

Follow the `PopulateAccountRelationsAsync()` pattern — load all related entities into dictionaries and iterate:

```csharp
private async Task Populate{Entity}RelationsAsync(List<{Entity}> entities)
{
    var db = await GetDatabaseAsync();
    var relatedItems = await db.Table<RelatedEntity>().ToListAsync();
    var relatedDict = relatedItems.ToDictionary(r => r.Id);

    foreach (var entity in entities)
    {
        if (entity.RelatedEntityId.HasValue && relatedDict.TryGetValue(entity.RelatedEntityId.Value, out var related))
            entity.RelatedEntity = related;
    }
}
```

---

### Step 3: ViewModel — `FamilyFinance/ViewModels/{Entity}ViewModel.cs`

Follow the exact pattern from `AccountViewModel.cs` (full CRUD) or `AccountTypeViewModel.cs` (inline form).

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Views;
using System.Collections.ObjectModel;

namespace FamilyFinance.ViewModels
{
    public partial class {Entity}ViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        public {Entity}ViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
        }

        // ── List Properties ──

        [ObservableProperty]
        private ObservableCollection<{Entity}> _items = new();

        [ObservableProperty]
        private bool _isProcessing;

        // ── Form Properties (if using separate form page) ──

        [ObservableProperty]
        private {Entity}? _editingItem;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string? _description;

        // ── List Commands ──

        [RelayCommand]
        private async Task LoadItemsAsync()
        {
            var items = await _db.Get{Entity}sAsync();
            Items = new ObservableCollection<{Entity}>(items);
        }

        [RelayCommand]
        private async Task DeleteItemAsync({Entity} item)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm", $"Delete '{item.Name}'?", "Yes", "No");
            if (!confirm) return;

            await _db.Delete{Entity}Async(item);
            await LoadItemsAsync();
        }

        // ── Navigation Commands (for separate form page) ──

        [RelayCommand]
        private async Task GoToAddAsync()
        {
            await Shell.Current.GoToAsync(nameof({Entity}FormPage));
        }

        [RelayCommand]
        private async Task GoToEditAsync({Entity} item)
        {
            await Shell.Current.GoToAsync(nameof({Entity}FormPage),
                new Dictionary<string, object> { { "{Entity}", item } });
        }

        // ── Form Commands ──

        public void SetEditingItem({Entity} item)
        {
            EditingItem = item;
            Name = item.Name;
            Description = item.Description;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsProcessing) return;
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Validation", "Name is required.", "OK");
                return;
            }

            try
            {
                IsProcessing = true;

                var entity = EditingItem ?? new {Entity}();
                entity.Name = Name.Trim();
                entity.Description = Description?.Trim();

                await _db.Save{Entity}Async(entity);
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        // ── Inline Form Commands (alternative, for AccountType-style) ──
        // Use [ObservableProperty] bool _isFormVisible;
        // Use [ObservableProperty] string _formTitle = "Add {Entity}";
        // Toggle visibility instead of navigating to a separate page.
    }
}
```

**Conventions:**
- Class is `partial` (required for source generators)
- Inherits from `ObservableObject` (CommunityToolkit.Mvvm)
- `[ObservableProperty]` on private fields with `_camelCase` naming → generates `PascalCase` public property
- `[RelayCommand]` on async methods with `Async` suffix → generates `MethodNameCommand` (without Async)
- `IsProcessing` flag guards double-submissions
- Confirmation dialog before delete via `Shell.Current.DisplayAlert`
- Navigation forward: `Shell.Current.GoToAsync(nameof(Page), params)`
- Navigation back: `Shell.Current.GoToAsync("..")`
- Constructor injection of `DatabaseService`

---

### Step 4: List View — `FamilyFinance/Views/{Entity}Page.xaml` + `.xaml.cs`

#### 4a. XAML — `{Entity}Page.xaml`

Follow the pattern from `AccountPage.xaml` or `PersonPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:FamilyFinance.ViewModels"
             x:Class="FamilyFinance.Views.{Entity}Page"
             x:DataType="vm:{Entity}ViewModel"
             Title="{Entity}s"
             BackgroundColor="{AppThemeBinding Light=White, Dark=#1E1E1E}">

    <Grid RowDefinitions="*,Auto" Padding="0">

        <!-- List -->
        <CollectionView Grid.Row="0"
                        ItemsSource="{Binding Items}"
                        SelectionMode="None"
                        Margin="16,8">

            <CollectionView.EmptyView>
                <Label Text="No items found."
                       HorizontalOptions="Center"
                       VerticalOptions="Center"
                       TextColor="Gray"
                       FontSize="16" />
            </CollectionView.EmptyView>

            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="model:{Entity}">
                    <SwipeView>
                        <SwipeView.RightItems>
                            <SwipeItems>
                                <SwipeItem Text="Delete"
                                           BackgroundColor="Red"
                                           Command="{Binding Source={RelativeSource AncestorType={x:Type vm:{Entity}ViewModel}}, Path=DeleteItemCommand}"
                                           CommandParameter="{Binding .}" />
                            </SwipeItems>
                        </SwipeView.RightItems>

                        <Frame Margin="0,4" Padding="12" CornerRadius="8" HasShadow="True"
                               BackgroundColor="{AppThemeBinding Light=White, Dark=#2D2D2D}">
                            <Frame.GestureRecognizers>
                                <TapGestureRecognizer
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type vm:{Entity}ViewModel}}, Path=GoToEditCommand}"
                                    CommandParameter="{Binding .}" />
                            </Frame.GestureRecognizers>

                            <VerticalStackLayout Spacing="4">
                                <Label Text="{Binding Name}" FontSize="16" FontAttributes="Bold" />
                                <Label Text="{Binding Description}" FontSize="13" TextColor="Gray" />
                            </VerticalStackLayout>
                        </Frame>
                    </SwipeView>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- FAB (Floating Action Button) -->
        <Button Grid.Row="1"
                Text="+"
                FontSize="24"
                WidthRequest="60"
                HeightRequest="60"
                CornerRadius="30"
                Margin="0,0,24,24"
                HorizontalOptions="End"
                VerticalOptions="End"
                BackgroundColor="{StaticResource Primary}"
                TextColor="White"
                Command="{Binding GoToAddCommand}" />
    </Grid>
</ContentPage>
```

#### 4b. Code-Behind — `{Entity}Page.xaml.cs`

```csharp
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views
{
    public partial class {Entity}Page : ContentPage
    {
        private readonly {Entity}ViewModel _viewModel;

        public {Entity}Page({Entity}ViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadItemsCommand.ExecuteAsync(null);
        }
    }
}
```

**Conventions:**
- Constructor receives ViewModel via DI, sets `BindingContext`
- `OnAppearing` reloads data (ensures fresh state after navigating back)
- `x:DataType` for compiled bindings
- `SwipeView` with delete swipe item (binds back to ViewModel via `RelativeSource AncestorType`)
- `TapGestureRecognizer` on `Frame` for edit navigation
- `CollectionView.EmptyView` for empty state
- FAB button positioned at bottom-right with `CornerRadius="30"`
- `AppThemeBinding` for light/dark mode support
- Add `xmlns:model="clr-namespace:FamilyFinance.Models"` if referencing model types in `DataTemplate`

---

### Step 5: Form View — `FamilyFinance/Views/{Entity}FormPage.xaml` + `.xaml.cs`

#### 5a. XAML — `{Entity}FormPage.xaml`

Follow the pattern from `AccountFormPage.xaml` or `PersonFormPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:FamilyFinance.ViewModels"
             x:Class="FamilyFinance.Views.{Entity}FormPage"
             x:DataType="vm:{Entity}ViewModel"
             Title="Add {Entity}"
             BackgroundColor="{AppThemeBinding Light=#F5F5F5, Dark=#1E1E1E}">

    <ScrollView Padding="20">
        <VerticalStackLayout Spacing="16">

            <Frame Padding="16" CornerRadius="12" HasShadow="True"
                   BackgroundColor="{AppThemeBinding Light=White, Dark=#2D2D2D}">
                <VerticalStackLayout Spacing="12">

                    <Label Text="Name" FontSize="14" FontAttributes="Bold" />
                    <Entry Text="{Binding Name}" Placeholder="Enter name" />

                    <Label Text="Description" FontSize="14" FontAttributes="Bold" />
                    <Editor Text="{Binding Description}" Placeholder="Enter description"
                            HeightRequest="100" AutoSize="TextChanges" />

                    <!-- Picker for FK relationships -->
                    <!--
                    <Label Text="Related Entity" FontSize="14" FontAttributes="Bold" />
                    <Picker Title="Select..."
                            ItemsSource="{Binding RelatedEntities}"
                            SelectedItem="{Binding SelectedRelatedEntity}"
                            ItemDisplayBinding="{Binding Name}" />
                    -->

                </VerticalStackLayout>
            </Frame>

            <Button Text="Save"
                    Command="{Binding SaveCommand}"
                    BackgroundColor="{StaticResource Primary}"
                    TextColor="White"
                    CornerRadius="8"
                    HeightRequest="48"
                    FontAttributes="Bold"
                    IsEnabled="{Binding IsProcessing, Converter={StaticResource InvertedBoolConverter}}" />

            <ActivityIndicator IsRunning="{Binding IsProcessing}"
                               IsVisible="{Binding IsProcessing}"
                               Color="{StaticResource Primary}" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

#### 5b. Code-Behind — `{Entity}FormPage.xaml.cs`

```csharp
using FamilyFinance.Models;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views
{
    [QueryProperty(nameof({Entity}), "{Entity}")]
    public partial class {Entity}FormPage : ContentPage
    {
        private readonly {Entity}ViewModel _viewModel;

        public {Entity}FormPage({Entity}ViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public {Entity}? {Entity}
        {
            set
            {
                if (value is not null)
                {
                    _viewModel.SetEditingItem(value);
                    Title = "Edit {Entity}";
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Load related data for pickers if needed:
            // await _viewModel.LoadFormDataCommand.ExecuteAsync(null);
        }
    }
}
```

**Conventions:**
- `[QueryProperty]` to receive navigation parameters (object-based)
- Property setter calls `_viewModel.SetEditingItem()` and updates page `Title`
- `OnAppearing` loads picker data if needed
- `ScrollView` wrapping form content
- Form fields inside a `Frame` with shadow
- Save button disabled during processing via `InvertedBoolConverter`
- `ActivityIndicator` shown while saving

---

### Step 6: DI Registration — Modify `FamilyFinance/MauiProgram.cs`

Add registrations in the appropriate sections:

```csharp
// ViewModels — transient
builder.Services.AddTransient<{Entity}ViewModel>();

// Pages — transient
builder.Services.AddTransient<{Entity}Page>();
builder.Services.AddTransient<{Entity}FormPage>();  // only if using separate form page
```

**Conventions:**
- Services are **singletons** (shared state, single DB connection)
- ViewModels are **transient** (fresh per navigation)
- Pages are **transient** (fresh per navigation)

---

### Step 7: Navigation — Modify `FamilyFinance/AppShell.xaml` + `.xaml.cs`

#### 7a. Add FlyoutItem — `AppShell.xaml`

Add a new `FlyoutItem` inside the `Shell` element, following existing order:

```xml
<FlyoutItem Title="{Entity}s" Icon="{entity}_icon.png">
    <ShellContent ContentTemplate="{DataTemplate views:{Entity}Page}"
                  Route="{Entity}Page" />
</FlyoutItem>
```

#### 7b. Register Sub-Routes — `AppShell.xaml.cs`

If using a separate form page, register the route in the constructor:

```csharp
Routing.RegisterRoute(nameof({Entity}FormPage), typeof({Entity}FormPage));
```

#### 7c. Add Flyout Icon

Create an SVG icon at `FamilyFinance/Resources/Images/{entity}_icon.svg`. MAUI auto-compiles SVGs to PNGs at build time. Reference as `.png` in XAML.

---

### Step 8: Resources (optional)

#### Custom Value Converter — `FamilyFinance/Helpers/{Name}Converter.cs`

Follow the `Base64ToImageConverter` pattern:

```csharp
using System.Globalization;

namespace FamilyFinance.Helpers
{
    public class {Name}Converter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Convert logic
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
```

Register as a page-level resource in XAML:
```xml
<ContentPage.Resources>
    <helpers:{Name}Converter x:Key="{Name}Converter" />
</ContentPage.Resources>
```

Or globally in `App.xaml` if used across multiple pages.

---

## Inline Form Alternative (AccountType Pattern)

For simpler entities that don't need a separate form page, use the `AccountTypeViewModel` / `AccountTypePage` pattern:

- Add `[ObservableProperty] bool _isFormVisible;` and `string _formTitle;` to ViewModel
- Add show/hide form commands instead of navigation
- In XAML, add a `Frame` with `IsVisible="{Binding IsFormVisible}"` at the bottom of the list
- **No separate form page needed** — no route registration, no `FormPage.xaml`

Use this pattern when the entity has 2-3 simple fields.

---

## Checklist

| # | Layer | Action | File |
|---|-------|--------|------|
| 1 | Model | Create | `FamilyFinance/Models/{Entity}.cs` |
| 2 | Service | Modify | `FamilyFinance/Services/DatabaseService.cs` — add table + CRUD |
| 3 | ViewModel | Create | `FamilyFinance/ViewModels/{Entity}ViewModel.cs` |
| 4 | View (List) | Create | `FamilyFinance/Views/{Entity}Page.xaml` + `.xaml.cs` |
| 5 | View (Form) | Create | `FamilyFinance/Views/{Entity}FormPage.xaml` + `.xaml.cs` |
| 6 | DI | Modify | `FamilyFinance/MauiProgram.cs` — register ViewModel + Pages |
| 7 | Navigation | Modify | `FamilyFinance/AppShell.xaml` — add FlyoutItem |
| 8 | Navigation | Modify | `FamilyFinance/AppShell.xaml.cs` — register form route |
| 9 | Icon | Create | `FamilyFinance/Resources/Images/{entity}_icon.svg` |

## Response Guidelines

1. **Read existing files first** to match current patterns exactly (use `Account` as primary reference)
2. **Follow the order** — Model → Service → ViewModel → Views → DI → Navigation
3. **Use `Account`** as the reference for full CRUD with separate form page
4. **Use `AccountType`** as the reference for inline form (simpler entities)
5. **Match conventions**: `[ObservableProperty]` fields are `_camelCase`, commands are `[RelayCommand]` async methods
6. **SQLite**: All operations async, `Id == 0` means insert, `[Ignore]` on nav properties
7. **Light/Dark mode**: Always use `AppThemeBinding` for colors
8. **Delete strategy**: Unlink dependents (set FK to null) rather than cascade delete
9. **Navigation**: Use `Shell.Current.GoToAsync` for forward, `".."` for back
10. **Guard saves**: Use `IsProcessing` flag to prevent double-submissions
