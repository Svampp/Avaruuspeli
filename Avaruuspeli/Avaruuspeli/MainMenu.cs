using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Main menu of the game.
    /// </summary>
    internal class MainMenu
    {
        public event EventHandler StartButtonPressed;
        public event EventHandler SettingsButtonPressed;
        public event EventHandler QuitButtonPressed;

        Vector2 spacePosition, invadersPosition;
        float animationProgress;
        const float animationSpeed = 0.5f;
        List<Vector2> startScreenStars;

        public MainMenu()
        {
            var random = new Random();
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            spacePosition = new Vector2(-200, windowHeight / 5);
            invadersPosition = new Vector2(windowWidth + 200, windowHeight / 5 + 80);
            animationProgress = 0.0f;

            // Creating stars for the effect
            startScreenStars = new List<Vector2>(20);
            for (int i = 0; i < startScreenStars.Capacity; i++)
            {
                startScreenStars.Add(new Vector2(random.Next(0, windowWidth), random.Next(-windowHeight, -1)));
            }
        }

        public void Update()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // Title animation
            if (animationProgress < 1.0f)
            {
                animationProgress = Math.Min(animationProgress + Raylib.GetFrameTime() * animationSpeed, 1.0f);
            }

            int spaceWidth = Raylib.MeasureText("SPACE", 80);
            int invadersWidth = Raylib.MeasureText("INVADERS", 80);

            spacePosition.X = Lerp(-spaceWidth, (windowWidth / 2 - spaceWidth / 2), animationProgress);
            invadersPosition.X = Lerp(windowWidth, (windowWidth / 2 - invadersWidth / 2), animationProgress);

            // Update star positions
            for (int i = 0; i < startScreenStars.Count; i++)
            {
                var star = startScreenStars[i];
                star.Y = (star.Y + 40 * Raylib.GetFrameTime()) % windowHeight;
                startScreenStars[i] = star;
            }
        }

        public void ShowMenu()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            Raylib.DrawCircleGradient(windowWidth / 2, windowHeight / 2 - 100, windowHeight * 3.0f, Raylib.BLACK, Raylib.BLUE);

            // Draw stars
            foreach (var star in startScreenStars)
            {
                Raylib.DrawCircleGradient((int)star.X, (int)star.Y, 4.0f, Raylib.WHITE, Raylib.BLACK);
            }

            // Title
            Raylib.DrawText("SPACE", (int)spacePosition.X, (int)spacePosition.Y, 80, Raylib.LIGHTGRAY);
            Raylib.DrawText("INVADERS", (int)invadersPosition.X, (int)invadersPosition.Y, 80, Raylib.GREEN);

            // Menu buttons
            int buttonWidth = 100, buttonHeight = 40;
            int centerX = windowWidth / 2 - buttonWidth / 2;
            int centerY = windowHeight / 2 - buttonHeight / 2;

            if (RayGui.GuiButton(new Rectangle(centerX, centerY, buttonWidth, buttonHeight), "Start Game"))
                StartButtonPressed?.Invoke(this, EventArgs.Empty);

            if (RayGui.GuiButton(new Rectangle(centerX, centerY + buttonHeight * 2, buttonWidth, buttonHeight), "Options"))
                SettingsButtonPressed?.Invoke(this, EventArgs.Empty);

            if (RayGui.GuiButton(new Rectangle(centerX, centerY + buttonHeight * 4, buttonWidth, buttonHeight), "Quit"))
                QuitButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
