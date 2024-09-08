using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TDU2SaveGameManager
{
    public partial class MainWindow : Window
    {
        private string userDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eden Games", "Test Drive Unlimited 2"); //
        private readonly string defaultBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        private string backupPath; // Use a variable to store the backup path

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ApplicationTitle = $"TDU2 Save Game Manager - Version {GetAssemblyVersion()}";

            backupPath = Properties.Settings.Default.backupFolderPath;

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
            if (FoldersListBox.SelectedItem is string sourceDirName)
            {
                string sourceDir = Path.Combine(userDocumentsPath, sourceDirName);
                string datetime = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
                string archivePath = Path.Combine(backupPath, $"{sourceDirName}_backup_{datetime}.tar");

                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\tar.exe",
                    Arguments = $"-cf \"{archivePath}\" -C \"{sourceDir}\" .",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    // Show the progress bar and error panel, and simulate a 2-second loading time
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;
                    ErrorPanel.Visibility = Visibility.Visible;

                    // Wait for 2 seconds before executing the backup
                    await Task.Delay(2000);

                    // Proceed with backup after the 2-second delay
                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null) // Ensure the process is not null
                        {
                            process.WaitForExit();
                            if (process.ExitCode == 0)
                            {
                                LoadBackups();

                                // Select the last backup in the list after loading backups
                                if (BackupsListBox.Items.Count > 0)
                                {
                                    BackupsListBox.SelectedIndex = BackupsListBox.Items.Count - 1;
                                }

                                // Show success message after progress bar is hidden
                                await Task.Delay(500); // Optional small delay for smooth transition
                                ShowError("Backup successful!");
                            }
                            else
                            {
                                ShowError($"Backup failed: {process.StandardError.ReadToEnd()}");
                            }
                        }
                        else
                        {
                            ShowError("Failed to start the backup process.");
                        }
                    }

                }
                catch (Exception ex)
                {
                    ShowError($"An error occurred during backup: {ex.Message}");
                }
                finally
                {
                    // Hide the progress bar and reset selections
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = false;
                    ErrorPanel.Visibility = Visibility.Collapsed;
                    FoldersListBox.SelectedItem = null;
                }
            }
            else
            {
                ShowError("Please select a folder to backup.");
            }
        }



        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
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

                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\tar.exe",
                    Arguments = $"-xf \"{backupFile}\" -C \"{restoreDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    ErrorPanel.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;

                    // Wait for 2 seconds before starting the restore process
                    await Task.Delay(2000);

                    await Task.Run(() =>
                    {
                        var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                        }
                        else
                        {
                            ShowError("Failed to start the process.");
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
    }
}
