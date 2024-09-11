using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using TDU2SaveGameManager.Properties;
using TDU2SaveGameManager.Classes;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;

namespace TDU2SaveGameManager
{
    public partial class MainWindow : Window
    {
        private string defaultSaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eden Games", "Test Drive Unlimited 2"); //
        private string defaultBackupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        private string userSelectedSaveFolder = Settings.Default.userSelectedSaveFolder;
        private string userSelectedBackupFolder = Settings.Default.userSelectedBackupFolder;
        private string timeFormat = !string.IsNullOrEmpty(Settings.Default.userSelectedTimeFormat)
            ? Settings.Default.userSelectedTimeFormat
            : Settings.Default.defaultTimeFormat;
        private double settingsMainWindowOpacity = Convert.ToDouble(Settings.Default.OpacityMainWindow);
        private double settingsBGOpacity = Convert.ToDouble(Settings.Default.OpacityBackground);
        public int Run = Settings.Default.runTimes;
        public int TypingDelay = Settings.Default.Typingdelay;
        //public bool actionlogdisplay = Settings.Default.ActionLogDisplay;
        //public bool typinganimationdisplay = Settings.Default.TypingAnimationDisplay;
        public required string ApplicationTitle { get; set; }
        public required string ApplicationRun { get; set; }
        private readonly Queue<string> messageQueue = new Queue<string>();
        private bool isTyping = false; // Flag to prevent overlapping typing effects
        public bool ActionLogDisplay
        {
            get { return Settings.Default.ActionLogDisplay; }
            set
            {
                Settings.Default.ActionLogDisplay = value;
                Settings.Default.Save(); // Save the setting
            }
        }
        public bool TypingAnimationDisplay
        {
            get { return Settings.Default.TypingAnimationDisplay; }
            set
            {
                Settings.Default.TypingAnimationDisplay = value;
                Settings.Default.Save(); // Save the setting
            }
        }
        public bool SettingsVisibility
        {
            get { return Settings.Default.SettingsVisibility; }
            set
            {
                Settings.Default.SettingsVisibility = value;
                Settings.Default.Save(); // Save the setting
            }
        }
        public ObservableCollection<string> ErrorMessages { get; set; }

        public MainWindow()
        {
            ErrorMessages = new ObservableCollection<string>();
            InitializeComponent();
            // Bind ErrorListBox to the ObservableCollection for error messages
            ErrorListBox.ItemsSource = ErrorMessages;
            bool SettingsVis = Settings.Default.SettingsVisibility;

            if (SettingsVis == true) {
                Settings_ToggleButton.IsChecked = true;
                SettingsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                Settings_ToggleButton.IsChecked = false;
                SettingsGrid.Visibility = Visibility.Hidden;
            }
            if (ActionLogDisplay == true)
            {
                ErrorGroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorGroupBox.Visibility = Visibility.Hidden;
            }
            if (TypingAnimationDisplay == false)
            {
                TypingDelay = Settings.Default.Typingdelay = 0;
                Settings.Default.TypingAnimationDisplay = false;
                Settings.Default.Save();
            }
            else
            {
                TypingDelay = Settings.Default.Typingdelay = 25;
                Settings.Default.TypingAnimationDisplay = true;
                Settings.Default.Save();
            }

            // Set DataContext for binding properties like ApplicationTitle and ApplicationRun
            DataContext = this;

            // Set ApplicationTitle and Run count for displaying in the UI
            ApplicationTitle = Settings.Default.version;
            Settings.Default.runTimes = ++Run; // Increment run counter
            ApplicationRun = Run.ToString();
            Settings.Default.Save();

             // Set the slider to reflect the saved value
            mainWindowOpacitySlider.Value = settingsMainWindowOpacity;
            backGroundOpacitySlider.Value = settingsBGOpacity;
            ActionLogDisplay = Settings.Default.ActionLogDisplay;
            // Initialize paths for save folder and backup folder
            InitializePaths();
            //ShowSettings();
            LoadFolders();
            LoadBackups();
            // Attach event handlers

        }

