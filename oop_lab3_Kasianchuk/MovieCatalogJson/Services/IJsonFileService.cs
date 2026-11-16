using MovieCatalogJson.Models;

namespace MovieCatalogJson.Services;

public interface IJsonFileService
{
    Task<List<Movie>> LoadMoviesAsync(string filePath);
    Task SaveMoviesAsync(string filePath, IEnumerable<Movie> movies);
}