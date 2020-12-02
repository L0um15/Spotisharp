# SpotiSharp

![RELEASES](https://img.shields.io/github/v/release/L0um15/SpotiSharp?include_prereleases&style=flat-square)
![HEART](https://img.shields.io/static/v1?label=made+with&message=❤&color=red&style=flat-square)
![LICENSE](https://img.shields.io/github/license/L0um15/SpotiSharp?style=flat-square)

![DOWNLOADS](https://img.shields.io/github/downloads/L0um15/SpotiSharp/total?color=%23FF&label=Downloads&style=flat-square)
![ISSUES](https://img.shields.io/github/issues/L0um15/SpotiSharp?style=flat-square)
![LAST COMMIT](https://img.shields.io/github/last-commit/L0um15/SpotiSharp?style=flat-square)

<img align="right" src=".github/images/icon.png" width="128px" height="128px" />

## Music Downloader using Spotify Web API

SpotiSharp applies metadata like `Artist` `Title` `Lyrics` `Genres` `Album` `AlbumArt` and more...<br />
Also supports Spotify Url's

Linux users: [README-LINUX](README-LINUX.md)

### Usage

**KEEP THESE KEYS PRIVATE, DO NOT SHARE THEM TO ANYONE**

Create Application on [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/) then provide these keys to config.json

        - ClientID
        - Client Secret

Config.json will be generated on first run with empty fields.
```json
{
    "CLIENTID": "",
    "SECRETID": "" 
}
```

FFmpeg is required, but SpotiSharp will download latest binary by itself to the application folder.

SpotiSharp is very easy to use...
```sh
.\SpotiSharp.exe "<Text Search/Spotify URL>"
```
... and thats it. Without any complicated flags, prefixes etc.

SpotiSharp will automatically download all tracks to C:\Users\YOU\MyMusic\SpotiSharp\

### Single Track

<img src=".github/images/preview.png"/><br />

### Playlist

<img src=".github/images/preview2.png"/><br />

### Result

<img src=".github/images/result.png" />

Screenshot was taken from my [Clair Musicplayer](https://github.com/L0um15/Clair-Musicplayer)

