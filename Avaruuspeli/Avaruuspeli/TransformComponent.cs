using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Component for storing an object's position, direction, and speed.
    /// </summary>
    internal class TransformComponent
    {
        public Vector2 position; // Current position of the object
        public Vector2 direction; // Direction of movement
        public float speed; // Movement speed

        public TransformComponent(Vector2 position, Vector2 direction, float speed)
        {
            this.position = position;
            this.direction = direction;
            this.speed = speed;
        }
    }
}
