using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli 
{
    /// <summary>
    /// Main menu of the game.
    /// </summary>
    internal class MainMenu
    {
        // Events triggered when the corresponding buttons are pressed
        public event EventHandler StartButtonPressed;
        public event EventHandler SettingsButtonPressed;
        public event EventHandler QuitButtonPressed;

        // Positions for the "SPACE INVADERS" title animation
        Vector2 spacePosition, invadersPosition;

        // Animation progress for the title movement
        float animationProgress;
        const float animationSpeed = 0.5f; // Speed of the animation

        // List of stars for the animated background effect
        List<Vector2> startScreenStars;

        public MainMenu()
        {
            var random = new Random();
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // Initial positions for the title text (off-screen)
            spacePosition = new Vector2(-200, windowHeight / 5);
            invadersPosition = new Vector2(windowWidth + 200, windowHeight / 5 + 80);
            animationProgress = 0.0f;

            // Creating stars for the background effect
            startScreenStars = new List<Vector2>(20);
            for (int i = 0; i < startScreenStars.Capacity; i++)
            {
                startScreenStars.Add(new Vector2(random.Next(0, windowWidth), random.Next(-windowHeight, -1)));
            }
        }

        /// <summary>
        /// Updates the menu animation and star positions.
        /// </summary>
        public void Update()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // Title animation (progressively moves to the center)
            if (animationProgress < 1.0f)
            {
                animationProgress = Math.Min(animationProgress + Raylib.GetFrameTime() * animationSpeed, 1.0f);
            }

            // Get text widths for proper centering
            int spaceWidth = Raylib.MeasureText("SPACE", 80);
            int invadersWidth = Raylib.MeasureText("INVADERS", 80);

            // Interpolating positions to smoothly animate text from off-screen to the center
            spacePosition.X = Lerp(-spaceWidth, (windowWidth / 2 - spaceWidth / 2), animationProgress);
            invadersPosition.X = Lerp(windowWidth, (windowWidth / 2 - invadersWidth / 2), animationProgress);

            // Update star positions (simulate star movement)
            for (int i = 0; i < startScreenStars.Count; i++)
            {
                var star = startScreenStars[i];

                // Move stars downward and loop them back to the top when they reach the bottom
                star.Y = (star.Y + 40 * Raylib.GetFrameTime()) % windowHeight;
                startScreenStars[i] = star;
            }
        }

        /// <summary>
        /// Renders the main menu.
        /// </summary>
        public void ShowMenu()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // Draw background gradient
            Raylib.DrawCircleGradient(windowWidth / 2, windowHeight / 2 - 100, windowHeight * 3.0f, Raylib.BLACK, Raylib.BLUE);

            // Draw moving stars
            foreach (var star in startScreenStars)
            {
                Raylib.DrawCircleGradient((int)star.X, (int)star.Y, 4.0f, Raylib.WHITE, Raylib.BLACK);
            }

            // Draw game title
            Raylib.DrawText("SPACE", (int)spacePosition.X, (int)spacePosition.Y, 80, Raylib.LIGHTGRAY);
            Raylib.DrawText("INVADERS", (int)invadersPosition.X, (int)invadersPosition.Y, 80, Raylib.GREEN);

            // Button properties
            int buttonWidth = 100, buttonHeight = 40;
            int centerX = windowWidth / 2 - buttonWidth / 2;
            int centerY = windowHeight / 2 - buttonHeight / 2;

            // Draw buttons and trigger events if clicked
            if (RayGui.GuiButton(new Rectangle(centerX, centerY, buttonWidth, buttonHeight), "Start Game"))
                StartButtonPressed?.Invoke(this, EventArgs.Empty);

            if (RayGui.GuiButton(new Rectangle(centerX, centerY + buttonHeight * 2, buttonWidth, buttonHeight), "Options"))
                SettingsButtonPressed?.Invoke(this, EventArgs.Empty);

            if (RayGui.GuiButton(new Rectangle(centerX, centerY + buttonHeight * 4, buttonWidth, buttonHeight), "Quit"))
                QuitButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Linear interpolation function for smooth animations.
        /// </summary>
        float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
