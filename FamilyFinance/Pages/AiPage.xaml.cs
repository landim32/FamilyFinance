using FamilyFinance.ViewModels;

namespace FamilyFinance.Pages;

public partial class AiPage : ContentPage
{
    public AiPage(AiViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
