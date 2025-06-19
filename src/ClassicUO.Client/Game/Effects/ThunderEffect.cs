using System;
using System.Collections.Generic;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Effects
{
    internal sealed class ThunderEffect
    {
        private readonly List<ThunderBolt> _bolts = new List<ThunderBolt>();
        private readonly Random _random = new Random();

        public void Update(int width, int height)
        {
            // Remove bolts that have faded out
            for (int i = 0; i < _bolts.Count; i++)
            {
                ThunderBolt bolt = _bolts[i];
                bolt.Life -= Time.Delta;
                if (bolt.Life <= 0)
                {
                    _bolts.RemoveAt(i--);
                }
                else
                {
                    bolt.Alpha = MathF.Sin(bolt.Life * 20f) * 0.5f + 0.5f; // Efeito de piscar
                    _bolts[i] = bolt;
                }
            }

            // Randomly spawn new bolts
            if (_bolts.Count < 3 && _random.NextDouble() < 0.05)
            {
                _bolts.Add(ThunderBolt.CreateRandom(width, height, _random));
            }
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            foreach (var bolt in _bolts)
            {
                // Desenha o glow externo (mais suave)
                for (int glow = 3; glow >= 0; glow--)
                {
                    float glowAlpha = bolt.Alpha * (0.2f - (glow * 0.05f));
                    Vector3 glowHue = new Vector3(0.5f, 0.7f, 1f) * glowAlpha;
                    DrawBoltSegments(batcher, bolt, glowHue, (glow + 1) * 2);
                }

                // Desenha o raio principal
                Vector3 mainHue = new Vector3(0.7f, 0.9f, 1f) * bolt.Alpha;
                DrawBoltSegments(batcher, bolt, mainHue, 1);
            }
        }

        private void DrawBoltSegments(UltimaBatcher2D batcher, ThunderBolt bolt, Vector3 hue, float thickness)
        {
            for (int i = 0; i < bolt.Points.Count - 1; i++)
            {
                Vector2 start = bolt.Points[i];
                Vector2 end = bolt.Points[i + 1];

                // Desenha uma linha para cada segmento do raio
                batcher.DrawLine(
                    SolidColorTextureCache.GetTexture(Color.White),
                    start,
                    end,
                    hue,
                    thickness
                );
            }
        }

        private struct ThunderBolt
        {
            public List<Vector2> Points;
            public float Life;
            public float Alpha;

            public static ThunderBolt CreateRandom(int width, int height, Random random)
            {
                // Define o ponto inicial em uma posição aleatória na tela
                Vector2 start = new Vector2(
                    random.Next(width),
                    random.Next(height) // Aparece em toda a altura da tela
                );

                // Cria uma lista de pontos para formar o raio
                List<Vector2> points = new List<Vector2>();
                points.Add(start);

                // Número de segmentos do raio
                int segments = random.Next(2, 4);
                Vector2 currentPos = start;

                // Gera pontos em zig-zag para formar o raio
                for (int i = 0; i < segments; i++)
                {
                    float angle = (float)(random.NextDouble() * MathF.PI / 4) + (MathF.PI / 2);
                    float length = random.Next(5, 20);

                    currentPos += new Vector2(
                        MathF.Cos(angle) * length,
                        MathF.Sin(angle) * length
                    );

                    // Adiciona pontos intermediários para mais detalhes
                    int subSegments = random.Next(1, 3);
                    Vector2 lastPos = points[points.Count - 1];
                    for (int j = 1; j <= subSegments; j++)
                    {
                        float t = j / (float)(subSegments + 1);
                        Vector2 subPos = Vector2.Lerp(lastPos, currentPos, t);
                        // Adiciona pequenas variações para parecer mais natural
                        subPos += new Vector2(
                            random.Next(-5, 6),
                            random.Next(-5, 6)
                        );
                        points.Add(subPos);
                    }

                    points.Add(currentPos);
                }

                return new ThunderBolt
                {
                    Points = points,
                    Life = 0.1f + (float)random.NextDouble() * 0.1f, // Duração do raio
                    Alpha = 1f
                };
            }
        }
    }
}
