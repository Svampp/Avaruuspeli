using Raylib_CsLo;
using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Component for rendering sprites and scaling them.
    /// </summary>
    internal class SpriteRendererComponent
    {
        Texture sprite; // Sprite texture
        Color debugColor; // Debug color (border)
        TransformComponent transform; // Reference to the object's position
        CollisionComponent collision; // Reference to the object's size

        float scale; // Sprite scaling factor
        Vector2 drawOffset; // Offset for centering the sprite

        public SpriteRendererComponent(Texture image, Color color, TransformComponent transform, CollisionComponent collision)
        {
            sprite = image;
            debugColor = color;
            this.transform = transform;
            this.collision = collision;

            // Calculate scale so the sprite fits within the collision bounds
            scale = Math.Min(collision.size.X / sprite.width, collision.size.Y / sprite.height);
            drawOffset = (collision.size - new Vector2(sprite.width, sprite.height) * scale) / 2;
        }

        public void Draw()
        {
            // Draw the texture with scaling and offset applied
            Raylib.DrawTextureEx(sprite, transform.position + drawOffset, 0.0f, scale, Raylib.WHITE);
        }
    }
}
