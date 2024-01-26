This is a personal project which handles backing up Project Zomboid saves into a ZIP file.

Future plans/research:

- A GUI for managing backups.
- Run on a schedule, detect when a Project Zomboid save is no longer file-locked, then back it up. (Not sure if this is possible yet)

## Requirements

All releases are built for x64 Windows machines. They are self contained and single file, therefore no dependencies are required.

## Development Requirements

This project was written in .NET 8, which requires the [.NET SDK](https://dotnet.microsoft.com/en-us/download).

## How to use

Running the `McKinley.ProjectZomboid.Backups.Runner.exe` file with no command line arguments will automatically detect your Project Zomboid save files and back them up into a ZIP file next to the `.exe`.
For more advanced usages, see the Advanced Options section below.

### Advanced Options

Powershell: `.\McKinley.ProjectZomboid.Backups.Runner.exe --help`

CMD: `McKinley.ProjectZomboid.Backups.Runner.exe --help`

```
  -t, --type              The backup file type. Zip and TarZLib are supported. The default is Zip

  -o, --output            Backup output folder. By default, the current directory will be used.

  -s, --save-directory    Project Zomboid Saves Directory. By default, it will try to detect your default Project
                          Zomboid saves directory (ex: %USERPROFILE%\Zomboid\Saves\Sandbox).

  --save-name             Only back up this save, ignores all others. There is no default value.

  --log-level             The log level to output to the console (Trace/Debug/Information/Warning/Error/Critical/None).
                          The default is Information.

  --zip-filename          Zip backup file name. The default is ProjectZomboid-Backups.zip

  --compression-level     Compression Level (Optimal/Fastest/NoCompression/SmallestSize). The default is Optimal.

  --help                  Display this help screen.

  --version               Display version information.
```