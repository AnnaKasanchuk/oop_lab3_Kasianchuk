using MovieCatalogJson.ViewModels;

namespace MovieCatalogJson.Views;

public partial class MovieEditorPage : ContentPage
{
    public MovieEditorPage(MovieEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}