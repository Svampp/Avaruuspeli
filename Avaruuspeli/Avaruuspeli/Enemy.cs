using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Enemy class, moves and shoots.
    /// </summary>
    internal class Enemy
    {
        public TransformComponent transform; // Position and movement
        public CollisionComponent collision; // Size for collision detection
        SpriteRendererComponent spriteRenderer; // Rendering component

        public bool active; // Activity flag
        public int scoreValue; // Points awarded for destroying the enemy

        public Enemy(Vector2 startPosition, Vector2 direction, float speed, int size, Texture image, int score)
        {
            transform = new TransformComponent(startPosition, direction, speed);
            collision = new CollisionComponent(new Vector2(size, size));
            spriteRenderer = new SpriteRendererComponent(image, Raylib.RED, transform, collision);
            active = true;
            scoreValue = score;
        }

        public void Update()
        {
            if (active)
            {
                transform.position += transform.direction * transform.speed * Raylib.GetFrameTime();
            }
        }

        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}
