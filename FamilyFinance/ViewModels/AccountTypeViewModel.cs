using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.DTOs;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.ViewModels;

public partial class AccountTypeViewModel : ObservableObject
{
    private readonly IAccountTypeRepository _accountTypeRepo;
    private readonly IMapper _mapper;

    public AccountTypeViewModel(IAccountTypeRepository accountTypeRepo, IMapper mapper)
    {
        _accountTypeRepo = accountTypeRepo;
        _mapper = mapper;
    }

    [ObservableProperty]
    private ObservableCollection<AccountTypeInfo> accountTypes = new();

    [ObservableProperty]
    private string typeName = string.Empty;

    [ObservableProperty]
    private string? typeDescription;

    [ObservableProperty]
    private AccountTypeInfo? editingType;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    private string formTitle = "Add Account Type";

    [RelayCommand]
    public async Task LoadAccountTypesAsync()
    {
        var list = await _accountTypeRepo.GetAllAsync();
        AccountTypes = new ObservableCollection<AccountTypeInfo>(_mapper.Map<List<AccountTypeInfo>>(list));
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
    public void ShowEditForm(AccountTypeInfo accountType)
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

        try
        {
            if (EditingType is not null)
            {
                var entity = await _accountTypeRepo.GetByIdAsync(EditingType.Id);
                if (entity is null) return;
                entity.Update(TypeName.Trim(), TypeDescription);
                var error = entity.Validate();
                if (error != null) { await Shell.Current.DisplayAlert("Error", error, "OK"); return; }
                await _accountTypeRepo.SaveAsync(entity);
            }
            else
            {
                var entity = AccountType.Create(TypeName.Trim(), TypeDescription);
                await _accountTypeRepo.SaveAsync(entity);
            }

            CancelForm();
            await LoadAccountTypesAsync();
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteAccountTypeAsync(AccountTypeInfo accountType)
    {
        var linkedCount = await _accountTypeRepo.GetAccountCountAsync(accountType.Id);

        if (linkedCount > 0)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Warning",
                $"This account type has {linkedCount} linked account(s). Are you sure you want to delete it?",
                "Delete", "Cancel");

            if (!confirm)
                return;
        }

        await _accountTypeRepo.DeleteAsync(accountType.Id);
        await LoadAccountTypesAsync();
    }
}
