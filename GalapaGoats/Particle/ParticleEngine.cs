using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GalapaGoats.Sprites;

namespace GalapaGoats
{
    public class ParticleEngine
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private List<Sprite> sprites;
        private bool generate = true;
        public bool is_palloncino{get{return _is_palloncino;}
            set
            {
                if (value != _is_palloncino)
                {
                    if (fart_sound == null)
                    {
                        fart_sound = AudioPlayer.Play("Fart", true);
                        fart_sound.Pause();
                    }
                    if (!_is_palloncino)
                        fart_sound.Resume();
                    else if (fart_sound != null)
                        fart_sound.Pause();
                    _is_palloncino = value;
                }
            }
        }

        public ParticleEngine(List<Sprite> sprites, Vector2 location)
        {
            EmitterLocation = location;
            this.sprites = sprites;
            this.particles = new List<Particle>();
            random = new Random();
        }

        public void dispose()
        {
            if (fart_sound != null)
                if (fart_sound.State == Microsoft.Xna.Framework.Audio.SoundState.Playing)
                    fart_sound.Stop();
        }

        public void Update()
        {
            int total = 5;

            if (is_palloncino)
            {
                if (generate)
                {
                    for (int i = 0; i < total; i++)
                    {
                        particles.Add(GenerateNewParticle());
                    }
                    generate = !generate;
                }
                else
                    generate = !generate;
            }
 
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }
 
        private Particle GenerateNewParticle()
        {
            int particleType = random.Next(sprites.Count);
            Vector2 position = EmitterLocation;

            var baseSpeed = Vector2.UnitY * (2f * (float)(random.NextDouble())+2);
            var velocity = Vector2.Transform(baseSpeed, Matrix.CreateRotationZ(MathHelper.ToRadians(-20 + random.Next(41))));
            //Vector2 velocity = new Vector2(
            //                        1f * (float)(random.NextDouble() * 2 - 1),
            //                        2f * (float)(random.NextDouble() * 2 - 1));

            float angle = 0f;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            //Color color = new Color(
            //            (float)random.NextDouble(),
            //            (float)random.NextDouble(),
            //            (float)random.NextDouble());
            Color color = Color.White;
            float size = 0.1f;//(float)random.NextDouble();
            int ttl = 5+random.Next(15);

            return new Particle(particleType, position, velocity, angle, angularVelocity, color, size, ttl);
        }

        Vector2 origin = new Vector2(24 / 2, 24 / 2);
        private bool _is_palloncino;
        private Microsoft.Xna.Framework.Audio.SoundEffectInstance fart_sound;
        public void Draw(SpriteBatch batch)
        {
            foreach(var particle in particles)
            {
                batch.Draw(sprites[particle.ParticleType].texture, particle.Position, sprites[particle.ParticleType].Source_Rect, particle.Color, particle.Angle, origin, particle.Size, SpriteEffects.None, 0f);
            }
        }        
    }
}