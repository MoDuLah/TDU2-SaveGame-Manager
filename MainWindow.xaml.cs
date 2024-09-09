using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;

namespace TDU2SaveGameManager
{
    public partial class MainWindow : Window
    {
        private string userDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eden Games", "Test Drive Unlimited 2"); //
        private string defaultBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        public string backupPath; // Use a variable to store the backup path
        public required string defaultSaveFolder; // Use a variable to store the gamesave path

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ApplicationTitle = $"TDU2 Save Game Manager - Version {GetAssemblyVersion()}";

            backupPath = Properties.Settings.Default.backupFolderPath;
            defaultSaveFolder = Properties.Settings.Default.defaultSaveFolder;
            SettingsGrid.Visibility = Visibility.Collapsed;
            // Validate if the saved backup path is valid and writable
            if (!Directory.Exists(backupPath) || !HasWritePermission(backupPath))
            {
                ShowError("The saved backup folder is invalid or not writable. Using the default backup folder.");
                // Fallback to the default backup path
                backupPath = defaultBackupPath;
                Properties.Settings.Default.backupFolderPath = backupPath;
                Properties.Settings.Default.Save();
            }
            ErrorPanel.Visibility = Visibility.Collapsed;
            LoadFolders();
            LoadBackups();
        }

        private void LoadFolders()
        {
            if (Directory.Exists(userDocumentsPath))
            {
                SaveLocation.Text = $"({userDocumentsPath})";
                Properties.Settings.Default.defaultSaveFolder = userDocumentsPath;
                Properties.Settings.Default.Save();
                FoldersListBox.Items.Clear();
                foreach (var dir in Directory.GetDirectories(userDocumentsPath))
                {
                    FoldersListBox.Items.Add(Path.GetFileName(dir));
                }
            }
            else
            {
                ShowError("Default TDU2 Save game path not found.");
                // Prompt user to select the save game folder
                var folderDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select TDU2 Save Game Folder",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                var result = folderDialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    userDocumentsPath = folderDialog.FileName;
                    Properties.Settings.Default.defaultSaveFolder = userDocumentsPath;
                    Properties.Settings.Default.Save();
                    LoadFolders(); // Reload folders from the new path
                }
                else
                {
                    ShowError("Please select a valid TDU2 Save game folder.");
                }
            }
        }

        private void LoadBackups()
        {
            if (!Directory.Exists(backupPath))
            {
                backupPath = defaultBackupPath;
                Directory.CreateDirectory(backupPath);
                ShowError($"Backup path created at: {backupPath}");
            }
            BackupLocation.Text = $"({backupPath})";
            BackupsListBox.Items.Clear();
            foreach (var file in Directory.GetFiles(backupPath, "*.tar"))
            {
                BackupsListBox.Items.Add(Path.GetFileName(file));
            }
            // Set the last item as the default selection
            if (BackupsListBox.Items.Count > 0)
            {
                BackupsListBox.SelectedIndex = BackupsListBox.Items.Count - 1; // Select the last item
            }
        }

        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            var backupTask = CreateBackupAsync();
            try
            {
                await backupTask;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                ShowError($"An error occurred: {ex.Message}");
            }
        }
        public async Task CreateBackupAsync()
        {
            string datetimeformat = Properties.Settings.Default.selectedTimeFormat;

            if (FoldersListBox.SelectedItem is string sourceDirName)
            {
                string sourceDir = Path.Combine(userDocumentsPath, sourceDirName);
                string datetime = DateTime.Now.ToString(datetimeformat);
                string archivePath = Path.Combine(backupPath, $"{sourceDirName}_backup_{datetime}.tar");

                try
                {
                    // Show the progress bar and error panel, and simulate a 2-second loading time
                    ErrorPanel.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;

                    // Wait for 2 seconds before executing the backup
                    await Task.Delay(2000);

                    // Proceed with backup after the 2-second delay
                    using (var tarStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var tarArchive = TarArchive.CreateOutputTarArchive(tarStream))
                    {
                        tarArchive.RootPath = sourceDir; // Set the root path for TAR entries
                        tarArchive.IsStreamOwner = true; // Make sure the stream is closed when finished
                        AddFilesToTarArchive(tarArchive, sourceDir);
                    }
                    // Show success message after progress bar is hidden
                    await Task.Delay(1000); // Optional small delay for smooth transition
                    ShowError("Backup successful!");
                    await Task.Delay(1000);

                    LoadBackups();

                    // Select the last backup in the list after loading backups
                    if (BackupsListBox.Items.Count > 0)
                    {
                        BackupsListBox.SelectedIndex = BackupsListBox.Items.Count - 1;
                    }


                }
                catch (Exception ex)
                {
                    ShowError($"An error occurred during backup: {ex.Message}");
                }
                finally
                {
                    // Hide the progress bar and reset selections
                    ProgressBar.IsIndeterminate = false;
                    FoldersListBox.SelectedItem = null;
                }
            }
            else
            {
                ShowError("Please select a folder to backup.");
            }
        }

        private void AddFilesToTarArchive(TarArchive tarArchive, string sourceDir)
        {
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);
                var entry = TarEntry.CreateEntryFromFile(file);
                entry.Name = relativePath;
                tarArchive.WriteEntry(entry, true);
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(defaultSaveFolder))
            {
                backupPath = defaultBackupPath;
                Directory.CreateDirectory(backupPath);
                ShowError($"Backup path created at: {backupPath}");
            }

            if (BackupsListBox.SelectedItem is string backupFileName)
            {
                string backupFile = Path.Combine(backupPath, backupFileName);
                string restoreDir = Path.Combine(userDocumentsPath, "savegame");

                if (!Directory.Exists(restoreDir))
                {
                    ShowError($"Restore directory does not exist: {restoreDir}");
                    return;
                }

                if (!HasWritePermission(restoreDir))
                {
                    ShowError($"You do not have write permissions for: {restoreDir}. Please ensure you have the necessary permissions or run the application as an administrator. Or attempt a manual extraction of the data to the savegame folder. Both folders will open in 5 seconds.");
                    await Task.Delay(5000);

                    // Open the backup and restore directories in Windows File Explorer
                    Process.Start("explorer.exe", backupFile);
                    Process.Start("explorer.exe", restoreDir);

                    return;
                }

                try
                {
                    ErrorPanel.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;

                    // Wait for 2 seconds before starting the restore process
                    await Task.Delay(2000);

                    await Task.Run(() =>
                    {
                        // Use SharpZipLib to extract the TAR file
                        using (Stream inStream = File.OpenRead(backupFile))
                        using (TarInputStream tarInputStream = new TarInputStream(inStream, Encoding.UTF8))
                        {
                            TarEntry entry;
                            while ((entry = tarInputStream.GetNextEntry()) != null)
                            {
                                string fullPath = Path.Combine(restoreDir, entry.Name);

                                if (entry.TarHeader.TypeFlag == TarHeader.LF_SYMLINK)
                                {
                                    // Optionally log and skip symlinks or handle accordingly
                                    continue;
                                }
                                else if (entry.IsDirectory)
                                {
                                    // Create directory if it doesn't exist
                                    Directory.CreateDirectory(fullPath);
                                }
                                else
                                {
                                    // Ensure the directory exists
                                    var directoryPath = Path.GetDirectoryName(fullPath);
                                    if (directoryPath != null)
                                    {
                                        Directory.CreateDirectory(directoryPath);
                                    }

                                    // Extract the file
                                    using (FileStream outputStream = File.Create(fullPath))
                                    {
                                        tarInputStream.CopyEntryContents(outputStream);
                                    }
                                }
                            }
                        }
                    });

                    LoadFolders();
                    LoadBackups();

                    // Show success message after progress bar is hidden
                    await Task.Delay(500); // Optional small delay for smooth transition
                    ShowError("Restore successful!");
                }
                catch (Exception ex)
                {
                    ShowError($"An error occurred during restore: {ex.Message}");
                }
                finally
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = false;
                    ErrorPanel.Visibility = Visibility.Collapsed;
                    FoldersListBox.SelectedItem = null;
                    BackupsListBox.SelectedItem = null;
                }
            }
            else
            {
                ShowError("Please select a backup to restore.");
            }
        }



        private bool HasWritePermission(string directoryPath)
        {
            try
            {
                string testFilePath = Path.Combine(directoryPath, Path.GetRandomFileName());
                using (var fs = File.Create(testFilePath)) { }
                File.Delete(testFilePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private async void ShowError(string message)
        {
            ErrorPanel.Visibility = Visibility.Visible;
            string formattedMessage = message.Replace(". ", "." + Environment.NewLine);
            ErrorTextBlock.Text = formattedMessage;
            await Task.Delay(5000);
            ErrorPanel.Visibility = Visibility.Collapsed;
        }

        private void ChooseBackupFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dialog.FileName;

                // Validate if the directory exists
                if (!Directory.Exists(selectedPath))
                {
                    ShowError("The selected directory does not exist.");
                    return;
                }

                // Validate if the application has write permissions
                if (!HasWritePermission(selectedPath))
                {
                    ShowError("You do not have write permissions for the selected directory.");
                    return;
                }

                // Save the valid selected path to application settings
                Properties.Settings.Default.backupFolderPath = selectedPath;
                Properties.Settings.Default.Save();

                // Update the backupPath variable and BackupLocation TextBlock
                backupPath = selectedPath;
                BackupLocation.Text = Properties.Settings.Default.backupFolderPath;
                BackupLocation.ToolTip = Properties.Settings.Default.backupFolderPath;

                // Reload the backups list for the new path
                LoadBackups();
            }
        }
        public required string ApplicationTitle { get; set; }

        private string GetAssemblyVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? version.ToString() : "Unknown Version";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dialog.FileName;

                // Validate if the directory exists
                if (!Directory.Exists(selectedPath))
                {
                    ShowError("The selected directory does not exist.");
                    return;
                }

                // Validate if the application has write permissions
                if (!HasWritePermission(selectedPath))
                {
                    ShowError("You do not have write permissions for the selected directory.");
                    return;
                }

                // Save the valid selected path to application settings
                Properties.Settings.Default.backupFolderPath = selectedPath;
                Properties.Settings.Default.Save();

                // Update the backupPath variable and BackupLocation TextBlock
                backupPath = selectedPath;
                BackupLocation.Text = $"({backupPath})";

                // Reload the backups list for the new path
                LoadBackups();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ShowError("Folders refreshed!");
            LoadFolders();
            LoadBackups();
        }


        private void toggle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (toggleOff.Visibility == Visibility.Visible)
            {
                toggleOff.Visibility = Visibility.Collapsed;
                toggleOn.Visibility = Visibility.Visible;
                SettingsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                toggleOff.Visibility = Visibility.Visible;
                toggleOn.Visibility = Visibility.Collapsed;
                SettingsGrid.Visibility = Visibility.Collapsed;
            }
        }


        private void timeformatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeformatComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Ensure Content is not null before converting to string
                string selectedTimeFormat = selectedItem.Content?.ToString() ?? "DefaultFormat";

                ShowError($"The time format has changed to: {selectedTimeFormat}");
                Properties.Settings.Default.selectedTimeFormat = selectedTimeFormat;
                Properties.Settings.Default.Save();
            }
            else
            {
                // Handle the case where no item is selected, if needed
                ShowError("No time format selected.");
            }
        }
    }
}
