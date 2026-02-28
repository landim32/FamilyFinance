using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Views;

namespace FamilyFinance.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    public AccountViewModel(DatabaseService db)
    {
        _db = db;
    }

    [ObservableProperty]
    private ObservableCollection<Account> accounts = new();

    [ObservableProperty]
    private ObservableCollection<Account> filteredAccounts = new();

    [ObservableProperty]
    private decimal totalBalance;

    [ObservableProperty]
    private Color balanceColor = Colors.Gray;

    [ObservableProperty]
    private string filterMode = "All";

    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        var list = await _db.GetAccountsAsync();
        Accounts = new ObservableCollection<Account>(list);
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
        FilteredAccounts = new ObservableCollection<Account>(filtered);
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
    public async Task DeleteAccountAsync(Account account)
    {
        await _db.DeleteAccountAsync(account);
        await LoadAccountsAsync();
    }

    [RelayCommand]
    public async Task GoToAddAccountAsync()
    {
        await Shell.Current.GoToAsync(nameof(AccountFormPage));
    }

    [RelayCommand]
    public async Task GoToEditAccountAsync(Account account)
    {
        await Shell.Current.GoToAsync(nameof(AccountFormPage), new Dictionary<string, object>
        {
            { "Account", account }
        });
    }

    // ---- Form properties for AccountFormPage ----

    [ObservableProperty]
    private Account? editingAccount;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string amountText = string.Empty;

    [ObservableProperty]
    private bool isCredit = true;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private ObservableCollection<Person> people = new();

    [ObservableProperty]
    private Person? selectedPerson;

    [ObservableProperty]
    private ObservableCollection<AccountType> accountTypes = new();

    [ObservableProperty]
    private AccountType? selectedAccountType;

    [RelayCommand]
    public async Task LoadFormDataAsync()
    {
        People = new ObservableCollection<Person>(await _db.GetPeopleAsync());
        AccountTypes = new ObservableCollection<AccountType>(await _db.GetAccountTypesAsync());

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

    public void SetEditingAccount(Account account)
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

        var account = EditingAccount ?? new Account();
        account.Title = Title.Trim();
        account.Amount = amount;
        account.IsCredit = IsCredit;
        account.Notes = Notes;
        account.PersonId = SelectedPerson?.Id;
        account.AccountTypeId = SelectedAccountType?.Id;

        if (account.Id == 0)
            account.CreatedAt = DateTime.Now;

        await _db.SaveAccountAsync(account);
        await Shell.Current.GoToAsync("..");
    }
}
