using System;
using System.IO;
using System.Windows.Controls;

namespace TDU2SaveGameManager.Classes
{
    public class FolderCheck
    {
        // Check if a directory exists, and if not, create it
        public static void EnsureDirectoryExists(string path, Action<string> showError)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    showError($"Directory created: {path}");
                }
                catch (Exception ex)
                {
                    showError($"Error creating directory: {path}. Exception: {ex.Message}");
                }
            }
        }

        // Check if a directory has write permissions
        public static bool HasWritePermission(string path, Action<string> showError)
        { if (!string.IsNullOrEmpty(path))
            { 
            try
                {
                    // Try to create a temporary file to check write permission
                    string testFile = Path.Combine(path, Path.GetRandomFileName());
                    using (FileStream fs = File.Create(testFile, 1, FileOptions.DeleteOnClose)) { }
                    showError($"User has Write permission for: {path}.");
                    return true;
                }
                catch (Exception ex)
                {
                    showError($"Write permission check failed for: {path}. Exception: {ex.Message}");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // Method to load directories into a ListBox
        public static void LoadDirectoriesIntoListBox(string path, ListBox listBox)
        {
            if (string.IsNullOrEmpty(path)) return;
            listBox.Items.Clear();
            if (Directory.Exists(path))
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    listBox.Items.Add(Path.GetFileName(dir));
                }
            }
        }
    }
}
