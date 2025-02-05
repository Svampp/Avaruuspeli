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


        // Конструктор изменен для случайной позиции по X и фиксированного движения вниз
        public Enemy(Vector2 position, Vector2 vector2, float speed, int size, Texture image, int score)
        {
            // Случайная позиция по оси X, от 0 до ширины окна
            float randomX = Raylib.GetRandomValue(0, Raylib.GetScreenWidth());
            transform = new TransformComponent(new Vector2(randomX, 0), new Vector2(0, 1), speed); // движение вниз по Y
            collision = new CollisionComponent(new Vector2(size, size));
            spriteRenderer = new SpriteRendererComponent(image, Raylib.RED, transform, collision);
            active = true;
            scoreValue = score;
        }

        // Обновление врага, движение вниз
        public void Update()
        {
            if (active)
            {
                transform.position += transform.direction * transform.speed * Raylib.GetFrameTime();

                // Проверяем, если враг вышел за пределы экрана
                if (transform.position.Y > Raylib.GetScreenHeight())
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
