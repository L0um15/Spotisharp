using SpotifyAPI.Web;

namespace Spotisharp.Client.Models;

public class TrackInfoModel
{
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string AlbumPicture { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty; 
    public string Playlist { get; set; } = string.Empty; 
    public string Url { get; set; } = string.Empty; 
    public int Year { get; set; }
    public int DiscNumber { get; set; }
    public int TrackNumber { get; set; }

    public override string ToString()
    {
        return $"{Artist} - {Title} | {Url}";   
    }
}
