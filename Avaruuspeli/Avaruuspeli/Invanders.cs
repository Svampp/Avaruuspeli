using Raylib_CsLo;
using System.Numerics;
using System.Collections.Generic;
using TiledSharp;

namespace Avaruuspeli
{
    /// <summary>
    /// Main game process.
    /// </summary>
    internal class Invanders
    {
        // Enum to represent different states of the game
        enum GameState
        {
            StartMenu,
            SettingsMenu,
            Play,
            ScoreScreen
        }

        GameState state; // Current state of the game
        bool gameRunning = true; // Flag to control the game loop

        const int WindowWidth = 640; // Width of the game window
        const int WindowHeight = 720; // Height of the game window

        double lastEnemyShootTime = 0; // Time of the last enemy shot
        const double baseEnemyShootInterval = 2.0; // Base interval between enemy shots
        double enemyShootInterval = baseEnemyShootInterval; // Current interval between enemy shots

        Player player; // Player object
        List<Bullet> bullets; // List of bullets in the game
        List<Enemy> enemies; // List of enemies in the game
        List<Explosion> explosions = new List<Explosion>(); // List of explosions in the game

        int enemiesDefeated = 0; // Number of defeated enemies
        int currentLevelIndex = 1; // Current level index
        const int totalLevels = 2; // Total number of levels

        List<Vector2> stars; // Coordinates of stars in the background
        int numStars = 100; // Number of stars in the background

        MainMenu mainMenu; // Main menu object
        SettingsMenu settingsMenu; // Settings menu object

        int scoreCounter = 0; // Player's score
        double gameStartTime, gameEndTime; // Timestamps for game start and end

        Texture playerImage; // Texture for the player
        List<Texture> enemyImages; // List of textures for enemies
        Texture bulletImage; // Texture for bullets
        Music backgroundMusic; // Background music
        Sound playerShootSound; // Sound for player shooting
        Sound enemyShootSound; // Sound for enemy shooting

        Camera2D camera; // Camera for the game

        int enemyDirection = 1; // Direction of enemy movement (1 = right, -1 = left)
        const float baseEnemySpeed = 150; // Base speed of enemies
        float enemySpeed = baseEnemySpeed; // Current speed of enemies

        /// <summary>
        /// Main method to run the game.
        /// </summary>
        public void Run()
        {
            Init(); // Initialize the game
            GameLoop(); // Start the game loop
            Cleanup(); // Clean up resources
            Raylib.CloseWindow(); // Close the game window
        }

