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


### How do i use your program? What is the console, where i cant find it, im confused.

Dont worry, you can use spotisharp just like regular program, just open it and type the title of your favourite song or give it a url.


### Commandline parsing

Version 3.1 now features commandline parsing which helps to change configuration on the fly!

All available flags can be seen by typing:

```bash
./spotisharp --help
```

This are example usages:

```zsh
./spotisharp -i "Lyn - Wake Up, Get Up, Get out there" --workers 2 --check-updates true -o "C:\\Users\\user\\Music\\Spotisharp"
```

```zsh
./spotisharp -i https://open.spotify.com/track/4AuZBIN4aeFL9egQldQfRn --workers 4 --check-updates false"
```

```zsh
./spotisharp -i spotify:track:4AuZBIN4aeFL9egQldQfRn --check-updates false"
```

`--keep-changes true` this flag allows you to save all arguments (with some minor exceptions) to the config file and are treated as default on next launch.

Keep in mind that this flag will overwrite your configuration.

if `-i | --input` flag is missing, spotisharp will wait for your keyboard input.


### Configuration

From the version 3.0, spotisharp stores configuration files inside `.config/spotisharp` located in `home` directory.

Like previous versions, configuration file will be updated with each new update by adding or removing entries when needed.

Editing values inside "VersionControl" triggers configuration update.
```json
{
  "WorkersCount": 2, // How many workers should be hired. Default: 2 of 4
  "VersionControl": "3.0.0.0", // Spotisharp updates config if values mismatch with app version
  "OutDir": "C:\\Users\\Damian\\Music\\Spotisharp" // Default download dir
}
```

