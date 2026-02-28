using FamilyFinance.ViewModels;

namespace FamilyFinance.Views;

public partial class AiPage : ContentPage
{
    public AiPage(AiViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
