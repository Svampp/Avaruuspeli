using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Bullet class, manages movement and rendering.
    /// </summary>
    internal class Bullet
    {
        public bool active; // Bullet activity flag
        public TransformComponent transform; // Position and movement
        public CollisionComponent collision; // Size for collision detection
        SpriteRendererComponent spriteRenderer; // Rendering component

        public Bullet(Vector2 startPosition, Vector2 direction, float speed, int size, Texture image, Color color)
        {
            transform = new TransformComponent(startPosition, direction, speed);
            collision = new CollisionComponent(new Vector2(size, size));
            spriteRenderer = new SpriteRendererComponent(image, color, transform, collision);
            active = true;
        }

        /// <summary>
        /// Resets the bullet parameters for reuse.
        /// </summary>
        public void Reset(Vector2 startPosition, Vector2 direction, float speed, int size)
        {
            transform.position = startPosition;
            transform.direction = direction;
            transform.speed = speed;
            collision.size = new Vector2(size, size);
            active = true;
        }

        public void Update()
        {
            transform.position += transform.direction * transform.speed * Raylib.GetFrameTime();
        }

        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}
