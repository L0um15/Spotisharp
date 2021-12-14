﻿using SpotifyAPI.Web;

namespace Spotisharp.Client.Models;

public class TrackInfoModel
{
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Album { get; set; }
    public string AlbumPicture { get; set; }
    public string Copyright { get; set; }
    public string Genres { get; set; }
    public string Playlist { get; set; }
    public string Url { get; set; }
    public string Date { get; set; }
    public int DiscNumber { get; set; }
    public int TrackNumber { get; set; }

    public override string ToString()
    {
        return $"Track Information: \n" +
            $"\t{Artist}\n" +
            $"\t{Title}\n" +
            $"\t{Album}\n" +
            $"\t{Copyright}\n" +
            $"\t{Genres}\n" +
            $"\t{Url}\n" +
            $"\t{Date}\n" +
            $"\t{DiscNumber}\n" +
            $"\t{TrackNumber}\n";    
    }
}