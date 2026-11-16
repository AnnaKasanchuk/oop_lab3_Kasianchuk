using MovieCatalogJson.ViewModels;

namespace MovieCatalogJson;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}