using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieCatalogJson.Models;
using MovieCatalogJson.Services;
using MovieCatalogJson.Views;

namespace MovieCatalogJson.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string AllGenres = "Усі";

    private readonly IJsonFileService _jsonService;
    private List<Movie> _allMovies = new();
    private string? _currentFilePath;

    [ObservableProperty] private ObservableCollection<Movie> _displayedMovies = new();
    [ObservableProperty] private ObservableCollection<string> _genres = new() { AllGenres };
    [ObservableProperty] private Movie? _selectedMovie;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private string? _selectedGenre = AllGenres;
    [ObservableProperty] private double _minRating;

    public MainViewModel(IJsonFileService jsonService)
    {
        _jsonService = jsonService;
    }

    // ---------- Файл ----------

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var jsonFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.MacCatalyst, new[] { "public.json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.iOS, new[] { "public.json" } }
        });

        FileResult? result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Оберіть JSON-файл каталогу фільмів",
            FileTypes = jsonFileType
        });

        if (result is null)
        {
            return;
        }

        try
        {
            List<Movie> movies = await _jsonService.LoadMoviesAsync(result.FullPath);
            _currentFilePath = result.FullPath;
            _allMovies = movies;
            RefreshGenres();
            FilterMoviesWithLinq();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveFileAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            await Shell.Current.DisplayAlert("Збереження", "Спочатку відкрийте JSON-файл.", "OK");
            return;
        }

        try
        {
            await _jsonService.SaveMoviesAsync(_currentFilePath, _allMovies);
            await Shell.Current.DisplayAlert("Збереження", "Зміни збережено у файл.", "OK");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await Shell.Current.DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
        }
    }

    // ---------- Додати / редагувати / видалити ----------

    [RelayCommand]
    private async Task AddMovieAsync()
    {
        var editor = new MovieEditorViewModel();
        await Shell.Current.Navigation.PushModalAsync(new MovieEditorPage(editor));

        Movie? created = await editor.Result;
        if (created is null)
        {
            return;
        }

        _allMovies.Add(created);
        RefreshGenres();
        FilterMoviesWithLinq();
    }

    [RelayCommand]
    private async Task EditMovieAsync()
    {
        Movie? original = SelectedMovie;
        if (original is null)
        {
            await Shell.Current.DisplayAlert("Підказка", "Спочатку оберіть фільм у таблиці.", "OK");
            return;
        }

        var editor = new MovieEditorViewModel(original);
        await Shell.Current.Navigation.PushModalAsync(new MovieEditorPage(editor));

        Movie? edited = await editor.Result;
        if (edited is null)
        {
            return;
        }

        int index = _allMovies.IndexOf(original);
        if (index >= 0)
        {
            _allMovies[index] = edited;
        }

        RefreshGenres();
        FilterMoviesWithLinq();
    }

    [RelayCommand]
    private async Task DeleteMovieAsync()
    {
        Movie? selected = SelectedMovie;
        if (selected is null)
        {
            await Shell.Current.DisplayAlert("Підказка", "Спочатку оберіть фільм у таблиці.", "OK");
            return;
        }

        bool confirmed = await Shell.Current.DisplayAlert(
            "Підтвердження", $"Видалити фільм «{selected.Title}»?", "Так", "Ні");
        if (!confirmed)
        {
            return;
        }

        _allMovies.Remove(selected);
        RefreshGenres();
        FilterMoviesWithLinq();
    }

    [RelayCommand]
    private async Task OpenAboutAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new AboutPage());
    }

    // ---------- Пошук (LINQ) ----------

    [RelayCommand]
    private void ApplyLinqFilter() => FilterMoviesWithLinq();

    private void FilterMoviesWithLinq()
    {
        IEnumerable<Movie> query = _allMovies;

        // 1. Пошук за назвою або режисером
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            query = query.Where(m =>
                m.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                m.Director.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        // 2. Фільтрація за жанром
        if (!string.IsNullOrWhiteSpace(SelectedGenre) && SelectedGenre != AllGenres)
        {
            query = query.Where(m => m.Genre.Equals(SelectedGenre, StringComparison.OrdinalIgnoreCase));
        }

        // 3. Фільтрація за мінімальним рейтингом
        if (MinRating > 0)
        {
            query = query.Where(m => m.Rating >= MinRating);
        }

        DisplayedMovies = new ObservableCollection<Movie>(query);
    }

    /// <summary>Перебудовує список жанрів для Picker за поточними даними.</summary>
    private void RefreshGenres()
    {
        string? current = SelectedGenre;

        var genres = _allMovies
            .Select(m => m.Genre)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .ToList();
        genres.Insert(0, AllGenres);

        Genres = new ObservableCollection<string>(genres);
        SelectedGenre = current is not null && genres.Contains(current) ? current : AllGenres;
    }
}