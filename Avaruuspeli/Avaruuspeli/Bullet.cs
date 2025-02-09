using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Bullet class, manages movement and rendering.
    /// </summary>
    internal class Bullet
    {
        public bool active; // Indicates whether the bullet is active
        public TransformComponent transform; // Stores position, movement direction, and speed
        public CollisionComponent collision; // Defines the bullet's collision area
        SpriteRendererComponent spriteRenderer; // Handles bullet rendering

        /// <summary>
        /// Constructor that initializes bullet properties.
        /// </summary>
        public Bullet(Vector2 startPosition, Vector2 direction, float speed, int size, Texture image, Color color)
        {
            transform = new TransformComponent(startPosition, direction, speed); // Set initial position and movement
            collision = new CollisionComponent(new Vector2(size, size)); // Define the bullet's size
            spriteRenderer = new SpriteRendererComponent(image, color, transform, collision); // Initialize rendering
            active = true; // Bullet is active when created
        }

        /// <summary>
        /// Resets the bullet parameters for reuse.
        /// </summary>
        public void Reset(Vector2 startPosition, Vector2 direction, float speed, int size)
        {
            transform.position = startPosition; // Set new starting position
            transform.direction = direction; // Update movement direction
            transform.speed = speed; // Update speed
            collision.size = new Vector2(size, size); // Adjust collision size
            active = true; // Reactivate the bullet
        }

        /// <summary>
        /// Updates bullet position based on speed and direction.
        /// </summary>
        public void Update()
        {
            transform.position += transform.direction * transform.speed * Raylib.GetFrameTime(); // Move bullet
        }

        /// <summary>
        /// Draws the bullet on the screen if it's active.
        /// </summary>
        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}
