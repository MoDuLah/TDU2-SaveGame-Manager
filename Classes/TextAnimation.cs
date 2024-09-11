using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TDU2SaveGameManager.Classes
{
    public static class TextAnimation
    {
        // Typing effect for a TextBlock
        public static async Task TypeTextAsync(string text, TextBlock textBlock, TimeSpan delay)
        {
            textBlock.Text = ""; // Clear existing text

            foreach (char c in text)
            {
                textBlock.Text += c; // Add one character at a time
                await Task.Delay(delay); // Wait for the specified delay
            }
        }

        // Typing effect for a ListBox (adds characters to the last item in the ListBox)
        public static async Task TypeListBoxTextAsync(string text, ObservableCollection<string> messages, TimeSpan delay)
        {
            // Add a new entry in the ObservableCollection if it's empty
            if (messages.Count == 0)
            {
                messages.Add("");
            }

            // Loop through each character and append it to the last item in the ObservableCollection
            for (int i = 0; i < text.Length; i++)
            {
                // Get the last message in the ObservableCollection
                string lastMessage = messages[messages.Count - 1];

                // Update the last message with the new character
                messages[messages.Count - 1] = lastMessage + text[i];

                // Delay to simulate typing
                await Task.Delay(delay);
            }
        }
    }
}