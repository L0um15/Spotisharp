<p align=center>
    <img height=256 src=".github/images/spotisharpv3icon.png">
</p>

<h2 align=center>Spotisharp V3</h2>
<p align=center>Cross-platform music assistant</p>

```zsh
 ~/docs > cat ./Spotisharp.md
```

<p align=center>
    <img src=".github/images/screenshot1.png">
</p>

<p align=center>
    <img src=".github/images/screenshot2.png">
</p>

### Prerequisites
 - Dotnet runtime 6.0 or newer
 - FFmpeg

### Authentication

Unlike previous versions of Spotisharp, version 3.0 uses now your personal account in order to retrieve information from spotify, thus you're not required to create application in dashboard anymore.

Spotisharp will open a webpage with authentication request. When granted access, spotisharp will cache retrieved `refresh token` inside `config` folder in order to skip this step on next use.

If `refresh token` has been corrupted/expired, spotisharp will ask you again for access to your account.

### Configuration

Version 3.0 stores configuration files inside `.config/spotisharp` located in `home` directory.

Like previous versions, configuration file will be updated with each new update by adding or removing entries when needed.

Editing values inside "VersionControl" triggers configuration update.
```json
{
  "WorkersCount": 2, // How many workers should be hired. Default: 2 of 4
  "VersionControl": "3.0.0.0", // Spotisharp updates config if values mismatch with app version
  "MusicDirectory": "C:\\Users\\Damian\\Music\\Spotisharp" // Default download dir
}
```

### Usage:

There are now two ways to use Spotisharp. Either by double-clicking executable or by passing an argument via the console.

Spotisharp accepts following inputs:
 - Text like: Lyn - Wake Up, Get Up, Get Out There
 - Url's like: https://open.spotify.com/track/4AuZBIN4aeFL9egQldQfRn
 - Uri's like: spotify:track:4AuZBIN4aeFL9egQldQfRn

Sometimes url will look like this: https://open.spotify.com/track/4AuZBIN4aeFL9egQldQfRn?si=3277af7219054b98 and this is fine too.

Spotisharp is able to work with tracks, playlists, and albums.

Passing argument to spotisharp looks like this:
```zsh
~/binaries > echo 'Use quotes when passing argument via console, otherwise everything after whitespaces will be ignored'
~/binaries > ./Spotisharp "Lyn - Wake Up, Get Up, Get Out There"
```
```zsh
~/binaries > ./Spotisharp https://open.spotify.com/track/4AuZBIN4aeFL9egQldQfRn
```
```zsh
~/binaries > ./Spotisharp spotify:track:4AuZBIN4aeFL9egQldQfRn
```
If no argument is provided, spotisharp will await for user input.