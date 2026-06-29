using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Digital Guide NPC for Chapter 14 - Digital Dimension helper
    /// </summary>
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC_DigitalGuide")]
    public class NPC_DigitalGuide : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting;
        private float glitchTimer;

        public NPC_DigitalGuide(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            try
            {
                // Use Theo sprite with digital effects
                Add(sprite = GFX.SpriteBank.Create("theo"));
                sprite.Play("idle");
                
                // Digital/pixelated appearance
                sprite.Color = Color.Cyan;
                
                IngesteLogger.Debug("NPC_DigitalGuide sprite setup complete");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_DigitalGuide sprite");
                sprite = null;
            }
        }

        private void SetupCollision()
        {
            try
            {
                Add(talker = new TalkComponent(
                    new Rectangle(-20, -8, 40, 16),
                    new Vector2(0f, -24f),
                    OnTalk
                ));
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_DigitalGuide collision");
                talker = null;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
          
            if (talker != null)
            {
                talker.Enabled = true;
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (isInteracting)
                return;

            isInteracting = true;
     
            Level level = Scene as Level;
        
            if (level?.Session.GetFlag("hollow_programmer_defeated") == true)
            {
                // Post-corruption dialogue
                // MultiCharacterCutscene doesn't exist
                // Scene.Add(new MultiCharacterCutscene(player, "DZ_CH14_DIGITAL_DIMENSION_RESTORED"));
            }
            else
            {
                // Initial warning about corruption
                Scene.Add(new CS14_Intro(player));
            }

            isInteracting = false;
        }

        public override void Update()
        {
            base.Update();
    
            if (sprite != null)
            {
                glitchTimer += Engine.DeltaTime;
    
                // Digital glitch effects
                if (glitchTimer > 0.5f)
                {
                    glitchTimer = 0f;
                    
                    // Random color shifts for digital effect
                    var colors = new[] { Color.Cyan, Color.Lime, Color.Magenta, Color.Yellow };
                    sprite.Color = Calc.Random.Choose(colors);
                }
    
                // Floating/hovering effect
                float hover = (float)Math.Sin(Scene.TimeActive * 2f) * 4f;
                sprite.Position.Y = hover;
            }
        }
    }
}
