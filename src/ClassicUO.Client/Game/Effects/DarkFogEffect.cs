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
                const int width = 128;
                const int height = 32;

                _texture = new Texture2D(device, width, height);
                var data = new Color[width * height];

                float rx = width / 2f;
                float ry = height / 2f;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx = (x - rx) / rx;
                        float dy = (y - ry) / ry;
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        float alpha = Math.Clamp(1f - dist, 0f, 1f);

                        data[y * width + x] = new Color(0.35f, 0.3f, 0.45f, alpha);
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
            int side = _random.Next(4);
            Vector2 pos;
            Vector2 vel;

            const float minSpeed = 30f;
            const float maxSpeed = 60f;

            switch (side)
            {
                case 0: // left to right
                    pos = new Vector2(-40, _random.Next(height));
                    vel = new Vector2(minSpeed + (float)_random.NextDouble() * (maxSpeed - minSpeed), 0f);
                    break;
                case 1: // right to left
                    pos = new Vector2(width + 40, _random.Next(height));
                    vel = new Vector2(-minSpeed - (float)_random.NextDouble() * (maxSpeed - minSpeed), 0f);
                    break;
                case 2: // top to bottom
                    pos = new Vector2(_random.Next(width), -40);
                    vel = new Vector2(0f, minSpeed + (float)_random.NextDouble() * (maxSpeed - minSpeed));
                    break;
                default: // bottom to top
                    pos = new Vector2(_random.Next(width), height + 40);
                    vel = new Vector2(0f, -minSpeed - (float)_random.NextDouble() * (maxSpeed - minSpeed));
                    break;
            }

            float scale = 0.8f + (float)_random.NextDouble() * 0.6f;
            float alpha = 0.5f + (float)_random.NextDouble() * 0.3f;
            float rotation = MathF.Atan2(vel.Y, vel.X);

            _particles.Add(new FogParticle
            {
                Position = pos,
                Velocity = vel,
                Alpha = alpha,
                Scale = scale,
                Rotation = rotation
            });
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            Texture2D tex = GetTexture(batcher.GraphicsDevice);

            foreach (FogParticle p in _particles)
            {
                Vector3 hue = ShaderHueTranslator.GetHueVector(0, false, p.Alpha);
                Vector2 scale = new Vector2(p.Scale);
                batcher.Draw(
                    tex,
                    p.Position,
                    null,
                    hue,
                    p.Rotation,
                    new Vector2(tex.Width / 2f, tex.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private struct FogParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Alpha;
            public float Scale;
            public float Rotation;
        }
    }
}
