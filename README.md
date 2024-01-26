This is a personal project which handles backing up Project Zomboid saves into a ZIP file.

Future plans/research:

- A GUI for managing backups.
- Run on a schedule, detect when a Project Zomboid save is no longer file-locked, then back it up. (Not sure if this is possible yet)

## Requirements

All releases are built for x64 Windows machines. They are self contained and single file, therefore no dependencies are required.

## Development Requirements

This project was written in .NET 8, which requires the [.NET SDK](https://dotnet.microsoft.com/en-us/download).

## How to use

Powershell: `.\McKinley.ProjectZomboid.Backups.Runner.exe --help`

CMD: `McKinley.ProjectZomboid.Backups.Runner.exe --help`

```
  -t, --type              Backup file type. Zip and TarZLib are supported.

  -o, --output            Backup output folder. By default the current directory will be used.

  -s, --save-directory    Project Zomboid Saves Directory. If not provided, it will try to detect your default Project
                          Zomboid saves directory (ex: %USERPROFILE%\Zomboid\Saves\Sandbox).

  --save-name             Only back up this save, ignores all others.

  --log-level             The log level to output to the console.

  --zip-filename          Zip backup file name

  --compression-level     Compression Level (Optimal/Fastest/NoCompression/SmallestSize). The default is Optimal.

  --help                  Display this help screen.

  --version               Display version information.
```