using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Player class, handles movement, shooting, and rendering.
    /// </summary>
    internal class Player
    {
        public TransformComponent transform { get; private set; } // Position and movement
        public CollisionComponent collision; // Dimensions for collision detection
        SpriteRendererComponent spriteRenderer; // Rendering component

        double shootInterval = 0.3; // Interval between shots
        double lastShootTime; // Time of the last shot
        public bool active; // Activity flag

        public Player(Vector2 startPos, Vector2 direction, float speed, int size, Texture image)
        {
            transform = new TransformComponent(startPos, direction, speed);
            collision = new CollisionComponent(new Vector2(size, size));
            spriteRenderer = new SpriteRendererComponent(image, Raylib.SKYBLUE, transform, collision);
            lastShootTime = -shootInterval;
            active = true;
        }

        /// <summary>
        /// Updates the player state (movement, shooting).
        /// </summary>
        public bool Update(int mapWidth, int mapHeight)
        {
            float deltaTime = Raylib.GetFrameTime();
            Vector2 moveDirection = Vector2.Zero;

            float speed = 200.0f * Raylib.GetFrameTime();

            // Keyboard controls
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT) || Raylib.IsKeyDown(KeyboardKey.KEY_A))
                transform.position.X -= speed;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) || Raylib.IsKeyDown(KeyboardKey.KEY_D))
                transform.position.X += speed;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP) || Raylib.IsKeyDown(KeyboardKey.KEY_W))
                transform.position.Y -= speed; // Двигаемся вверх
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN) || Raylib.IsKeyDown(KeyboardKey.KEY_S))
                transform.position.Y += speed; // Двигаемся вниз

            transform.direction = moveDirection;
            transform.position += transform.direction * transform.speed * deltaTime;

            // Keep the player within screen bounds
            KeepInsideBounds(mapWidth, mapHeight);

            // Check for shooting
            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (Raylib.GetTime() - lastShootTime >= shootInterval)
                {
                    lastShootTime = Raylib.GetTime();
                    return true; // Player shoots
                }
            }

            return false; // Player does not shoot
        }

        void KeepInsideBounds(int mapWidth, int mapHeight)
        {
            transform.position.X = Math.Clamp(transform.position.X, 0, mapWidth - collision.size.X);
            transform.position.Y = Math.Clamp(transform.position.Y, 0, mapHeight - collision.size.Y);
        }

        /// <summary>
        /// Reads input direction from keyboard.
        /// </summary>
        private Vector2 ReadDirectionInput()
        {
            Vector2 moveDirection = Vector2.Zero;

            // Horizontal movement
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) moveDirection.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) moveDirection.X += 1;

            // Vertical movement
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) moveDirection.Y -= 1;  // Move up
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) moveDirection.Y += 1;  // Move down

            return moveDirection;
        }

        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}
