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

        // Конструктор для создания врага в заданной позиции
        public Enemy(Vector2 position, Vector2 direction, float speed, int size, Texture image, int score)
        {
            transform = new TransformComponent(position, direction, speed);
            collision = new CollisionComponent(new Vector2(size, size));
            spriteRenderer = new SpriteRendererComponent(image, Raylib.RED, transform, collision);
            active = true;
            scoreValue = score;
        }

        // Обновление врага, движение по заданному направлению
        public void Update()
        {
            if (active)
            {
                transform.position += transform.direction * transform.speed * Raylib.GetFrameTime();

                // Проверяем, если враг вышел за пределы экрана
                if (transform.position.Y > Raylib.GetScreenHeight() || transform.position.Y < 0 || transform.position.X > Raylib.GetScreenWidth() || transform.position.X < 0)
                {
                    active = false; // деактивируем врага, если он выходит за экран
                }
            }
        }

        public void Draw()
        {
            if (active) spriteRenderer.Draw();
        }
    }
}