namespace Celeste.Entities
{
    /// <summary>
    /// A core message entity that displays text with a shimmering bright effect.
    /// Used for the mysterious man from the vessel creation in the void of the heart
    /// of the Desolo Zantas mountain.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MysteryManCoreMessage")]
    [Tracked]
    public class MysteryManCoreMessage : Entity
    {
        // Dialog key for fetching text
        private string dialogKey;
        
        // The line index to display
        private int lineIndex;
        
        // The text to display
        private string text;
        
        // Alpha transparency based on player distance
        private float alpha;
        
        // Timer for shimmer animation
        private float shimmerTimer;
        
        // Shimmer speed
        private float shimmerSpeed;
        
        // Base color for the text
        private Color baseColor;
        
        // Secondary shimmer color
        private Color shimmerColor;
        
        // Text scale
        private float textScale;
        
        // Whether to use rainbow shimmer effect
        private bool useRainbowShimmer;
        
        // Distance threshold for visibility
        private float visibilityDistance;

        public MysteryManCoreMessage(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = Tags.HUD;
            
            // Get configuration from entity data
            dialogKey = data.Attr("dialogKey", "MAGGYHELPER_CH18_ENDING");
            lineIndex = data.Int("line", 0);
            shimmerSpeed = data.Float("shimmerSpeed", 2.0f);
            textScale = data.Float("textScale", 1.25f);
            useRainbowShimmer = data.Bool("useRainbowShimmer", false);
            visibilityDistance = data.Float("visibilityDistance", 128f);
            
            // Parse base color (default: bright cyan/white shimmer)
            string baseColorHex = data.Attr("baseColor", "E0FFFF");
            baseColor = Calc.HexToColor(baseColorHex);
            
            // Parse shimmer color (default: golden/yellow shimmer)
            string shimmerColorHex = data.Attr("shimmerColor", "FFD700");
            shimmerColor = Calc.HexToColor(shimmerColorHex);
            
            // Fetch the text from dialog
            LoadDialogText();
        }

        private void LoadDialogText()
        {
            try
            {
                string[] lines = Dialog.Clean(dialogKey).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineIndex >= 0 && lineIndex < lines.Length)
                {
                    text = lines[lineIndex];
                }
                else
                {
                    text = $"[Line {lineIndex} not found in {dialogKey}]";
                }
            }
            catch
            {
                text = $"[Dialog '{dialogKey}' not found]";
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Update shimmer timer
            shimmerTimer += Engine.DeltaTime * shimmerSpeed;
            
            // Calculate alpha based on player distance
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float distance = Math.Abs(X - player.X);
                alpha = Ease.CubeInOut(Calc.ClampedMap(distance, 0f, visibilityDistance, 1f, 0f));
            }
            else
            {
                alpha = 0f;
            }
        }

        public override void Render()
        {
            if (alpha <= 0f || string.IsNullOrEmpty(text))
                return;
            
            Level level = Scene as Level;
            if (level == null)
                return;
            
            // Calculate screen position with parallax effect
            Vector2 cameraPosition = level.Camera.Position;
            Vector2 screenCenter = cameraPosition + new Vector2(160f, 90f);
            Vector2 renderPosition = (Position - cameraPosition + (Position - screenCenter) * 0.2f) * 6f;
            
            // Handle mirror mode
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
            {
                renderPosition.X = 1920f - renderPosition.X;
            }
            
            // Calculate shimmer effect
            Color currentColor = GetShimmerColor();
            
            // Draw outline/shadow for better visibility
            Color shadowColor = Color.Black * alpha * 0.5f;
            Vector2 origin = new Vector2(0.5f, 0.5f);
            Vector2 scale = Vector2.One * textScale;
            
            // Draw shadow
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        ActiveFont.Draw(text, renderPosition + new Vector2(x, y), origin, scale, shadowColor);
                    }
                }
            }
            
            // Draw main text with shimmer
            ActiveFont.Draw(text, renderPosition, origin, scale, currentColor * alpha);
            
            // Draw highlight shimmer overlay
            Color highlightColor = GetHighlightColor();
            ActiveFont.Draw(text, renderPosition, origin, scale, highlightColor * alpha * 0.3f);
        }

        private Color GetShimmerColor()
        {
            if (useRainbowShimmer)
            {
                return GetRainbowColor(shimmerTimer);
            }
            
            // Smooth oscillation between base and shimmer colors
            float t = (float)(Math.Sin(shimmerTimer) * 0.5 + 0.5);
            return Color.Lerp(baseColor, shimmerColor, t);
        }

        private Color GetHighlightColor()
        {
            if (useRainbowShimmer)
            {
                return GetRainbowColor(shimmerTimer + 1.0f) * 0.5f;
            }
            
            // Create a bright white highlight that pulses
            float pulse = (float)(Math.Sin(shimmerTimer * 2.0f) * 0.5 + 0.5);
            return Color.White * pulse;
        }

        private Color GetRainbowColor(float time)
        {
            float r = (float)(Math.Sin(time) * 0.5 + 0.5);
            float g = (float)(Math.Sin(time + 2.094f) * 0.5 + 0.5);
            float b = (float)(Math.Sin(time + 4.189f) * 0.5 + 0.5);
            return new Color(r, g, b);
        }
    }
}
