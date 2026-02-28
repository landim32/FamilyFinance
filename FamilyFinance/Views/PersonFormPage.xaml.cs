using FamilyFinance.Models;
using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

[QueryProperty(nameof(Person), "Person")]
public partial class PersonFormPage : ContentPage
{
    private readonly PersonViewModel _viewModel;

    public PersonFormPage(PersonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public Person? Person
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
