using FamilyFinance.Models;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

[QueryProperty(nameof(Account), "Account")]
public partial class AccountFormPage : ContentPage
{
    private readonly AccountViewModel _viewModel;

    public AccountFormPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public Account? Account
    {
        set
        {
            if (value is not null)
            {
                _viewModel.SetEditingAccount(value);
                Title = "Edit Account";
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadFormDataAsync();
    }
}
