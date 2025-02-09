using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Avaruuspeli
{
    // Explosion class to handle explosion logic in the game
    class Explosion
    {
        public Vector2 position; // Position where the explosion occurs
        public List<Vector2> particles; // List of particles representing explosion
        public List<Vector2> velocities; // Velocities of the explosion particles
        public float lifeTime; // The remaining life time of the explosion
        public float maxLifeTime = 0.5f; // Maximum lifetime of the explosion (in seconds)

        // Constructor initializes the explosion with a given position
        public Explosion(Vector2 position)
        {
            this.position = position;
            particles = new List<Vector2>(); // List of particles initialized
            velocities = new List<Vector2>(); // List of velocities initialized

            Random random = new Random(); // Random number generator to randomize particle movement
            for (int i = 0; i < 20; i++) // Create 20 particles for the explosion
            {
                particles.Add(position); // Add the initial position of the explosion for each particle
                float angle = (float)(random.NextDouble() * 2 * Math.PI); // Random angle for particle movement
                float speed = (float)(random.NextDouble() * 100 + 50); // Random speed for each particle
                velocities.Add(new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed)); // Calculate velocity based on angle and speed
            }
            lifeTime = maxLifeTime; // Set the initial lifetime to max lifetime
        }

        // Update method is called each frame to update the particles' position
        public void Update(float deltaTime)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i] += velocities[i] * deltaTime; // Update the particle position based on its velocity and delta time
            }
            lifeTime -= deltaTime; // Decrease the lifetime of the explosion over time
        }

        // Draw method renders the explosion particles on the screen
        public void Draw()
        {
            foreach (var particle in particles)
            {
                Raylib.DrawCircleV(particle, 3, Raylib.ORANGE); // Draw each particle as an orange circle
            }
        }

        // Method to check if the explosion has finished (lifetime <= 0)
        public bool IsFinished()
        {
            return lifeTime <= 0; // Return true if the explosion's lifetime has finished
        }
    }
}
