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

        int enemiesDefeated = 0; // Number of defeated enemies
        int currentLevelIndex = 1; // Current level index
        const int totalLevels = 2; // Total number of levels

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

        Camera2D camera;

        int enemyDirection = 1; // 1 = вправо, -1 = влево



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

        LevelData currentLevel;
        Dictionary<int, Texture> tilesetTextures = new Dictionary<int, Texture>();

        void ResetGame()
        {
            enemiesDefeated = 0;
            gameStartTime = Raylib.GetTime();
            gameEndTime = 0;
            scoreCounter = 0;

            //var playerStart = new Vector2(WindowWidth / 2, WindowHeight - 40);
            //player = new Player(playerStart, Vector2.Zero, 120, 40, playerImage);

            bullets = new List<Bullet>();

            // Загружаем Level1 как фон (но не врагов)
            currentLevel = LevelLoader.LoadLevel($"data/levels/Level{currentLevelIndex}.tmx");

            float playerX = currentLevel.Map.Width * currentLevel.Map.TileWidth / 2;
            float playerY = currentLevel.Map.Height * currentLevel.Map.TileHeight - 60;
            player = new Player(new Vector2(playerX, playerY), Vector2.Zero, 120, 40, playerImage);

            // Загружаем фоновые тайлы
            tilesetTextures.Clear();
            foreach (var tileset in currentLevel.Map.Tilesets)
            {
                string tilesetPath = "data/images/Palette.png";
                if (File.Exists(tilesetPath))
                {
                    tilesetTextures[tileset.FirstGid] = Raylib.LoadTexture(tilesetPath);
                }
                else
                {
                    Console.WriteLine($"ERROR: File not found -> {tilesetPath}");
                }
            }

            // Создаем врагов вручную
            //var manualEnemies = CreateEnemiesManually();
            //enemies = CreateEnemies(manualEnemies);
            //Console.WriteLine($"Enemies created manually: {enemies.Count}");


            enemies = CreateSpaceInvadersEnemies();
            Console.WriteLine($"Enemies created: {enemies.Count}");

            camera = new Camera2D
            {
                offset = new Vector2(WindowWidth / 2, WindowHeight / 2), // Центрируем игрока в окне
                target = player.transform.position, // Камера на игроке
                rotation = 0,
                zoom = 1.0f
            };

        }

        List<Enemy> CreateSpaceInvadersEnemies()
        {
            var enemies = new List<Enemy>();

            int rows = 4; // Количество рядов
            int cols = 4; // Количество врагов в ряду
            int startX = 100; // Отступ от левого края
            int startY = 50; // Отступ сверху
            int spacingX = 60; // Расстояние между врагами по X
            int spacingY = 50; // Расстояние между рядами по Y

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector2 position = new Vector2(startX + col * spacingX, startY + row * spacingY);
                    int enemyType = (row % 2) + 1; // Чередуем тип врага

                    Texture enemyTexture = enemyImages[enemyType - 1];
                    int enemyScore = enemyType == 1 ? 10 : 20;

                    enemies.Add(new Enemy(position, Vector2.Zero, 60, 40, enemyTexture, enemyScore));
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
            player.Update(currentLevel.Map.Width * currentLevel.Map.TileWidth, currentLevel.Map.Height * currentLevel.Map.TileHeight); // Передаем размер карты

            UpdateCamera(); // Обновляем камеру
            UpdateEnemies();
            UpdateBullets();
            CheckCollisions();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                PlayerShoot();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            Raylib.BeginMode2D(camera); // Включаем камеру
            DrawBackground();
            DrawGameObjects();
            Raylib.EndMode2D(); // Выключаем камеру

            Raylib.EndDrawing();
        }

        void PlayerShoot()
        {
            float bulletSpacing = 10; // Расстояние между пулями
            Vector2 bulletStart = player.transform.position + new Vector2(20, -20);

            for (int i = -1; i <= 1; i++) // Стреляем 3 пулями
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
        }


        void UpdateCamera()
        {
            camera.target = player.transform.position; // Центрируем камеру на игроке

            float minY = WindowHeight / 2;
            float maxY = currentLevel.Map.Height * currentLevel.Map.TileHeight - WindowHeight / 2;

            camera.target.Y = Math.Clamp(camera.target.Y, minY, maxY);
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
            float enemySpeed = 30.0f * Raylib.GetFrameTime(); // Скорость врагов
            bool changeDirection = false;

            foreach (var enemy in enemies)
            {
                if (!enemy.active) continue;

                enemy.transform.position.X += enemyDirection * enemySpeed;

                // Проверяем, достиг ли враг края экрана
                if (enemy.transform.position.X < 20 || enemy.transform.position.X > WindowWidth - 80)
                {
                    changeDirection = true;
                }
            }

            // Если хотя бы один враг достиг края — меняем направление и опускаем их вниз
            if (changeDirection)
            {
                enemyDirection *= -1; // Меняем направление

                foreach (var enemy in enemies)
                {
                    enemy.transform.position.Y += 10; // Опускаем врагов вниз
                }
            }

            // Враги стреляют каждые 2 секунды
            if (Raylib.GetTime() - lastEnemyShootTime > enemyShootInterval)
            {
                EnemyShoot();
                lastEnemyShootTime = Raylib.GetTime();
            }

            Console.WriteLine($"Enemies alive: {enemies.Count(e => e.active)}");

            if (!enemies.Any(e => e.active))
            {
                Console.WriteLine("All enemies defeated, transitioning to ScoreScreen.");
                gameEndTime = Raylib.GetTime();
                state = GameState.ScoreScreen;
            }
        }



        void EnemyShoot()
        {
            Random random = new Random();
            List<Enemy> activeEnemies = enemies.Where(e => e.active).ToList();

            if (activeEnemies.Count > 0)
            {
                Enemy shooter = activeEnemies[random.Next(activeEnemies.Count)]; // Случайный враг стреляет
                var bulletStart = shooter.transform.position + new Vector2(20, 20);
                bullets.Add(new Bullet(bulletStart, new Vector2(0, 1), 200, 16, bulletImage, Raylib.RED)); // Вражеская пуля
            }
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
                                // Transition to next level
                                currentLevelIndex++;
                                if (currentLevelIndex > totalLevels)
                                {
                                    // If it was the last level, end the game
                                    currentLevelIndex = 1; // or handle game end differently
                                }
                                state = GameState.ScoreScreen;

                                // Debugging log
                                Console.WriteLine("All enemies defeated, transitioning to ScoreScreen.");
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

        void DrawGameObjects()
        {
            player.Draw();
            foreach (var enemy in enemies)
            {
                if (enemy.active) enemy.Draw();
            }
            foreach (var bullet in bullets)
            {
                if (bullet.active) bullet.Draw();
            }
            Raylib.DrawText($"Score: {scoreCounter}", 10, 10, 20, Raylib.WHITE);
        }

        void DrawBackground()
        {
            if (currentLevel == null || currentLevel.Map == null) return;

            foreach (var layer in currentLevel.Map.Layers)
            {
                if (!layer.Visible || !(layer is TmxLayer tileLayer)) continue;

                int layerWidth = currentLevel.Map.Width;  // Получаем ширину карты
                int layerHeight = currentLevel.Map.Height; // Получаем высоту карты
                int tileSize = currentLevel.Map.TileWidth; // Размер тайла

                for (int y = 0; y < layerHeight; y++)
                {
                    for (int x = 0; x < layerWidth; x++)
                    {
                        int tileIndex = x + y * layerWidth; // Индекс тайла в массиве
                        if (tileIndex >= tileLayer.Tiles.Count) continue; // Защита от выхода за границы массива

                        int tileId = tileLayer.Tiles[tileIndex].Gid;
                        if (tileId == 0) continue; // Пропускаем пустые тайлы

                        foreach (var tileset in tilesetTextures)
                        {
                            if (tileId >= tileset.Key)
                            {
                                Texture texture = tileset.Value;
                                Rectangle sourceRect = new Rectangle((tileId - tileset.Key) * tileSize, 0, tileSize, tileSize);
                                Vector2 position = new Vector2(x * tileSize, y * tileSize);

                                Raylib.DrawTextureRec(texture, sourceRect, position, Raylib.WHITE);
                                break;
                            }
                        }
                    }
                }
            }
        }



        Rectangle GetRectangle(TransformComponent transform, CollisionComponent collision)
        {
            return new Rectangle(transform.position.X, transform.position.Y, collision.size.X, collision.size.Y);
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
    }
}