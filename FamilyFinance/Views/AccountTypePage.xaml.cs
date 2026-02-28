using FamilyFinance.Models;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

public partial class AccountTypePage : ContentPage
{
    private readonly AccountTypeViewModel _viewModel;

    public AccountTypePage(AccountTypeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAccountTypesAsync();
    }

    private void OnTypeSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AccountType accountType)
        {
            _viewModel.ShowEditForm(accountType);

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
    }
}
