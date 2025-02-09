using System.Numerics;

namespace Avaruuspeli
{
    /// <summary>
    /// Component for handling object collisions.
    /// Stores the size of an object to determine collision boundaries.
    /// </summary>
    internal class CollisionComponent
    {
        public Vector2 size; // Width and height of the object for collision detection

        /// <summary>
        /// Constructor to initialize the collision size.
        /// </summary>
        /// <param name="size">Size of the object (width, height)</param>
        public CollisionComponent(Vector2 size)
        {
            this.size = size; // Assign size to the object
        }
    }
}
