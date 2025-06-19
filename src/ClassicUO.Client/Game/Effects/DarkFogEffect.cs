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
        private readonly Random _random = new Random();
        private Texture2D _noiseTexture;
        private float _time = 0f;
        private float _offsetX = 0f;
        private float _offsetY = 0f;

        private float Noise(float x, float y)
        {
            return (MathF.Sin(x * 113f + y * 412f) * 6339f) % 1.0f;
        }

        private float SmoothNoise(float x, float y)
        {
            // Simplifica o smooth noise para melhor performance
            float fractX = x - MathF.Floor(x);
            float fractY = y - MathF.Floor(y);

            fractX = fractX * fractX * (3 - 2 * fractX);
            fractY = fractY * fractY * (3 - 2 * fractY);

            int x1 = (int)MathF.Floor(x) & 255;
            int y1 = (int)MathF.Floor(y) & 255;
            int x2 = (x1 + 1) & 255;
            int y2 = (y1 + 1) & 255;

            float topLeft = Noise(x1, y1);
            float topRight = Noise(x2, y1);
            float bottomLeft = Noise(x1, y2);
            float bottomRight = Noise(x2, y2);

            float top = topLeft + fractX * (topRight - topLeft);
            float bottom = bottomLeft + fractX * (bottomRight - bottomLeft);

            return top + fractY * (bottom - top);
        }

        private Texture2D GenerateNoiseTexture(GraphicsDevice device, int width, int height)
        {
            if (_noiseTexture == null || _noiseTexture.IsDisposed)
            {
                // Resolução bem reduzida para performance máxima
                int texWidth = width ;
                int texHeight = height;

                _noiseTexture = new Texture2D(device, texWidth, texHeight);
                var data = new Color[texWidth * texHeight];

                float aspectRatio = (float)width / height;

                // Pre-calcula valores para evitar cálculos repetidos
                float timeOffsetX1 = _offsetX / 60f;
                float timeOffsetX2 = _offsetX / 30f;

                for (int y = 0; y < texHeight; y++)
                {
                    float v = (float)y / texHeight;

                    for (int x = 0; x < texWidth; x++)
                    {
                        float u = (float)x / texWidth * aspectRatio;

                        // Reduzido para apenas 2 camadas para melhor performance
                        float noise = 0f;

                        // Primeira camada - base grande e suave
                        float uv1x = u + timeOffsetX1;
                        noise += SmoothNoise(uv1x * 0.5f, v * 0.5f);

                        // Segunda camada - detalhe sutil
                        float uv2x = u + timeOffsetX2;
                        noise += SmoothNoise(uv2x * 1f, v * 1f) * 0.3f;

                        // Normalização mais suave
                        noise = (noise / 1.3f) + 0.15f;
                        noise = Math.Clamp(noise, 0f, 1f);

                        // Suaviza ainda mais com pow
                        noise = MathF.Pow(noise, 1.5f);

                        // Alpha extremamente sutil
                        float alpha = noise * 0.2f;

                        // Rosa suave e claro
                        data[y * texWidth + x] = new Color(1f, 0.2f, 0.2f, alpha);
                    }
                }

                _noiseTexture.SetData(data);
            }

            return _noiseTexture;
        }

        public void Update(int width, int height)
        {
            _time += Time.Delta;
            _offsetX += Time.Delta * 20f; // Movimento mais lento e suave

            // Atualiza menos frequentemente para melhor performance
            if (_time % 0.12f < Time.Delta)
            {
                _noiseTexture?.Dispose();
                _noiseTexture = null;
            }
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            int width = batcher.GraphicsDevice.Viewport.Width;
            int height = batcher.GraphicsDevice.Viewport.Height;

            Texture2D noiseTex = GenerateNoiseTexture(batcher.GraphicsDevice, width, height);

            // Desenha com escala maior e offset negativo maior para garantir cobertura
            batcher.Draw(
                noiseTex,
                new Vector2(-width * 0.2f, -height * 0.2f), // Offset maior para garantir cobertura
                null,
                Vector3.One,
                0f,
                Vector2.Zero,
                new Vector2(10f, 10f), // Escala maior para garantir cobertura total
                SpriteEffects.None,
                0f
            );

            // Desenha uma segunda vez com offset diferente para mais suavidade
            batcher.Draw(
                noiseTex,
                new Vector2(-width * 0.15f, -height * 0.15f),
                null,
                Vector3.One,
                0f,
                Vector2.Zero,
                new Vector2(9.5f, 9.5f),
                SpriteEffects.None,
                0f
            );
        }
    }
}
