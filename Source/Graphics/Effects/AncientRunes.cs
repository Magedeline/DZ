namespace Celeste.Effects
{
    /// <summary>
    /// Ancient Runes effect that displays glowing runes with animation
    /// </summary>
    [CustomBackdrop("DZ/AncientRunes")]
    public class AncientRunes : Backdrop
    {
        private struct Rune
        {
            public Vector2 Position;
            public string Symbol;
            public float Alpha;
            public float Scale;
            public float Rotation;
            public float GlowIntensity;
            public float AnimationTimer;
            public float FadeDistance;
            public Color Color;
        }

        private List<Rune> runes;
        private string runeType;
        private float glowIntensity;
        private float animationSpeed;
        private float fadeDistance;
        private string[] runeSymbols;
        private Color runeColor;
        private Random random;
        private float globalTimer;

        public AncientRunes(string runeType = "ancient", float glowIntensity = 1.0f, float animationSpeed = 1.0f, float fadeDistance = 200f)
        {
            this.runeType = runeType;
            this.glowIntensity = Math.Max(0.1f, Math.Min(3.0f, glowIntensity));
            this.animationSpeed = Math.Max(0.1f, Math.Min(5.0f, animationSpeed));
            this.fadeDistance = Math.Max(50f, Math.Min(500f, fadeDistance));

            random = new Random();
            runes = new List<Rune>();
            globalTimer = 0f;

            setupRuneType();
            initializeRunes();
        }

        private void setupRuneType()
        {
            switch (runeType.ToLower())
            {
                case "ancient":
                    runeSymbols = new string[] { "◊", "◈", "◇", "⬟", "⬢", "⬡", "⟐", "⟡" };
                    runeColor = Color.Gold;
                    break;
                case "mystical":
                    runeSymbols = new string[] { "✦", "✧", "✩", "✪", "✫", "✬", "✭", "✮" };
                    runeColor = Color.Purple;
                    break;
                case "elemental":
                    runeSymbols = new string[] { "▲", "▼", "◆", "●", "■", "♦", "♠", "♣" };
                    runeColor = Color.CornflowerBlue;
                    break;
                case "celestial":
                    runeSymbols = new string[] { "☆", "★", "☉", "☽", "☾", "♈", "♉", "♊" };
                    runeColor = Color.Silver;
                    break;
                case "demonic":
                    runeSymbols = new string[] { "⧨", "⧩", "⧪", "⧫", "⟢", "⟣", "⟤", "⟥" };
                    runeColor = Color.Crimson;
                    break;
                default:
                    runeSymbols = new string[] { "◊", "◈", "◇", "⬟", "⬢", "⬡", "⟐", "⟡" };
                    runeColor = Color.Gold;
                    break;
            }
        }

        private void initializeRunes()
        {
            int runeCount = random.Next(8, 16);

            for (int i = 0; i < runeCount; i++)
            {
                createRune();
            }
        }

        private void createRune()
        {
            Rune rune = new Rune
            {
                Position = new Vector2(
                    (float)(random.NextDouble() * 1600 - 800),
                    (float)(random.NextDouble() * 1000 - 500)
                ),
                Symbol = runeSymbols[random.Next(runeSymbols.Length)],
                Alpha = (float)(random.NextDouble() * 0.6 + 0.4),
                Scale = (float)(random.NextDouble() * 0.8 + 0.6),
                Rotation = (float)(random.NextDouble() * Math.PI * 2),
                GlowIntensity = (float)(random.NextDouble() * 0.5 + 0.5) * glowIntensity,
                AnimationTimer = (float)(random.NextDouble() * Math.PI * 2),
                FadeDistance = fadeDistance,
                Color = runeColor
            };

            runes.Add(rune);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            globalTimer += Engine.DeltaTime * animationSpeed;

            for (int i = 0; i < runes.Count; i++)
            {
                var rune = runes[i];

                // Update animation timer
                rune.AnimationTimer += Engine.DeltaTime * animationSpeed;

                // Pulsing glow effect
                rune.GlowIntensity = (float)(Math.Sin(rune.AnimationTimer) * 0.3 + 0.7) * glowIntensity;

                // Slow rotation
                rune.Rotation += Engine.DeltaTime * 0.5f * animationSpeed;

                // Floating effect
                rune.Position.Y += (float)Math.Sin(globalTimer + rune.AnimationTimer) * 0.2f;

                runes[i] = rune;
            }
        }

        public override void Render(Scene scene)
        {
            var camera = (scene as Level)?.Camera ?? new Camera();
            var player = (scene as Level)?.Tracker?.GetEntity<global::Celeste.Player>();

            foreach (var rune in runes)
            {
                Vector2 screenPos = rune.Position - camera.Position;

                // Calculate distance to player for fading
                float distanceToPlayer = 1f;
                if (player != null)
                {
                    float distance = Vector2.Distance(rune.Position, player.Position);
                    distanceToPlayer = Math.Max(0.1f, Math.Min(1f, distance / rune.FadeDistance));
                }

                // Only render runes visible on screen
                if (screenPos.X > -100 && screenPos.X < 420 && screenPos.Y > -100 && screenPos.Y < 340)
                {
                    float finalAlpha = rune.Alpha * rune.GlowIntensity * distanceToPlayer;
                    Color glowColor = rune.Color * (finalAlpha * 0.3f);
                    Color mainColor = rune.Color * finalAlpha;

                    // Render glow effect (larger, transparent)
                    if (finalAlpha > 0.2f)
                    {
                        ActiveFont.DrawOutline(
                            rune.Symbol,
                            screenPos,
                            new Vector2(0.5f, 0.5f),
                            Vector2.One * rune.Scale * 2f,
                            glowColor,
                            2f,
                            Color.Transparent
                        );
                    }

                    // Render main rune
                    ActiveFont.DrawOutline(
                        rune.Symbol,
                        screenPos,
                        new Vector2(0.5f, 0.5f),
                        Vector2.One * rune.Scale,
                        mainColor,
                        1f,
                        Color.Black * (finalAlpha * 0.5f)
                    );

                    // Add extra sparkle for high glow
                    if (rune.GlowIntensity > 1.5f)
                    {
                        float sparkleSize = rune.Scale * 8f;
                        Draw.Line(
                            screenPos + new Vector2(-sparkleSize, 0),
                            screenPos + new Vector2(sparkleSize, 0),
                            mainColor * 0.5f
                        );
                        Draw.Line(
                            screenPos + new Vector2(0, -sparkleSize),
                            screenPos + new Vector2(0, sparkleSize),
                            mainColor * 0.5f
                        );
                    }
                }
            }
        }
    }
}




