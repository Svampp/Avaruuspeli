using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Component for handling object collisions.
    /// </summary>
    internal class CollisionComponent
    {
        public Vector2 size; // Object dimensions for collision calculations

        public CollisionComponent(Vector2 size)
        {
            this.size = size;
        }
    }
}
