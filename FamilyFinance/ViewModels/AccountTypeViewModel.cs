using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.ViewModels;

public partial class AccountTypeViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    public AccountTypeViewModel(DatabaseService db)
    {
        _db = db;
    }

    [ObservableProperty]
    private ObservableCollection<AccountType> accountTypes = new();

    [ObservableProperty]
    private string typeName = string.Empty;

    [ObservableProperty]
    private string? typeDescription;

    [ObservableProperty]
    private AccountType? editingType;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    private string formTitle = "Add Account Type";

    [RelayCommand]
    public async Task LoadAccountTypesAsync()
    {
        var list = await _db.GetAccountTypesAsync();
        AccountTypes = new ObservableCollection<AccountType>(list);
    }

    [RelayCommand]
    public void ShowAddForm()
    {
        EditingType = null;
        TypeName = string.Empty;
        TypeDescription = null;
        FormTitle = "Add Account Type";
        IsFormVisible = true;
    }

    [RelayCommand]
    public void ShowEditForm(AccountType accountType)
    {
        EditingType = accountType;
        TypeName = accountType.Name;
        TypeDescription = accountType.Description;
        FormTitle = "Edit Account Type";
        IsFormVisible = true;
    }

    [RelayCommand]
    public void CancelForm()
    {
        IsFormVisible = false;
        EditingType = null;
        TypeName = string.Empty;
        TypeDescription = null;
    }

    [RelayCommand]
    public async Task SaveAccountTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(TypeName))
        {
            await Shell.Current.DisplayAlert("Validation", "Name is required.", "OK");
            return;
        }

        var accountType = EditingType ?? new AccountType();
        accountType.Name = TypeName.Trim();
        accountType.Description = TypeDescription;

        await _db.SaveAccountTypeAsync(accountType);
        CancelForm();
        await LoadAccountTypesAsync();
    }

    [RelayCommand]
    public async Task DeleteAccountTypeAsync(AccountType accountType)
    {
        var linkedCount = await _db.GetAccountCountByTypeAsync(accountType.Id);

        if (linkedCount > 0)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Warning",
                $"This account type has {linkedCount} linked account(s). Are you sure you want to delete it?",
                "Delete", "Cancel");

            if (!confirm)
                return;
        }

        await _db.DeleteAccountTypeAsync(accountType);
        await LoadAccountTypesAsync();
    }
}
