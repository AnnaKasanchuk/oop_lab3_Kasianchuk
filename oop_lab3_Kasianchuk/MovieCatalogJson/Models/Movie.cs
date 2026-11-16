namespace MovieCatalogJson.Models;

public class Movie
{
    public string Title { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public double Rating { get; set; }
}