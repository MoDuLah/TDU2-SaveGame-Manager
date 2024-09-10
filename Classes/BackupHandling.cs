using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TDU2SaveGameManager.Classes
{
    public class BackupHandling
    {
        // Method to create a zip backup
        public static async Task CreateBackupAsync(string sourceDir, string backupPath, string archiveName, ProgressBar progressBar, Action<string> showError)
        {
            try
            {
                progressBar.Visibility = System.Windows.Visibility.Visible;
                progressBar.IsIndeterminate = true;
                await Task.Delay(2000);  // Simulate 2-second loading time

                string archivePath = Path.Combine(backupPath, $"{archiveName}.zip");

                var fastZip = new FastZip();
                fastZip.CreateZip(archivePath, sourceDir, true, null);

                showError($"Backup created successfully at {archivePath}!");
            }
            catch (Exception ex)
            {
                showError($"An error occurred during backup: {ex.Message}");
            }
            finally
            {
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                progressBar.IsIndeterminate = false;
            }
        }
        // Method to restore from a zip backup
        public static async Task RestoreBackupAsync(string backupFile, string restoreDir, ProgressBar progressBar, Action<string> showError)
        {
            try
            {
                progressBar.Visibility = System.Windows.Visibility.Visible;
                progressBar.IsIndeterminate = true;
                await Task.Delay(2000);  // Simulate 2-second loading time

                var fastZip = new FastZip();
                fastZip.ExtractZip(backupFile, restoreDir, null);

                showError($"Backup restored successfully at {restoreDir}!");
            }
            catch (Exception ex)
            {
                showError($"An error occurred during restore: {ex.Message}");
            }
            finally
            {
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                progressBar.IsIndeterminate = false;
            }
        }
    }
}