        /// <summary>
        /// Initializes the game.
        /// </summary>
        void Init()
        {
            Raylib.InitWindow(WindowWidth, WindowHeight, "Space Invaders Demo");
            Raylib.SetTargetFPS(60);
            Raylib.InitAudioDevice();

            // Loading resources
            playerImage = Raylib.LoadTexture("data/images/player.png");
            bulletImage = Raylib.LoadTexture("data/images/laserGreen13.png");
            playerShootSound = Raylib.LoadSound("data/sounds/PlayerShoot.wav");
            enemyShootSound = Raylib.LoadSound("data/sounds/EnemyShoot.wav");

            // Play background music
            backgroundMusic = Raylib.LoadMusicStream("data/sounds/Background.wav");
            Raylib.PlayMusicStream(backgroundMusic);
            Raylib.SetMusicVolume(backgroundMusic, 1f);

            // Loading enemy images
            enemyImages = new List<Texture>
            {
                Raylib.LoadTexture("data/images/enemyBlack1.png"),
                Raylib.LoadTexture("data/images/enemyBlack2.png"),
                Raylib.LoadTexture("data/images/enemyBlack3.png"),
                Raylib.LoadTexture("data/images/enemyBlack4.png")
            };

            // Initialize stars for the background
            stars = new List<Vector2>(numStars);
            Random random = new Random();
            for (int i = 0; i < numStars; i++)
            {
                stars.Add(new Vector2(random.Next(0, WindowWidth), random.Next(0, WindowHeight)));
            }

            // Creating menus
            mainMenu = new MainMenu();
            mainMenu.StartButtonPressed += OnStartButtonPressed;
            mainMenu.SettingsButtonPressed += OnSettingsButtonPressed;
            mainMenu.QuitButtonPressed += OnQuitButtonPressed;

            settingsMenu = new SettingsMenu();
            settingsMenu.BackButtonPressed += OnSettingsBackButtonPressed;

            state = GameState.StartMenu; // Set initial state to StartMenu
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        void Cleanup()
        {
            Raylib.UnloadTexture(playerImage);
            Raylib.UnloadMusicStream(backgroundMusic);
            Raylib.UnloadSound(playerShootSound);
            Raylib.UnloadSound(enemyShootSound);
            foreach (var enemyImage in enemyImages)
                Raylib.UnloadTexture(enemyImage);
        }

        // Event handlers for menu buttons
        void OnStartButtonPressed(object sender, EventArgs e)
        {
            ResetGame();
            state = GameState.Play;
        }

        void OnSettingsButtonPressed(object sender, EventArgs e)
        {
            state = GameState.SettingsMenu;
        }

        void OnQuitButtonPressed(object sender, EventArgs e)
        {
            gameRunning = false;
        }

        void OnSettingsBackButtonPressed(object sender, EventArgs e)
        {
            state = GameState.StartMenu;
        }

        LevelData currentLevel; // Current level data
        Dictionary<int, Texture> tilesetTextures = new Dictionary<int, Texture>(); // Textures for tiles

        /// <summary>
        /// Resets the game to its initial state.
        /// </summary>
        void ResetGame()
        {
            enemiesDefeated = 0;
            gameStartTime = Raylib.GetTime();
            gameEndTime = 0;
            scoreCounter = 0;

            bullets = new List<Bullet>();

            enemySpeed = baseEnemySpeed; // Reset enemy speed to base value
            enemyShootInterval = baseEnemyShootInterval; // Reset enemy shoot interval to base value

            currentLevel = LevelLoader.LoadLevel($"data/levels/Level{currentLevelIndex}.tmx");

            float playerX = currentLevel.Map.Width * currentLevel.Map.TileWidth / 2;
            float playerY = currentLevel.Map.Height * currentLevel.Map.TileHeight - 60;
            player = new Player(new Vector2(playerX, playerY), Vector2.Zero, 120, 40, playerImage);

            // Load background tiles
            tilesetTextures.Clear();
            foreach (var tileset in currentLevel.Map.Tilesets)
            {
                string tilesetPath = "data/images/Palette.png";
                if (File.Exists(tilesetPath))
                {
                    tilesetTextures[tileset.FirstGid] = Raylib.LoadTexture(tilesetPath);
                }
            }

            enemies = CreateSpaceInvadersEnemies();
            Console.WriteLine($"Enemies created: {enemies.Count}");

            camera = new Camera2D
            {
                offset = new Vector2(WindowWidth / 2, WindowHeight / 2), // Center the camera on the player
                target = player.transform.position, // Camera follows the player
                rotation = 0,
                zoom = 1.0f
            };
        }

        /// <summary>
        /// Creates a list of enemies for the Space Invaders game.
        /// </summary>
        List<Enemy> CreateSpaceInvadersEnemies()
        {
            var enemies = new List<Enemy>();

            int rows = 4; // Number of rows
            int cols = 5; // Number of enemies per row
            int startX = 100; // Offset from the left edge
            int startY = 50; // Offset from the top
            int spacingX = 60; // Horizontal spacing between enemies
            int spacingY = 50; // Vertical spacing between rows

            // Adjust enemy settings for Level 2
            if (currentLevelIndex == 2)
            {
                rows = 8; // Number of rows
                cols = 5; // Number of columns
                enemySpeed = baseEnemySpeed * 1.5f; // Increase enemy speed by 1.5 times
                enemyShootInterval = baseEnemyShootInterval / 3; // Triple the shooting frequency
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector2 position = new Vector2(startX + col * spacingX, startY + row * spacingY);
                    int enemyType = row % enemyImages.Count; // Alternate enemy types

                    Texture enemyTexture = enemyImages[enemyType];
                    int enemyScore = Math.Max((4 - row) * 10, 10); // Ensure score is non-negative

                    enemies.Add(new Enemy(position, new Vector2(1, 0), enemySpeed, 40, enemyTexture, enemyScore));
                }
            }

            return enemies;
        }

        /// <summary>
        /// Main game loop.
        /// </summary>
        void GameLoop()
        {
            while (gameRunning && !Raylib.WindowShouldClose())
            {
                switch (state)
                {
                    case GameState.StartMenu:
                        UpdateStartMenu();
                        break;
                    case GameState.SettingsMenu:
                        UpdateSettingsMenu();
                        break;
                    case GameState.Play:
                        UpdateGame();
                        break;
                    case GameState.ScoreScreen:
                        UpdateScoreScreen();
                        break;
                }
            }
            if (!Raylib.IsMusicStreamPlaying(backgroundMusic))
            {
                Raylib.PlayMusicStream(backgroundMusic);
            }
        }