        private void InitializePaths()
        {
            // Ensure backup directory exists, and check for write permission
            FolderCheck.EnsureDirectoryExists(Settings.Default.userSelectedBackupFolder, ShowError);
            if (!FolderCheck.HasWritePermission(Settings.Default.userSelectedBackupFolder, ShowError))
            {
                ShowError($"Checking for write permissions on backup folder. Error: Backup folder does not have write permission. {Settings.Default.userSelectedBackupFolder}");
            }

            // Ensure save directory exists, and check for write permission
            FolderCheck.EnsureDirectoryExists(Settings.Default.userSelectedSaveFolder, ShowError);
            if (!FolderCheck.HasWritePermission(Settings.Default.userSelectedSaveFolder, ShowError))
            {
                ShowError($"Save folder does not have write permission. {Settings.Default.userSelectedSaveFolder}");
            }

            // Handle user-defined or default save folder
            if (string.IsNullOrEmpty(Settings.Default.userSelectedSaveFolder) || Settings.Default.userSelectedSaveFolder == "null")
            {
                // Use default save folder if user-defined one is invalid or null
                ShowError($"Checking for User-Defined Save Folder. No user-defined save folder found. Setting Default save at: {defaultSaveFolder}");
                Settings.Default.defaultSaveFolder = defaultSaveFolder; // Set the default path in settings
                SavesLocation.Text = defaultSaveFolder;
            }
            else
            {
                // Use user-defined save folder if valid
                ShowError($"Checking for User-Defined Save Folder. User-defined save folder found! {Settings.Default.userSelectedSaveFolder}");
                SavesLocation.Text = Settings.Default.userSelectedSaveFolder;
            }

            // Handle user-defined or default backup folder
            if (string.IsNullOrEmpty(Settings.Default.userSelectedBackupFolder) || Settings.Default.userSelectedBackupFolder == "null")
            {
                // Use default backup folder if user-defined one is invalid or null
                ShowError($"No user-defined backup folder. Default backup folder set: {defaultBackupFolder}");
                Settings.Default.defaultBackupFolder = defaultBackupFolder; // Set the default path in settings
                BackupLocation.Text = defaultBackupFolder;
            }
            else
            {
                // Use user-defined backup folder if valid
                ShowError($"User-defined backup folder used: {Settings.Default.userSelectedBackupFolder}");
                SettingsCheck.SaveBackupFolderPath(Settings.Default.userSelectedBackupFolder);
                BackupLocation.Text = Settings.Default.userSelectedBackupFolder;
            }

            // Save the updated settings
            Settings.Default.Save();
        }

        private void ShowSettings()
        {
            MessageBox.Show($"User Selected Backup Folder: {Settings.Default.userSelectedBackupFolder}\n" +
                            $"Default Backup Folder:{Settings.Default.defaultBackupFolder}\n\n" +
                            $"User Selected Save Folder: {Settings.Default.userSelectedSaveFolder}\n" +
                            $"Default Savegame Folder: {Settings.Default.defaultSaveFolder}.\n\n" +
                            $"Current Time Format: {timeFormat}");
        }
        private void TimeformatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedFormat = "";

            // Check if a valid ComboBoxItem is selected
            if (timeformatComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Get the selected format as a string
                selectedFormat = selectedItem.Content?.ToString() ?? ""; // Ensure it's not null

                if (!string.IsNullOrEmpty(selectedFormat))
                {
                    // Save the selected format
                    SettingsCheck.SaveTimeFormat(selectedFormat);
                    ShowError($"The time format has changed to: {selectedFormat}");
                }
                else
                {
                    // Handle potential empty or null cases
                    ShowError("Invalid format selected.");
                }
            }
            else
            {
                // Fallback to default time format
                ShowError($"Using default format {timeFormat}");
            }
        }

