using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieCatalogJson.Models;

namespace MovieCatalogJson.ViewModels;

public partial class MovieEditorViewModel : ObservableObject
{
    private const int MinYear = 1888;
    private const double MaxRating = 10;

    private readonly TaskCompletionSource<Movie?> _resultSource = new();

    public Movie CurrentMovie { get; }
    public bool IsEditMode { get; }
    public string PageTitle => IsEditMode ? "Редагування фільму" : "Новий фільм";

    /// <summary>Завершується збереженим фільмом або null, якщо користувач скасував.</summary>
    public Task<Movie?> Result => _resultSource.Task;

    public MovieEditorViewModel(Movie? source = null)
    {
        IsEditMode = source is not null;

        // Працюємо з копією, щоб «Скасувати» не змінювало оригінал
        CurrentMovie = source is null
            ? new Movie { ReleaseYear = DateTime.Now.Year }
            : new Movie
            {
                Title = source.Title,
                Director = source.Director,
                ReleaseYear = source.ReleaseYear,
                Genre = source.Genre,
                Rating = source.Rating
            };
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Validate(out string error))
        {
            await Shell.Current.DisplayAlert("Помилка", error, "OK");
            return;
        }

        CurrentMovie.Title = CurrentMovie.Title.Trim();
        CurrentMovie.Director = CurrentMovie.Director.Trim();
        CurrentMovie.Genre = CurrentMovie.Genre.Trim();

        _resultSource.TrySetResult(CurrentMovie);
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        _resultSource.TrySetResult(null);
        await Shell.Current.Navigation.PopModalAsync();
    }

    private bool Validate(out string error)
    {
        int maxYear = DateTime.Now.Year + 5;

        if (string.IsNullOrWhiteSpace(CurrentMovie.Title))
            error = "Вкажіть назву фільму.";
        else if (string.IsNullOrWhiteSpace(CurrentMovie.Director))
            error = "Вкажіть режисера.";
        else if (string.IsNullOrWhiteSpace(CurrentMovie.Genre))
            error = "Вкажіть жанр.";
        else if (CurrentMovie.ReleaseYear < MinYear || CurrentMovie.ReleaseYear > maxYear)
            error = $"Рік має бути від {MinYear} до {maxYear}.";
        else if (CurrentMovie.Rating < 0 || CurrentMovie.Rating > MaxRating)
            error = $"Рейтинг має бути від 0 до {MaxRating}.";
        else
        {
            error = string.Empty;
            return true;
        }

        return false;
    }
}