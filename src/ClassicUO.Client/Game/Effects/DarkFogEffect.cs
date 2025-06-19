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
            float n = MathF.Sin(x * 113f + y * 412f) * 6339f;
            return n - MathF.Floor(n);
        }

        private float SmoothNoise(float x, float y)
        {
            float ix = MathF.Floor(x);
            float iy = MathF.Floor(y);
            float fx = x - ix;
            float fy = y - iy;

            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);

            float topLeft = Noise(ix, iy);
            float topRight = Noise(ix + 1f, iy);
            float bottomLeft = Noise(ix, iy + 1f);
            float bottomRight = Noise(ix + 1f, iy + 1f);

            float top = MathHelper.Lerp(topLeft, topRight, fx);
            float bottom = MathHelper.Lerp(bottomLeft, bottomRight, fx);

            return MathHelper.Lerp(top, bottom, fy);
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
                float time40 = _offsetX / 40f;
                float time10 = _offsetX / 10f;
                float time30 = _offsetX / 30f;

                for (int y = 0; y < texHeight; y++)
                {
                    float v = (float)y / texHeight;

                    for (int x = 0; x < texWidth; x++)
                    {
                        float u = (float)x / texWidth * aspectRatio;

                        float uvx = u + time40;
                        float uv2x = u + time10;
                        float uv3x = u + time30;

                        float col = SmoothNoise(uvx * 4f, v * 4f);
                        col += SmoothNoise(uvx * 8f, v * 8f) * 0.5f;
                        col += SmoothNoise(uv2x * 16f, v * 16f) * 0.25f;
                        col += SmoothNoise(uv3x * 32f, v * 32f) * 0.125f;
                        col += SmoothNoise(uv3x * 64f, v * 64f) * 0.0625f;

                        col /= 2f;
                        col *= MathHelper.Clamp((col - 0.2f) / (0.4f - 0.2f), 0f, 1f);

                        float alpha = col * 0.2f;

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
