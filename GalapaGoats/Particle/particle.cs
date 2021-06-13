using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GalapaGoats.Sprites;

namespace GalapaGoats
{
    public class Particle
    {
        public int ParticleType;
        public Vector2 Position;         // The current position of the particle        
        public Vector2 Velocity;         // The speed of the particle at the current instance
        public float Angle;              // The current angle of rotation of the particle
        public float AngularVelocity;    // The speed that the angle is changing
        public Color Color;              // The color of the particle
        public float Size;               // The size of the particle
        public int TTL;                  // The 'time to live' of the particle

        public Particle(int particleType, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, int ttl)
        {
            ParticleType = particleType;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            TTL = ttl;
        }
 
        public void Update()
        {
            TTL--;
            Position += Velocity;
            Angle += AngularVelocity;
            Size += 0.13f;
            this.Color = new Color(Color.R - 15, Color.G - 15, Color.B - 15, Color.A - 15);
        } 
    }
}