        /// <summary>
        /// Updates the start menu.
        /// </summary>
        void UpdateStartMenu()
        {
            mainMenu.Update();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);
            mainMenu.ShowMenu();
            Raylib.EndDrawing();
        }

        /// <summary>
        /// Updates the settings menu.
        /// </summary>
        void UpdateSettingsMenu()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.DARKGRAY);
            settingsMenu.ShowMenu();
            Raylib.EndDrawing();
        }

        /// <summary>
        /// Updates the game state.
        /// </summary>
        void UpdateGame()
        {
            Raylib.UpdateMusicStream(backgroundMusic);

            UpdateStars();
            player.Update(currentLevel.Map.Width * currentLevel.Map.TileWidth, currentLevel.Map.Height * currentLevel.Map.TileHeight); // Pass map size

            UpdateCamera(); // Update the camera
            UpdateEnemies();
            UpdateBullets();
            CheckCollisions();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                PlayerShoot();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            Raylib.BeginMode2D(camera);
            DrawBackground();
            DrawGameObjects();
            Raylib.EndMode2D(); // Disable camera

            DrawHUD(); // Draw the HUD on top

            Raylib.EndDrawing();

            float deltaTime = Raylib.GetFrameTime();
            foreach (var explosion in explosions)
            {
                explosion.Update(deltaTime);
            }
            explosions.RemoveAll(e => e.IsFinished());
        }

        /// <summary>
        /// Draws the HUD (Heads-Up Display).
        /// </summary>
        void DrawHUD()
        {
            string scoreText = $"Score: {scoreCounter}";
            int fontSize = 20;

            // Draw the score text at the top of the screen
            Raylib.DrawText(scoreText, 10, 10, fontSize, Raylib.WHITE);
        }

        /// <summary>
        /// Handles player shooting.
        /// </summary>
        void PlayerShoot()
        {
            float bulletSpacing = 10; // Spacing between bullets
            Vector2 bulletStart = player.transform.position + new Vector2(20, -20);

            for (int i = -1; i <= 1; i++) // Shoot 3 bullets
            {
                bullets.Add(new Bullet(
                    bulletStart + new Vector2(i * bulletSpacing, 0),
                    new Vector2(0, -1),
                    300,
                    16,
                    bulletImage,
                    Raylib.WHITE
                ));
            }

            Raylib.PlaySound(playerShootSound);
        }

        /// <summary>
        /// Updates the camera to follow the player.
        /// </summary>
        void UpdateCamera()
        {
            camera.target = player.transform.position; // Center the camera on the player

            float minX = WindowWidth / 2;
            float maxX = currentLevel.Map.Width * currentLevel.Map.TileWidth - WindowWidth / 2;
            float minY = WindowHeight / 2;
            float maxY = currentLevel.Map.Height * currentLevel.Map.TileHeight - WindowHeight / 2;

            // Clamp the camera to stay within the map boundaries
            camera.target.X = Math.Clamp(camera.target.X, minX, maxX);
            camera.target.Y = Math.Clamp(camera.target.Y, minY, maxY);
        }

        /// <summary>
        /// Updates the score screen.
        /// </summary>
        void UpdateScoreScreen()
        {
            string resultText = player.active ? "You won!" : "Game Over! You lost.";

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                ResetGame();
                state = GameState.Play;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.DARKGRAY);
            DrawScoreScreen(resultText);
            Raylib.EndDrawing();
        }

        /// <summary>
        /// Updates the positions of the stars in the background.
        /// </summary>
        void UpdateStars()
        {
            float starSpeed = 70.0f; // Speed of stars
            for (int i = 0; i < stars.Count; i++)
            {
                Vector2 star = stars[i];
                star.Y += starSpeed * Raylib.GetFrameTime(); // Stars move down

                // If a star goes off-screen, reset it to the top
                if (star.Y > WindowHeight)
                {
                    star.Y = 0;
                    star.X = new Random().Next(0, WindowWidth);
                }

                stars[i] = star;
            }
        }

        /// <summary>
        /// Handles the player losing the game.
        /// </summary>
        void PlayerLose()
        {
            gameEndTime = Raylib.GetTime();
            player.active = false;
            state = GameState.ScoreScreen;
        }

        /// <summary>
        /// Updates the positions and states of the enemies.
        /// </summary>
        void UpdateEnemies()
        {
            float deltaTime = Raylib.GetFrameTime();
            bool changeDirection = false;

            for (int i = enemies.Count - 1; i >= 0; i--) // Iterate through the list from the end
            {
                var enemy = enemies[i];

                if (!enemy.active)
                {
                    enemies.RemoveAt(i); // Remove dead enemy
                    continue;
                }

                enemy.transform.position.X += enemyDirection * enemySpeed * deltaTime;

                // If an enemy reaches the edge of the screen, change direction
                if (enemy.transform.position.X < 20 || enemy.transform.position.X > WindowWidth - 80)
                {
                    changeDirection = true;
                }

                Rectangle enemyRect = GetRectangle(enemy.transform, enemy.collision);
                Rectangle playerRect = GetRectangle(player.transform, player.collision);

                // Check for collision between enemy and player
                if (Raylib.CheckCollisionRecs(enemyRect, playerRect))
                {
                    PlayerLose();
                    return;
                }
            }

            // If any enemy reaches the edge, change direction and move down
            if (changeDirection)
            {
                enemyDirection *= -1;

                foreach (var enemy in enemies)
                {
                    enemy.transform.position.Y += 10;
                }
            }

            // Enemies shoot every 2 seconds
            if (Raylib.GetTime() - lastEnemyShootTime > enemyShootInterval)
            {
                EnemyShoot();
                lastEnemyShootTime = Raylib.GetTime();
            }
        }

        /// <summary>
        /// Handles enemy shooting.
        /// </summary>
        void EnemyShoot()
        {
            Random random = new Random();
            List<Enemy> activeEnemies = enemies.Where(e => e.active).ToList();

            if (activeEnemies.Count > 0)
            {
                // In Level 2, three enemies shoot simultaneously
                int numShooters = currentLevelIndex == 2 ? 3 : 1;

                for (int i = 0; i < numShooters; i++)
                {
                    Enemy shooter = activeEnemies[random.Next(activeEnemies.Count)]; // Random enemy shoots
                    var bulletStart = shooter.transform.position + new Vector2(20, 20);

                    bullets.Add(new Bullet(bulletStart, new Vector2(0, 1), 200, 16, bulletImage, Raylib.RED)); // Enemy bullet

                    Raylib.PlaySound(enemyShootSound);
                }
            }
        }

        /// <summary>
        /// Updates the positions and states of the bullets.
        /// </summary>
        void UpdateBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--) // Iterate through the list from the end
            {
                var bullet = bullets[i];

                if (bullet.active)
                {
                    bullet.Update();

                    // Remove bullets that are off-screen
                    if (bullet.transform.position.Y < -50 || bullet.transform.position.Y > currentLevel.Map.Height * currentLevel.Map.TileHeight + 50)
                    {
                        bullets.RemoveAt(i); // Remove the bullet from the list
                    }
                }
            }
        }
        void CheckCollisions()
        {
            // Get the collision rectangle for the player
            Rectangle playerRect = GetRectangle(player.transform, player.collision);

            // Iterate through the enemy list in reverse order to allow safe removal
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];

                if (!enemy.active)
                {
                    // Remove inactive enemies from the list
                    enemies.RemoveAt(i);
                    continue;
                }

                // Get the collision rectangle for the enemy
                Rectangle enemyRect = GetRectangle(enemy.transform, enemy.collision);

                // Check if the player collides with an enemy → Game Over
                if (Raylib.CheckCollisionRecs(playerRect, enemyRect))
                {
                    PlayerLose();
                    return;
                }

                // Check for bullet collisions with enemies
                foreach (var bullet in bullets)
                {
                    if (!bullet.active) continue;

                    Rectangle bulletRect = GetRectangle(bullet.transform, bullet.collision);

                    if (bullet.transform.direction.Y < 0) // Player bullet moving upwards
                    {
                        if (Raylib.CheckCollisionRecs(bulletRect, enemyRect))
                        {
                            // Add an explosion effect at the enemy's position
                            explosions.Add(new Explosion(enemy.transform.position));
                            // Remove the enemy from the list
                            enemies.RemoveAt(i);
                            bullet.active = false;
                            // Increase score
                            scoreCounter += enemy.scoreValue;
                            enemiesDefeated++;

                            // If all enemies are defeated, transition to ScoreScreen
                            if (!enemies.Any(e => e.active))
                            {
                                gameEndTime = Raylib.GetTime();
                                currentLevelIndex++;
                                if (currentLevelIndex > totalLevels)
                                {
                                    currentLevelIndex = 1;
                                }
                                state = GameState.ScoreScreen;
                            }
                            break;
                        }
                    }
                    else if (bullet.transform.direction.Y > 0) // Enemy bullet moving downwards
                    {
                        if (Raylib.CheckCollisionRecs(bulletRect, playerRect))
                        {
                            bullet.active = false;
                            player.active = false;
                            PlayerLose();
                            return;
                        }
                    }
                }
            }
        }

        void DrawGameObjects()
        {
            // Draw the player
            player.Draw();

            // Draw active enemies
            foreach (var enemy in enemies)
            {
                if (enemy.active) enemy.Draw();
            }

            // Draw active bullets
            foreach (var bullet in bullets)
            {
                if (bullet.active) bullet.Draw();
            }

            // Draw explosion effects
            foreach (var explosion in explosions)
            {
                explosion.Draw();
            }
        }

        void DrawBackground()
        {
            // Check if the current level and its map are valid
            if (currentLevel == null || currentLevel.Map == null) return;

            // Iterate through all map layers
            foreach (var layer in currentLevel.Map.Layers)
            {
                if (!layer.Visible || !(layer is TmxLayer tileLayer)) continue;

                // Get map dimensions
                int layerWidth = currentLevel.Map.Width;
                int layerHeight = currentLevel.Map.Height;
                int tileSize = currentLevel.Map.TileWidth;

                // Iterate over each tile position in the layer
                for (int y = 0; y < layerHeight; y++)
                {
                    for (int x = 0; x < layerWidth; x++)
                    {
                        int tileIndex = x + y * layerWidth;
                        if (tileIndex >= tileLayer.Tiles.Count) continue; // Prevent out-of-bounds access

                        int tileId = tileLayer.Tiles[tileIndex].Gid;
                        if (tileId == 0) continue; // Skip empty tiles

                        // Find the correct tileset texture
                        foreach (var tileset in tilesetTextures)
                        {
                            if (tileId >= tileset.Key)
                            {
                                Texture texture = tileset.Value;
                                Rectangle sourceRect = new Rectangle((tileId - tileset.Key) * tileSize, 0, tileSize, tileSize);
                                Vector2 position = new Vector2(x * tileSize, y * tileSize);

                                // Draw the tile on the screen
                                Raylib.DrawTextureRec(texture, sourceRect, position, Raylib.WHITE);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Helper function to get the collision rectangle of a game object
        Rectangle GetRectangle(TransformComponent transform, CollisionComponent collision)
        {
            return new Rectangle(transform.position.X, transform.position.Y, collision.size.X, collision.size.Y);
        }

        void DrawScoreScreen(string resultText)
        {
            // Format score, enemies defeated, and game time
            string scoreText = $"Final score: {scoreCounter}";
            string enemiesText = $"Enemies defeated: {enemiesDefeated}";
            string timeText = $"Game time: {(gameEndTime - gameStartTime):F2} seconds";
            string instructionText = "Press Enter to play again";

            int fontSize = 20;

            // Calculate text widths for proper centering
            int scoreWidth = Raylib.MeasureText(scoreText, fontSize);
            int enemiesWidth = Raylib.MeasureText(enemiesText, fontSize);
            int timeWidth = Raylib.MeasureText(timeText, fontSize);
            int resultWidth = Raylib.MeasureText(resultText, fontSize + 10);
            int instructionWidth = Raylib.MeasureText(instructionText, fontSize);

            // Draw background gradient
            Raylib.DrawCircleGradient(WindowWidth / 2, WindowHeight / 2 - 100, WindowHeight * 3.0f, Raylib.BLACK, Raylib.BLUE);

            // Choose result text color (green for win, red for loss)
            Color resultColor = resultText == "You won!" ? Raylib.GREEN : Raylib.RED;

            // Draw game result and statistics
            Raylib.DrawText(resultText, WindowWidth / 2 - resultWidth / 2, WindowHeight / 2 - 120, fontSize + 10, resultColor);
            Raylib.DrawText(scoreText, WindowWidth / 2 - scoreWidth / 2, WindowHeight / 2 - 80, fontSize, Raylib.WHITE);
            Raylib.DrawText(enemiesText, WindowWidth / 2 - enemiesWidth / 2, WindowHeight / 2 - 60, fontSize, Raylib.WHITE);
            Raylib.DrawText(timeText, WindowWidth / 2 - timeWidth / 2, WindowHeight / 2 - 40, fontSize, Raylib.WHITE);
            Raylib.DrawText(instructionText, WindowWidth / 2 - instructionWidth / 2, WindowHeight / 2, fontSize, Raylib.WHITE);
        }

    }
}