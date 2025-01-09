using Raylib_CsLo;

namespace Avaruuspeli
{
    /// <summary>
    /// Settings screen.
    /// </summary>
    internal class SettingsMenu
    {
        public event EventHandler BackButtonPressed;

        public void ShowMenu()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();
            int buttonWidth = 100, buttonHeight = 40;

            int centerX = windowWidth / 2 - buttonWidth / 2;
            int centerY = windowHeight / 2 - buttonHeight / 2;

            RayGui.GuiLabel(new Rectangle(centerX, centerY - buttonHeight, buttonWidth, buttonHeight), "OPTIONS");

            if (RayGui.GuiButton(new Rectangle(centerX, centerY, buttonWidth, buttonHeight), "Back"))
                BackButtonPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
