using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TDU2SaveGameManager
{
    public partial class MainWindow : Window
    {
        private string userDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eden Games", "Test Drive Unlimited 2");
        private readonly string defaultBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        private string backupPath; // Use a variable to store the backup path

        public MainWindow()
        {
            InitializeComponent();

            backupPath = Properties.Settings.Default.BackupFolderPath;

            // Validate if the saved backup path is valid and writable
            if (!Directory.Exists(backupPath) || !HasWritePermission(backupPath))
            {
                ShowError("The saved backup folder is invalid or not writable. Using the default backup folder.");
                // Fallback to the default backup path
                backupPath = defaultBackupPath;
                Properties.Settings.Default.BackupFolderPath = backupPath;
                Properties.Settings.Default.Save();
            }

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
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersListBox.SelectedItem is string sourceDirName)
            {
                string sourceDir = Path.Combine(userDocumentsPath, sourceDirName);
                string datetime = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
                string archivePath = Path.Combine(backupPath, $"backup_{datetime}.tar");

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
                    ProgressBar.Visibility = Visibility.Visible;

                    using (var process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            ShowError("Backup successful!");
                            LoadBackups();
                        }
                        else
                        {
                            ShowError($"Backup failed: {process.StandardError.ReadToEnd()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"An error occurred during backup: {ex.Message}");
                }
                finally
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    FoldersListBox.SelectedItem = null;
                    BackupsListBox.SelectedItem = null;
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
                //MessageBox.Show(backupPath);
                string backupFile = Path.Combine(backupPath, backupFileName);
                string restoreDir = Path.Combine(userDocumentsPath, $"savegame");
                //MessageBox.Show($"backupFile: " + backupFile + "\nRestore Dir: " + restoreDir);
                if (!Directory.Exists(restoreDir))
                {
                    ShowError($"Restore directory does not exist: {restoreDir}");
                    return;
                }

                if (!HasWritePermission(restoreDir))
                {
                    ShowError($"You do not have write permissions for: {restoreDir}. Please ensure you have the necessary permissions or run the application as an administrator. Or attempt a manual extraction of the data to the savegame folder. Both folders openning in 5 seconds.");
                    await Task.Delay(5000);

                    // Open the restore directory
                    Process.Start("explorer.exe", restoreDir);

                    // Open the backup file
                    Process.Start("explorer.exe", $"/select,\"{backupFile}\"");

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
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;

                    await Task.Run(() =>
                    {
                        using (var process = Process.Start(startInfo))
                        {
                            process.WaitForExit();
                        }
                    });

                    ShowError("Restore successful!");
                    LoadFolders();
                    LoadBackups();
                }
                catch (Exception ex)
                {
                    ShowError($"An error occurred during restore: {ex.Message}.");
                }
                finally
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = false;
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
            string formattedMessage = message.Replace(". ", "." + Environment.NewLine);
            ErrorTextBlock.Text = formattedMessage;
            ErrorPanel.Visibility = Visibility.Visible;
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
                Properties.Settings.Default.BackupFolderPath = selectedPath;
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
