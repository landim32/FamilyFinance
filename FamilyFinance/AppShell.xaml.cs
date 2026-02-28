using FamilyFinance.Views;

namespace FamilyFinance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(AccountFormPage), typeof(AccountFormPage));
        Routing.RegisterRoute(nameof(PersonFormPage), typeof(PersonFormPage));
    }
}
