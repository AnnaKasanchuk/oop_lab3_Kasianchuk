using System.Text.Encodings.Web;
using System.Text.Json;
using MovieCatalogJson.Models;

namespace MovieCatalogJson.Services;

public class JsonFileService : IJsonFileService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<Movie>> LoadMoviesAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<Movie>>(stream, _options) 
               ?? new List<Movie>();
    }

    public async Task SaveMoviesAsync(string filePath, IEnumerable<Movie> movies)
    {
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, movies, _options);
    }
}