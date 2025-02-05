using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Main game process.
    /// </summary>
    internal class Invanders
    {
        enum GameState
        {
            StartMenu,
            SettingsMenu,
            Play,
            ScoreScreen
        }

        GameState state;
        bool gameRunning = true;

        const int WindowWidth = 640;
        const int WindowHeight = 720;

        double lastEnemyShootTime = 0; // Time of the last enemy shot
        double enemyShootInterval = 2.0; // Interval between enemy shots

        Player player;
        List<Bullet> bullets;
        List<Enemy> enemies;

        Camera2D camera; // Camera

        int enemiesDefeated = 0; // Number of defeated enemies

        List<Vector2> stars; // Coordinates of stars
        int numStars = 100; // Number of stars

        MainMenu mainMenu;
        SettingsMenu settingsMenu;

        int scoreCounter = 0;
        double gameStartTime, gameEndTime;

        Texture playerImage;
        List<Texture> enemyImages;
        Texture bulletImage;
        Sound enemyExplode;

        public void Run()
        {
            Init();
            GameLoop();
            Cleanup();
            Raylib.CloseWindow();
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
            enemyExplode = Raylib.LoadSound("data/audio/EnemyExplode.wav");

            // Loading enemy images
            enemyImages = new List<Texture>
            {
                Raylib.LoadTexture("data/images/enemyBlack1.png"),
                Raylib.LoadTexture("data/images/enemyBlack2.png"),
                Raylib.LoadTexture("data/images/enemyBlack3.png"),
                Raylib.LoadTexture("data/images/enemyBlack4.png")
            };

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

            state = GameState.StartMenu;

            // Initialize camera
            camera = new Camera2D
            {
                target = new Vector2(WindowWidth / 2, WindowHeight / 2),
                offset = new Vector2(WindowWidth / 2, WindowHeight / 2),
                zoom = 1.0f
            };
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        void Cleanup()
        {
            Raylib.UnloadTexture(playerImage);
            Raylib.UnloadSound(enemyExplode);

            foreach (var enemyImage in enemyImages)
                Raylib.UnloadTexture(enemyImage);
        }

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

        /// <summary>
        /// Resets game data before starting a new game.
        /// </summary>
        void ResetGame()
        {
            enemiesDefeated = 0; // Reset the number of defeated enemies

            gameStartTime = Raylib.GetTime();
            gameEndTime = 0;
            scoreCounter = 0;

            var playerStart = new Vector2(WindowWidth / 2, WindowHeight - 80);
            player = new Player(playerStart, Vector2.Zero, 120, 40, playerImage);

            bullets = new List<Bullet>();
            enemies = CreateEnemies();
        }

        /// <summary>
        /// Creates the enemy formation.
        /// </summary>
        List<Enemy> CreateEnemies()
        {
            var enemies = new List<Enemy>();
            const int numberOfEnemies = 20; // Увеличим количество врагов
            const int enemySize = 40;
            const int minSpeed = 40, maxSpeed = 100; // Скорость движения врагов
            const int minY = 0, maxY = WindowHeight / 2; // Разброс врагов по высоте (верхняя половина экрана)

            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Случайная позиция по оси X
                float randomX = Raylib.GetRandomValue(0, WindowWidth - enemySize);

                // Случайная высота на экране (не выше половины)
                float randomY = Raylib.GetRandomValue(minY, maxY);

                // Случайная скорость врага
                float randomSpeed = Raylib.GetRandomValue(minSpeed, maxSpeed);

                // Создаем врага с начальной позицией на верхней части экрана
                var position = new Vector2(randomX, randomY); // Начальная позиция врага
                enemies.Add(new Enemy(position, new Vector2(0, 1), randomSpeed, enemySize, enemyImages[(int)Raylib.GetRandomValue(0, enemyImages.Count - 1)], 10)); // Враг двигается вниз
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
        }

        void UpdateStartMenu()
        {
            mainMenu.Update();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);
            mainMenu.ShowMenu();
            Raylib.EndDrawing();
        }

        void UpdateSettingsMenu()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.DARKGRAY);
            settingsMenu.ShowMenu();
            Raylib.EndDrawing();
        }

        void UpdateGame()
        {
            UpdateStars();
            UpdateCamera();

            if (player.Update(WindowWidth, WindowHeight))
            {
                var bulletStart = player.transform.position + new Vector2(20, -20);
                bullets.Add(new Bullet(bulletStart, new Vector2(0, -1), 300, 16, bulletImage, Raylib.RED));
            }

            UpdateEnemies();
            UpdateBullets();
            CheckCollisions();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            DrawBackground();

            Raylib.BeginMode2D(camera);

            DrawGameObjects();
            Raylib.EndMode2D();

            Raylib.EndDrawing();
        }

        void DrawBackground()
        {
            foreach (var star in stars)
            {
                Raylib.DrawCircle((int)star.X, (int)star.Y, 2, Raylib.WHITE);
            }
        }


        void UpdateCamera()
        {
            camera.target = player.transform.position;

            float cameraSpeed = 200 * Raylib.GetFrameTime();

            // Allow moving the camera using arrow keys (up/down)
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) camera.target.Y -= cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) camera.target.Y += cameraSpeed;

            // Make sure the camera stays within the game boundaries
            camera.target.Y = Math.Clamp(camera.target.Y, WindowHeight / 2, WindowHeight - 100);
        }

        void DrawGameObjects()
        {
            // Piirrä pelaaja
            player.Draw();

            // Piirrä viholliset ja luodit vain, jos ne ovat kameran alueella
            Rectangle cameraBounds = GetCameraBounds();
            foreach (var enemy in enemies)
            {
                if (enemy.active && Raylib.CheckCollisionRecs(GetRectangle(enemy.transform, enemy.collision), cameraBounds))
                {
                    enemy.Draw();
                }
            }

            foreach (var bullet in bullets)
            {
                if (bullet.active && Raylib.CheckCollisionRecs(GetRectangle(bullet.transform, bullet.collision), cameraBounds))
                {
                    bullet.Draw();
                }
            }
        }

        Rectangle GetCameraBounds()
        {
            float left = camera.target.X - (camera.offset.X / camera.zoom);
            float top = camera.target.Y - (camera.offset.Y / camera.zoom);
            float width = WindowWidth / camera.zoom;
            float height = WindowHeight / camera.zoom;

            return new Rectangle(left, top, width, height);
        }

        void UpdateScoreScreen()
        {
            string resultText = player.active ? "You won!" : "Defeat";

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

        void UpdateStars()
        {
            float starSpeed = 40.0f; // Speed of stars
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

        void UpdateEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.active)
                {
                    // Обновляем позицию врага (движение вниз)
                    enemy.Update();

                    // Если враг выходит за пределы экрана, деактивируем его
                    if (enemy.transform.position.Y > WindowHeight)
                    {
                        enemy.active = false;
                    }
                }
            }

            // Проверка, если все враги уничтожены
            if (!enemies.Any(e => e.active))
            {
                gameEndTime = Raylib.GetTime();
                state = GameState.ScoreScreen;
            }
        }


        Enemy FindShootingEnemy()
        {
            var activeEnemies = enemies.Where(e => e.active).ToList();
            if (activeEnemies.Count > 0)
            {
                Random random = new Random();
                return activeEnemies[random.Next(activeEnemies.Count)];
            }
            return null;
        }

        void UpdateBullets()
        {
            foreach (var bullet in bullets)
            {
                if (bullet.active)
                {
                    bullet.Update();
                    if (bullet.transform.position.Y < 0) bullet.active = false;
                }
            }
        }

        void CheckCollisions()
        {
            Rectangle playerRect = GetRectangle(player.transform, player.collision);

            foreach (var enemy in enemies)
            {
                if (!enemy.active) continue;

                Rectangle enemyRect = GetRectangle(enemy.transform, enemy.collision);

                foreach (var bullet in bullets)
                {
                    if (!bullet.active) continue;

                    Rectangle bulletRect = GetRectangle(bullet.transform, bullet.collision);

                    if (bullet.transform.direction.Y < 0) // Player bullet
                    {
                        if (Raylib.CheckCollisionRecs(bulletRect, enemyRect))
                        {
                            enemy.active = false;
                            bullet.active = false;
                            scoreCounter += enemy.scoreValue;
                            enemiesDefeated++;
                            Raylib.PlaySound(enemyExplode);

                            if (!enemies.Any(e => e.active))
                            {
                                gameEndTime = Raylib.GetTime();
                                state = GameState.ScoreScreen;
                            }

                            break;
                        }
                    }
                    else if (bullet.transform.direction.Y > 0) // Enemy bullet
                    {
                        if (Raylib.CheckCollisionRecs(bulletRect, playerRect))
                        {
                            bullet.active = false;
                            player.active = false;

                            gameEndTime = Raylib.GetTime();
                            state = GameState.ScoreScreen;

                            break;
                        }
                    }
                }
            }
        }

        void DrawGame()
        {
            Raylib.DrawCircleGradient(WindowWidth / 2, WindowHeight / 2, WindowHeight, Raylib.BLACK, Raylib.BLUE);

            foreach (var star in stars)
            {
                Raylib.DrawCircle((int)star.X, (int)star.Y, 2, Raylib.WHITE);
            }

            player.Draw();
            foreach (var enemy in enemies) if (enemy.active) enemy.Draw();
            foreach (var bullet in bullets) if (bullet.active) bullet.Draw();

            Raylib.DrawText($"Score: {scoreCounter}", 10, 10, 20, Raylib.WHITE);
        }

        void DrawScoreScreen(string resultText)
        {
            string scoreText = $"Final score: {scoreCounter}";
            string enemiesText = $"Enemies defeated: {enemiesDefeated}";
            string timeText = $"Game time: {(gameEndTime - gameStartTime):F2} seconds";
            string instructionText = "Press Enter to play again";

            int fontSize = 20;

            int scoreWidth = Raylib.MeasureText(scoreText, fontSize);
            int enemiesWidth = Raylib.MeasureText(enemiesText, fontSize);
            int timeWidth = Raylib.MeasureText(timeText, fontSize);
            int resultWidth = Raylib.MeasureText(resultText, fontSize + 10);
            int instructionWidth = Raylib.MeasureText(instructionText, fontSize);

            Raylib.DrawCircleGradient(WindowWidth / 2, WindowHeight / 2 - 100, WindowHeight * 3.0f, Raylib.BLACK, Raylib.BLUE);

            Color resultColor = resultText == "You won!" ? Raylib.GREEN : Raylib.RED;
            Raylib.DrawText(resultText, WindowWidth / 2 - resultWidth / 2, WindowHeight / 2 - 120, fontSize + 10, resultColor);
            Raylib.DrawText(scoreText, WindowWidth / 2 - scoreWidth / 2, WindowHeight / 2 - 80, fontSize, Raylib.WHITE);
            Raylib.DrawText(enemiesText, WindowWidth / 2 - enemiesWidth / 2, WindowHeight / 2 - 60, fontSize, Raylib.WHITE);
            Raylib.DrawText(timeText, WindowWidth / 2 - timeWidth / 2, WindowHeight / 2 - 40, fontSize, Raylib.WHITE);
            Raylib.DrawText(instructionText, WindowWidth / 2 - instructionWidth / 2, WindowHeight / 2, fontSize, Raylib.WHITE);
        }

        Rectangle GetRectangle(TransformComponent transform, CollisionComponent collision)
        {
            return new Rectangle(transform.position.X, transform.position.Y, collision.size.X, collision.size.Y);
        }
    }
}