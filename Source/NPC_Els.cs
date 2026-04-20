using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Els/Flowey NPC for Chapter 16 - Final Boss
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/NPC_Els")]
    public class NPC_Els : NpcBase
    {
        private string elsForm; // "flowey", "genocider", "corrupted"
        private bool isPowerful;
        private float corruptionEffect;
        private Sprite sprite;

        public NPC_Els(EntityData data, Vector2 offset) : base(data.Position + offset, "CH16_CORRUPTED_REALITY_INTRO")
        {
            elsForm = data.Attr("elsForm", "flowey");
            isPowerful = elsForm == "genocider" || elsForm == "corrupted";
            
            // Larger interaction radius for final boss
            InteractRadius = 96f;
            
            SetupSprite();
        }

        private void SetupSprite()
        {
            try
            {
                // Use Badeline sprite for dark, evil appearance
                Add(sprite = GFX.SpriteBank.Create("maggy_badeline"));
                sprite.Play("idle");
                
                switch (elsForm)
                {
                    case "flowey":
                        sprite.Color = Color.Yellow;
                        sprite.Scale = Vector2.One * 1.0f;
                        CutsceneId = "CH15_FLOWEY_ATTACK";
                        break;
                    case "genocider":
                        sprite.Color = Color.DarkRed;
                        sprite.Scale = Vector2.One * 1.5f;
                        CutsceneId = "CH16_CORRUPTED_REALITY_INTRO";
                        break;
                    case "corrupted":
                        sprite.Color = Color.Purple;
                        sprite.Scale = Vector2.One * 2.0f;
                        CutsceneId = "CH16_FINAL_CONFRONTATION";
                        break;
                }
                
                IngesteLogger.Debug($"NPC_Els sprite setup complete - Form: {elsForm}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Els sprite");
                sprite = null;
            }
        }

        protected override void Interact(global::Celeste.Player player)
        {
            if (!Interacting)
            {
                Interacting = true;
                
                Level level = Scene as Level;
                
                // Use simple cutscene trigger instead of complex cutscene classes
                string cutsceneToPlay = elsForm switch
                {
                    "flowey" => "CH15_FLOWEY_ATTACK",
                    "genocider" => "CH16_CORRUPTED_REALITY_INTRO", 
                    "corrupted" => "CH16_FINAL_CONFRONTATION",
                    _ => CutsceneId
                };
                
                Scene.Add(new CutsceneTrigger(cutsceneToPlay, player, () => Interacting = false));
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (sprite != null)
            {
                corruptionEffect += Engine.DeltaTime;
                
                if (isPowerful)
                {
                    // Reality-warping effects for powerful forms
                    if (Scene.OnInterval(0.1f))
                    {
                        (Scene as Level)?.Shake(0.2f);
                    }
                    
                    // Glitchy color shifts
                    if (corruptionEffect > 0.3f)
                    {
                        corruptionEffect = 0f;
                        var darkColors = new[] { Color.DarkRed, Color.Purple, Color.Black, Color.DarkGray };
                        sprite.Color = Calc.Random.Choose(darkColors);
                    }
                    
                    // Menacing scale pulse
                    float scalePulse = 1f + (float)Math.Sin(Scene.TimeActive * 4f) * 0.2f;
                    sprite.Scale = Vector2.One * scalePulse * (elsForm == "corrupted" ? 2.0f : 1.5f);
                }
                else
                {
                    // Normal Flowey movement
                    float sway = (float)Math.Sin(Scene.TimeActive * 3f) * 2f;
                    sprite.Position.X = sway;
                }
            }
        }
    }
}



