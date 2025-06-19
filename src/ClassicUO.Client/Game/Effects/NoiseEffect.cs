using System;
using System.Collections.Generic;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Effects
{
    internal sealed class NoiseEffect
    {
        private readonly List<NoiseParticle> _particles = new List<NoiseParticle>();
        private readonly Random _random = new Random();
        private float _spawnTimer = 0f;

        public void Update(int width, int height)
        {
            // Remove particles that have faded out
            for (int i = 0; i < _particles.Count; i++)
            {
                NoiseParticle particle = _particles[i];
                particle.Life -= Time.Delta;
                if (particle.Life <= 0)
                {
                    _particles.RemoveAt(i--);
                }
                else
                {
                    // Update particle position
                    particle.Position += particle.Velocity * Time.Delta;
                    particle.Alpha = particle.Life / particle.MaxLife;
                    _particles[i] = particle;
                }
            }

            // Spawn new particles
            _spawnTimer += Time.Delta;
            if (_spawnTimer >= 0.01f) // Spawn every 10ms
            {
                _spawnTimer = 0f;

                // Spawn 1-3 particles at a time
                int count = _random.Next(1, 4);
                for (int i = 0; i < count && _particles.Count < 50; i++)
                {
                    _particles.Add(CreateRandomParticle(width, height));
                }
            }
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            foreach (var particle in _particles)
            {
                // Draw noise particle as a small colored dot
                batcher.Draw(
                    SolidColorTextureCache.GetTexture(Color.White),
                    particle.Position,
                    new Vector3(particle.Color.X * particle.Alpha, particle.Color.Y * particle.Alpha, particle.Color.Z * particle.Alpha)
                );
            }
        }

        private NoiseParticle CreateRandomParticle(int width, int height)
        {
            // Random position on screen
            Vector2 position = new Vector2(
                _random.Next(width),
                _random.Next(height)
            );

            // Random velocity (slow movement)
            Vector2 velocity = new Vector2(
                (_random.NextSingle() - 0.5f) * 20f,
                (_random.NextSingle() - 0.5f) * 20f
            );

            // Random color (grayscale with slight tint)
            Vector3 color = new Vector3(
                0.8f + _random.NextSingle() * 0.2f,
                0.8f + _random.NextSingle() * 0.2f,
                0.8f + _random.NextSingle() * 0.2f
            );

            // Random size
            int size = _random.Next(1, 4);

            // Random life duration
            float life = 0.5f + _random.NextSingle() * 1.0f;

            return new NoiseParticle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Size = size,
                Life = life,
                MaxLife = life,
                Alpha = 1f
            };
        }

        private struct NoiseParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector3 Color;
            public int Size;
            public float Life;
            public float MaxLife;
            public float Alpha;
        }
    }
}