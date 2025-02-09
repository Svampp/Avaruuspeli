using Raylib_CsLo;

namespace Avaruuspeli
{
    /// <summary>
    /// Settings screen.
    /// </summary>
    internal class SettingsMenu
    {
        // Event triggered when the "Back" button is pressed
        public event EventHandler BackButtonPressed;

        /// <summary>
        /// Displays the settings menu.
        /// </summary>
        public void ShowMenu()
        {
            // Get the current window size
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // Define button dimensions
            int buttonWidth = 100, buttonHeight = 40;

            // Calculate the centered position for the buttons
            int centerX = windowWidth / 2 - buttonWidth / 2;
            int centerY = windowHeight / 2 - buttonHeight / 2;

            // Draw the "OPTIONS" title
            RayGui.GuiLabel(new Rectangle(centerX, centerY - buttonHeight, buttonWidth, buttonHeight), "OPTIONS");

            // Draw the "Back" button and check if it's clicked
            if (RayGui.GuiButton(new Rectangle(centerX, centerY, buttonWidth, buttonHeight), "Back"))
                BackButtonPressed?.Invoke(this, EventArgs.Empty); // Trigger the event when the button is pressed
        }
    }
}
