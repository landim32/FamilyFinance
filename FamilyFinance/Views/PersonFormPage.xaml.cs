using FamilyFinance.DTOs;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

[QueryProperty(nameof(PersonInfo), "PersonInfo")]
public partial class PersonFormPage : ContentPage
{
    private readonly PersonViewModel _viewModel;

    public PersonFormPage(PersonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public PersonInfo? PersonInfo
    {
        set
        {
            if (value is not null)
            {
                _viewModel.SetEditingPerson(value);
                Title = "Edit Person";
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadFormData();
    }
}
