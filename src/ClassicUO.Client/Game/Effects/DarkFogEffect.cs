using System;
using System.Collections.Generic;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Effects
{
    internal sealed class DarkFogEffect
    {
        private readonly List<FogParticle> _particles = new List<FogParticle>();
        private readonly Random _random = new Random();
        private Texture2D _texture;

        private Texture2D GetTexture(GraphicsDevice device)
        {
            if (_texture == null || _texture.IsDisposed)
            {
                const int size = 64;
                _texture = new Texture2D(device, size, size);
                var data = new Color[size * size];
                float radius = size / 2f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - radius;
                        float dy = y - radius;
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        float alpha = Math.Clamp(1f - dist / radius, 0f, 1f);
                        data[y * size + x] = new Color(0f, 0f, 0f, alpha);
                    }
                }

                _texture.SetData(data);
            }

            return _texture;
        }

        public void Update(int width, int height)
        {
            if (_particles.Count < 20)
            {
                SpawnParticle(width, height);
            }

            float dt = Time.Delta;
            for (int i = 0; i < _particles.Count; i++)
            {
                FogParticle p = _particles[i];
                p.Position += p.Velocity * dt;
                p.Alpha -= dt * 0.05f;

                if (p.Alpha <= 0)
                {
                    _particles.RemoveAt(i--);
                }
                else
                {
                    _particles[i] = p;
                }
            }
        }

        private void SpawnParticle(int width, int height)
        {
            float x = _random.Next(width);
            float y = _random.Next(height);
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 10f + (float)_random.NextDouble() * 20f;
            Vector2 vel = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
            float scale = 0.5f + (float)_random.NextDouble();
            float alpha = 0.3f + (float)_random.NextDouble() * 0.2f;

            _particles.Add(new FogParticle { Position = new Vector2(x, y), Velocity = vel, Alpha = alpha, Scale = scale });
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            Texture2D tex = GetTexture(batcher.GraphicsDevice);

            foreach (FogParticle p in _particles)
            {
                Vector3 hue = ShaderHueTranslator.GetHueVector(0, false, p.Alpha);
                int size = (int)(tex.Width * p.Scale);
                var dest = new Rectangle((int)p.Position.X - size / 2, (int)p.Position.Y - size / 2, size, size);
                batcher.Draw(tex, dest, hue);
            }
        }

        private struct FogParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Alpha;
            public float Scale;
        }
    }
}
