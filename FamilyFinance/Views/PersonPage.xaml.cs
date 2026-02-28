using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

public partial class PersonPage : ContentPage
{
    private readonly PersonViewModel _viewModel;

    public PersonPage(PersonViewModel viewModel)
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

    private async void OnPersonSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PersonDisplay personDisplay)
        {
            await _viewModel.GoToEditPersonAsync(personDisplay);

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
    }
}
