using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.DTOs;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Views;

namespace FamilyFinance.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly IAccountRepository _accountRepo;
    private readonly IPersonRepository _personRepo;
    private readonly IAccountTypeRepository _accountTypeRepo;
    private readonly IMapper _mapper;

    public AccountViewModel(
        IAccountRepository accountRepo,
        IPersonRepository personRepo,
        IAccountTypeRepository accountTypeRepo,
        IMapper mapper)
    {
        _accountRepo = accountRepo;
        _personRepo = personRepo;
        _accountTypeRepo = accountTypeRepo;
        _mapper = mapper;
    }

    [ObservableProperty]
    private ObservableCollection<AccountInfo> accounts = new();

    [ObservableProperty]
    private ObservableCollection<AccountInfo> filteredAccounts = new();

    [ObservableProperty]
    private decimal totalBalance;

    [ObservableProperty]
    private Color balanceColor = Colors.Gray;

    [ObservableProperty]
    private string filterMode = "All";

    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        var list = await _accountRepo.GetAllAsync();
        var people = await _personRepo.GetAllAsync();
        var types = await _accountTypeRepo.GetAllAsync();

        var peopleLookup = people.ToDictionary(p => p.Id);
        var typesLookup = types.ToDictionary(t => t.Id);

        var infoList = list.Select(a =>
        {
            var info = _mapper.Map<AccountInfo>(a);
            if (a.PersonId.HasValue && peopleLookup.TryGetValue(a.PersonId.Value, out var person))
                info.PersonName = person.Name;
            if (a.AccountTypeId.HasValue && typesLookup.TryGetValue(a.AccountTypeId.Value, out var type))
                info.AccountTypeName = type.Name;
            return info;
        }).ToList();

        Accounts = new ObservableCollection<AccountInfo>(infoList);
        ApplyFilter();
        CalculateBalance();
    }

    [RelayCommand]
    public void SetFilter(string mode)
    {
        FilterMode = mode;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = FilterMode switch
        {
            "Credit" => Accounts.Where(a => a.IsCredit).ToList(),
            "Debit" => Accounts.Where(a => !a.IsCredit).ToList(),
            _ => Accounts.ToList()
        };
        FilteredAccounts = new ObservableCollection<AccountInfo>(filtered);
    }

    private void CalculateBalance()
    {
        var credits = Accounts.Where(a => a.IsCredit).Sum(a => a.Amount);
        var debits = Accounts.Where(a => !a.IsCredit).Sum(a => a.Amount);
        TotalBalance = credits - debits;

        BalanceColor = TotalBalance > 0 ? Colors.Green
                     : TotalBalance < 0 ? Colors.Red
                     : Colors.Gray;
    }

    [RelayCommand]
    public async Task DeleteAccountAsync(AccountInfo account)
    {
        await _accountRepo.DeleteAsync(account.Id);
        await LoadAccountsAsync();
    }

    [RelayCommand]
    public async Task GoToAddAccountAsync()
    {
        await Shell.Current.GoToAsync(nameof(AccountFormPage));
    }

    [RelayCommand]
    public async Task GoToEditAccountAsync(AccountInfo account)
    {
        await Shell.Current.GoToAsync(nameof(AccountFormPage), new Dictionary<string, object>
        {
            { "AccountInfo", account }
        });
    }

    // ---- Form properties for AccountFormPage ----

    [ObservableProperty]
    private AccountInfo? editingAccount;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string amountText = string.Empty;

    [ObservableProperty]
    private bool isCredit = true;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private ObservableCollection<PersonInfo> people = new();

    [ObservableProperty]
    private PersonInfo? selectedPerson;

    [ObservableProperty]
    private ObservableCollection<AccountTypeInfo> accountTypes = new();

    [ObservableProperty]
    private AccountTypeInfo? selectedAccountType;

    [RelayCommand]
    public async Task LoadFormDataAsync()
    {
        var personModels = await _personRepo.GetAllAsync();
        var typeModels = await _accountTypeRepo.GetAllAsync();
        People = new ObservableCollection<PersonInfo>(_mapper.Map<List<PersonInfo>>(personModels));
        AccountTypes = new ObservableCollection<AccountTypeInfo>(_mapper.Map<List<AccountTypeInfo>>(typeModels));

        if (EditingAccount is not null)
        {
            Title = EditingAccount.Title;
            AmountText = EditingAccount.Amount.ToString("F2");
            IsCredit = EditingAccount.IsCredit;
            Notes = EditingAccount.Notes;
            SelectedPerson = People.FirstOrDefault(p => p.Id == EditingAccount.PersonId);
            SelectedAccountType = AccountTypes.FirstOrDefault(t => t.Id == EditingAccount.AccountTypeId);
        }
    }

    public void SetEditingAccount(AccountInfo account)
    {
        EditingAccount = account;
    }

    [RelayCommand]
    public async Task SaveAccountAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Validation", "Title is required.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0)
        {
            await Shell.Current.DisplayAlert("Validation", "Amount must be greater than zero.", "OK");
            return;
        }

        try
        {
            if (EditingAccount is not null)
            {
                var entity = await _accountRepo.GetByIdAsync(EditingAccount.Id);
                if (entity is null) return;
                entity.Update(Title.Trim(), amount, IsCredit, Notes, SelectedPerson?.Id, SelectedAccountType?.Id);
                var error = entity.Validate();
                if (error != null) { await Shell.Current.DisplayAlert("Error", error, "OK"); return; }
                await _accountRepo.SaveAsync(entity);
            }
            else
            {
                var entity = Account.Create(Title.Trim(), amount, IsCredit, Notes, SelectedPerson?.Id, SelectedAccountType?.Id);
                await _accountRepo.SaveAsync(entity);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