        private void LoadFolders()
        {

            if (Directory.Exists(defaultSaveFolder) || string.IsNullOrEmpty(userSelectedSaveFolder))
            {
                SavesLocation.Text = $"({defaultSaveFolder})";

                FoldersListBox.Items.Clear();
                foreach (var dir in Directory.GetDirectories(defaultSaveFolder))
                {
                    FoldersListBox.Items.Add(Path.GetFileName(dir));
                }
            }
            else
            {
                ShowError("TDU2 Save game path not found.");
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
                    string selectedFolder = folderDialog.FileName;
                    SavesLocation.Text = $"({selectedFolder})";
                    Settings.Default.userSelectedSaveFolder = selectedFolder;
                    Settings.Default.Save();
                    ShowError("User selected valid TDU2 Save game folder and it saved at settings.");
                    LoadFolders(); // Reload folders from the new path
                }
                else
                {
                    ShowError("Please select a valid TDU2 Save game folder.");
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFolders();
            LoadBackups();
        }
        private void LoadBackups()
        {
            string backupPath;
            if (!Directory.Exists(userSelectedBackupFolder) && !string.IsNullOrEmpty(userSelectedBackupFolder))
            {
                Directory.CreateDirectory(userSelectedBackupFolder);
                ShowError($"Backup path created at: {userSelectedBackupFolder}");
                backupPath = userSelectedBackupFolder;
            }
            else
            {
                if (!string.IsNullOrEmpty(userSelectedBackupFolder)){
                    backupPath = userSelectedBackupFolder;
                    //ShowError($"Backup path is set by user at {backupPath}");

                } 
                else
                {
                    backupPath = defaultBackupFolder;
                    //ShowError($"Backup path is set to default folder (App root folder) {backupPath}");
                }
            }
            BackupLocation.Text = $"({backupPath})";
            BackupsListBox.Items.Clear();
            foreach (var file in Directory.GetFiles(backupPath, "*.zip"))
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
                string savePath, backupPath;
                if (string.IsNullOrEmpty(userSelectedSaveFolder))
                {
                    savePath = defaultSaveFolder;
                }
                else
                {
                    savePath = userSelectedSaveFolder;
                }
                if (string.IsNullOrEmpty(userSelectedBackupFolder))
                {
                    backupPath = defaultBackupFolder;
                } else
                {
                    backupPath = userSelectedBackupFolder;
                }
                // Get the used backup folder (either default or user-selected)
                string sourceDir = Path.Combine(savePath, sourceDirName);
                string datetimeFormat = SettingsCheck.GetTimeFormat();
                string datetime = DateTime.Now.ToString(datetimeFormat);
                string archiveName = $"{sourceDirName}_backup_{datetime}";

                // Ensure backup folder exists and has write permissions
                FolderCheck.EnsureDirectoryExists(backupPath, ShowError);
                if (!FolderCheck.HasWritePermission(backupPath, ShowError)) return;

                // Perform the backup
                await BackupHandling.CreateBackupAsync(sourceDir, backupPath, archiveName, ProgressBar, ShowError);

                // Reload backup list after backup completes
                FolderCheck.LoadDirectoriesIntoListBox(backupPath, BackupsListBox);

                if (BackupsListBox.Items.Count > 0)
                {
                    BackupsListBox.SelectedIndex = BackupsListBox.Items.Count - 1;
                }
                LoadFolders();
                LoadBackups();
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
                string savePath, backupPath;
                if (string.IsNullOrEmpty(userSelectedSaveFolder))
                {
                    savePath = defaultSaveFolder;
                }
                else
                {
                    savePath = userSelectedSaveFolder;
                }
                if (string.IsNullOrEmpty(userSelectedBackupFolder))
                {
                    backupPath = defaultBackupFolder;
                }
                else
                {
                    backupPath = userSelectedBackupFolder;
                }
                // Get the used save folder (either default or user-selected)
                string backupFile = Path.Combine(backupPath, backupFileName);
                string restoreDir = Path.Combine(savePath,"savegame");

                // Ensure restore directory exists and has write permissions
                FolderCheck.EnsureDirectoryExists(restoreDir, ShowError);
                if (!FolderCheck.HasWritePermission(restoreDir, ShowError)) return;

                // Perform the restore
                await BackupHandling.RestoreBackupAsync(backupFile, restoreDir, ProgressBar, ShowError);

                // Reload folders after restore completes
                FolderCheck.LoadDirectoriesIntoListBox(savePath, FoldersListBox);
                FolderCheck.LoadDirectoriesIntoListBox(backupPath, BackupsListBox);
                LoadFolders();
                LoadBackups();
            }
            else
            {
                ShowError("Please select a backup to restore.");
            }
        }
        public async void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            // Add the new message to the queue
            messageQueue.Enqueue(message);

