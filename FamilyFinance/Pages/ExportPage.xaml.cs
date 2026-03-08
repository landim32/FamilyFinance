using FamilyFinance.ViewModels;

namespace FamilyFinance.Pages;

public partial class ExportPage : ContentPage
{
    private readonly ExportViewModel _viewModel;

    public ExportPage(ExportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadPeopleAsync();
    }
}
