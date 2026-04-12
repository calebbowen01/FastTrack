using MyMauiApp.ViewModels;

namespace MyMauiApp.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
