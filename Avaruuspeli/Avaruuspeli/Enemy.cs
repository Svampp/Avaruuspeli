using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Enemy class, handles movement and rendering.
    /// </summary>
    internal class Enemy
    {
        public TransformComponent transform; // Stores position, direction, and speed
        public CollisionComponent collision; // Defines enemy's collision size
        SpriteRendererComponent spriteRenderer; // Handles rendering

        public bool active; // Indicates if the enemy is active
        public int scoreValue; // Score points awarded when the enemy is destroyed

        /// <summary>
        /// Constructor to initialize an enemy at a specific position.
        /// </summary>
        /// <param name="position">Starting position of the enemy</param>
        /// <param name="direction">Movement direction</param>
        /// <param name="speed">Speed of movement</param>
        /// <param name="size">Size of the enemy for collision detection</param>
        /// <param name="image">Texture used for rendering</param>
        /// <param name="score">Score value assigned to this enemy</param>
        public Enemy(Vector2 position, Vector2 direction, float speed, int size, Texture image, int score)
        {
            transform = new TransformComponent(position, direction, speed); // Initialize position and movement
            collision = new CollisionComponent(new Vector2(size, size)); // Set collision size
            spriteRenderer = new SpriteRendererComponent(image, Raylib.RED, transform, collision); // Set rendering
            active = true; // Enemy starts as active
            scoreValue = score; // Assign score value
        }

        /// <summary>
        /// Updates enemy movement and deactivates it if out of bounds.
        /// </summary>
        public void Update()
        {
            if (active)
            {
                transform.position += transform.direction * transform.speed * Raylib.GetFrameTime(); // Move enemy

                // Check if the enemy goes out of screen bounds
                if (transform.position.Y > Raylib.GetScreenHeight() || transform.position.Y < 0 ||
                    transform.position.X > Raylib.GetScreenWidth() || transform.position.X < 0)
                {
                    active = false; // Deactivate enemy if it moves off-screen
                }
            }
        }

        /// <summary>
        /// Draws the enemy if it is active.
        /// </summary>
        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}
