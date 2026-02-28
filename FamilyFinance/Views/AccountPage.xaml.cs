using FamilyFinance.Models;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

public partial class AccountPage : ContentPage
{
    private readonly AccountViewModel _viewModel;

    public AccountPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAccountsAsync();
    }

    private async void OnAccountSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Account account)
        {
            await _viewModel.GoToEditAccountAsync(account);

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
    }
}
