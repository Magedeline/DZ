using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaggyHelper
{
    public class ParallaxMountain : Parallax
    {
        private int roomSeed;
        private float baseScale = 1.0f;
        private float noiseScale = 0.1f;
        private int octaves = 3;
        private float persistence = 0.5f;
        private Dictionary<(float, float, int), float> noiseCache = new();

        public ParallaxMountain(int seed) : base(null)
        {
            this.roomSeed = seed;
            Visible = true;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
        }

        public override void Render(Scene scene)
        {
            if (!Visible)
                return;

            if (!(scene is Level level))
                return;

            Vector2 cameraPosition = level.Camera.Position;
            RenderProceduralMountain(level, cameraPosition);
        }

        private void RenderProceduralMountain(Level level, Vector2 cameraPosition)
        {
            var spriteBatch = Draw.SpriteBatch;

            // Mountain dimensions
            float mountainX = cameraPosition.X - 50;
            float mountainY = cameraPosition.Y + 160;
            float mountainWidth = 400;
            float mountainHeight = 100;

            // Draw multiple horizontal bands, each with noise-based depth variation
            for (int band = 0; band < 5; band++)
            {
                DrawMountainBand(spriteBatch, mountainX, mountainY - (band * 20), mountainWidth, 20, band);
            }
        }

        private void DrawMountainBand(SpriteBatch batch, float x, float y, float width, float height, int bandIndex)
        {
            // Sample noise across the band
            float bandNoise = GetOctaveNoise(y * noiseScale, bandIndex * 0.5f, octaves, persistence);

            // Adjust depth/color based on noise
            float depth = MathHelper.Lerp(0.7f, 1.3f, (bandNoise + 1) / 2);
            Color bandColor = new Color(
                MathHelper.Clamp(0.4f * depth, 0, 1),
                MathHelper.Clamp(0.3f * depth, 0, 1),
                MathHelper.Clamp(0.2f * depth, 0, 1),
                0.6f
            );

            // Draw simple rectangle band
            try
            {
                batch.Draw(
                    Draw.Pixel.Texture.Texture,
                    new Rectangle((int)x, (int)y, (int)width, (int)height),
                    bandColor
                );
            }
            catch
            {
                // Fallback if texture unavailable
                return;
            }

            // Draw variation: small "peaks" based on noise
            for (int i = 0; i < 8; i++)
            {
                float sampleX = x + (i / 8f) * width;
                float sampleY = y + height / 2;
                float peakNoise = GetCachedNoise(sampleX * 0.01f, sampleY * 0.01f, roomSeed);
                float peakHeight = MathHelper.Lerp(0, height, (peakNoise + 1) / 2);

                batch.Draw(
                    Draw.Pixel.Texture.Texture,
                    new Rectangle((int)sampleX, (int)(sampleY - peakHeight), 50, (int)peakHeight),
                    new Color(bandColor.R, bandColor.G, bandColor.B, 0.4f)
                );
            }
        }

        private float GetOctaveNoise(float x, float y, int octaveCount, float persistenceValue)
        {
            float result = 0;
            float amplitude = 1;
            float frequency = 1;
            float maxValue = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                result += GetCachedNoise(
                    x * frequency * noiseScale,
                    y * frequency * noiseScale,
                    roomSeed + i
                ) * amplitude;

                maxValue += amplitude;
                amplitude *= persistenceValue;
                frequency *= 2;
            }

            return result / maxValue;
        }

        private float GetCachedNoise(float x, float y, int seed)
        {
            var key = (x, y, seed);
            if (noiseCache.TryGetValue(key, out float cached))
                return cached;

            float result = SimplexNoise.Noise(x, y, seed);
            noiseCache[key] = result;
            return result;
        }
    }
}
