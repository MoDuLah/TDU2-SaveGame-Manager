# TDU2 SaveGame Manager
![GitHub Repo stars](https://img.shields.io/github/stars/MoDuLah/TDU2-SaveGame-Manager)
## Download
From the [releases page](https://github.com/MoDuLah/TDU2-SaveGame-Manager/releases). 
## Overview
The **TDU2 SaveGame Manager** is a tool designed to manage, backup, and restore save game files for the game **Test Drive Unlimited 2**.  
It provides an intuitive interface for players to manage their savegame data, back it up, and easily restore backups.  
The tool also includes additional features such as progress bars for loading tasks and validation to ensure smooth operations.  

## Features
- **Backup Save Games:** Easily back up your TDU2 save game files with the click of a button. Backups are created with a timestamp to help keep track of versions.
- **Restore Backups:** Select from a list of previously created backups and restore your save game data.
- **Validation:** Checks for write permissions and ensures proper directory structures before performing operations.
- **Progress Feedback:** Displays progress bar animations during backup and restore operations for a smooth user experience.
- **Selectable Backup Destination:** The user can select a custom directory to store backups.

## Backup and Restore Operations

### Backup
1. Select the folder containing the savegame files from the list on the left side.
2. Feel free to customise the backup destination folder or let the program do it for you on a subdirectory in the software folder.
3. Click the Backup button to create an uncompressed .zip archive of the save game folder in the selected destination.
4. The backup will be saved in the selected filename format & folder from the settings (or will use the default).

### Restore
1. Choose the backup file from the list.
2. Click the Restore button to extract and restore the selected backup on the selected folder.

## Known Issues
![GitHub issues](https://img.shields.io/github/issues/MoDuLah/TDU2-SaveGame-Manager)

## Future Improvements
- Add compression choices.


## Prerequisites
- **.NET 8.0** (or later) SDK
- **Windows OS** (for running the executable)

# For developers/contributors
### Clone this repository:
   ```bash
   git clone https://github.com/MoDuLah/MoDuLah.git
   ```
1. Open the solution in Visual Studio or your preferred C# editor.
2. Restore the NuGet packages.
3. Build the solution.

## Running the Application
Once the project is built, you can run the application directly from your IDE or as a standalone executable.

Forks: 
## Contributing
If you'd like to contribute to the project, feel free to fork the repository and submit a pull request.  
Be sure to check out the issues page for any known issues or feature requests!  
![GitHub forks](https://img.shields.io/github/forks/MoDuLah/TDU2-SaveGame-Manager)

## License
This project is licensed under the GNU General Public License (GPL) v3.0 - see the [LICENSE](LICENSE) file for details.
