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

            float top = Microsoft.Xna.Framework.MathHelper.Lerp(topLeft, topRight, fx);
            float bottom = Microsoft.Xna.Framework.MathHelper.Lerp(bottomLeft, bottomRight, fx);

            return Microsoft.Xna.Framework.MathHelper.Lerp(top, bottom, fy);
        }

        private Texture2D GenerateNoiseTexture(GraphicsDevice device, int width, int height)
        {
            if (_noiseTexture == null || _noiseTexture.IsDisposed)
            {
                // Reduz resolução para melhor performance
                int texWidth = width / 4;  // Reduz resolução para 1/4
                int texHeight = height / 4;

                _noiseTexture = new Texture2D(device, texWidth, texHeight);
                var data = new Color[texWidth * texHeight];

                float aspectRatio = (float)width / height;

                // Pre-calcula valores para evitar cálculos repetidos
                float time40 = _offsetX / 40f;
                float time20 = _offsetX / 20f;
                float time10 = _offsetX / 10f;

                // Pre-calcula escalas de noise
                float scale1 = 2f;
                float scale2 = 4f;
                float scale3 = 8f;

                for (int y = 0; y < texHeight; y++)
                {
                    float v = (float)y / texHeight;
                    float v1 = v * scale1;
                    float v2 = v * scale2;
                    float v3 = v * scale3;

                    for (int x = 0; x < texWidth; x++)
                    {
                        float u = (float)x / texWidth * aspectRatio;

                        float uvx = u + time40;
                        float uv2x = u + time20;
                        float uv3x = u + time10;

                        // Reduz para 3 camadas de noise
                        float col = SmoothNoise(uvx * scale1, v1);
                        col += SmoothNoise(uv2x * scale2, v2) * 0.5f;
                        col += SmoothNoise(uv3x * scale3, v3) * 0.25f;

                        col /= 1.75f;

                        // Simplifica o smoothstep
                        col = Microsoft.Xna.Framework.MathHelper.Clamp((col - 0.2f) / 0.2f, 0f, 1f);

                        float alpha = col * 0.15f; // Alpha mais sutil

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
            _offsetX += Time.Delta * 30f; // Velocidade ajustada

            // Atualiza menos frequentemente
            if (_time % 0.15f < Time.Delta)
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

            // Desenha com escala maior para compensar a resolução menor
            batcher.Draw(
                noiseTex,
                new Vector2(-width * 0.1f, -height * 0.1f),
                null,
                Vector3.One,
                0f,
                Vector2.Zero,
                new Vector2(4.2f, 4.2f), // Escala ajustada para a nova resolução
                SpriteEffects.None,
                0f
            );
        }
    }
}
