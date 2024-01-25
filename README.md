This is a personal project which handles backing up Project Zomboid saves into a ZIP file.

Future plans/research:

- A GUI for managing backups.
- Better compression algorithm, or a choice between compression algorithms?? (maybe, ZIP does a good job here)
- Run on a schedule, detect when a Project Zomboid save is no longer file-locked, then back it up. (Not sure if this is possible yet)

## Requirements

All releases are built for x64 Windows machines. They are self contained and single file, therefore no dependencies are required.

## Development Requirements

This project was written in .NET 8, which requires the [.NET SDK](https://dotnet.microsoft.com/en-us/download).

## How to use

A guide on how to use this will be constructed later. For now, check out the `McKinley.ProjectZomboid.Backups.Runner` release.