            // Process messages from the queue one at a time
            await ProcessMessageQueue();
        }
        private async Task ProcessMessageQueue()
        {
            // If a message is already being typed, return early
            if (isTyping || messageQueue.Count == 0) return;

            // Set the typing flag
            isTyping = true;

            // Get the first message from the queue
            string message = messageQueue.Dequeue();

            // Format the message for multi-line display
            string formattedMessage = message.Replace(". ", "." + Environment.NewLine)
                                             .Replace("! ", "! " + Environment.NewLine);

            ErrorMessages.Add(""); // Add an empty message to start typing

            // Scroll to the last item.
            if (ErrorListBox.Items.Count > 4)
            {
                // Select the last item in the ListBox
                ErrorListBox.SelectedIndex = ErrorListBox.Items.Count - 1;

                // Scroll the ListBox to the last item
                var lastItem = ErrorListBox.Items[ErrorListBox.Items.Count - 1];
                ErrorListBox.ScrollIntoView(lastItem);

                // Get the ScrollViewer and scroll to the bottom
                if (VisualTreeHelper.GetChild(ErrorListBox, 0) is Decorator border && border.Child is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToBottom();
                }
            }
            // Apply the typing effect
            int typingdelay = Settings.Default.Typingdelay;
            await TextAnimation.TypeListBoxTextAsync(formattedMessage, ErrorMessages, TimeSpan.FromMilliseconds(TypingDelay));

            // Delay to keep the message visible
            await Task.Delay(500);

            // Reset the flag after typing is finished
            isTyping = false;

            // Continue processing the next message in the queue
            if (messageQueue.Count > 0)
            {
                await ProcessMessageQueue();  // Process the next message
            }
        }

        private string GetAssemblyVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? version.ToString() : "Unknown Version";
        }

        private void btn_BackupFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dialog.FileName;

                // Validate if the directory exists
                FolderCheck.EnsureDirectoryExists(selectedPath, ShowError);


                // Validate if the application has write permissions
                if (!FolderCheck.HasWritePermission(selectedPath, ShowError))
                {
                    ShowError("You do not have write permissions for the selected directory.");
                    return;
                }

                // Save the valid selected path to application settings
                Settings.Default.userSelectedBackupFolder = userSelectedBackupFolder = selectedPath;
                Settings.Default.Save();

                // Update the backupPath variable and BackupLocation TextBlock
                string backupPath = selectedPath;
                BackupLocation.Text = $"({backupPath})";
                BackupLocation.Text = Settings.Default.userSelectedBackupFolder;
                BackupLocation.ToolTip = Settings.Default.userSelectedBackupFolder;
                ShowError("User selected backup folder has changed successfully.");

                // Reload the backups list for the new path
                LoadBackups();
            }
        }

        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
        private void SocialButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var url = button?.CommandParameter as string;
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        private void browseBackups_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string Originaltext = BackupLocation.Text;
            string updatedText = BackupLocation.Text.Substring(1, BackupLocation.Text.Length - 2); // Remove first and last character
            BackupLocation.Text = updatedText;
            if (!string.IsNullOrEmpty(updatedText)) {
                ShowError($"{updatedText} is opening!");
                Process.Start("explorer.exe", updatedText);
            }
            else
            {
                ShowError($"{updatedText} is blank");
            }
            BackupLocation.Text = Originaltext;
        }

        private void SaveGameManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void SaveGameManager_Loaded(object sender, RoutedEventArgs e)
        {
            double windowWidth = this.ActualWidth;
            // Use the width as needed
            spacer.Width = windowWidth / 2 - 100;
        }

        private void SaveGameManager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double windowWidth = this.ActualWidth;
            // Use the width as needed
            spacer.Width = windowWidth / 2 - 100;
        }

        private void SaveGameManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void AL_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ErrorGroupBox.Visibility = Visibility.Visible;
            ActionLogDisplay = true; // Update setting when checked

        }

        private void AL_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ErrorGroupBox.Visibility = Visibility.Hidden;
            ActionLogDisplay = false;
        }

        private void TA_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            TypingDelay = 25;
            TypingAnimationDisplay = true;
            Settings.Default.Typingdelay = 25;
            Settings.Default.Save();
        }

        private void TA_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            TypingDelay = 0;
            TypingAnimationDisplay = false;
            Settings.Default.Typingdelay = 0;
            Settings.Default.Save();
        }

        private void Settings_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;
            Settings.Default.SettingsVisibility = true;
            Settings.Default.Save();
        }

        private void Settings_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Hidden;
            Settings.Default.SettingsVisibility = false;
            Settings.Default.Save();
        }
    }
}
