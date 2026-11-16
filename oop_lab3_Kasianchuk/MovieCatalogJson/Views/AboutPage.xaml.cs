namespace MovieCatalogJson.Views;

public partial class AboutPage : ContentPage
{
    public string StudentInfo { get; } =
        "ПІБ: Кас'янчук Анна\nКурс: 2\nГрупа: К-25\nРік: 2026";

    public string Description { get; } =
        "Програма відкриває JSON-файл із каталогом фільмів, показує дані у таблиці, " +
        "дозволяє додавати, редагувати, видаляти записи та шукати їх за допомогою LINQ.";

    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}