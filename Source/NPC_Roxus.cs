using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Roxus - Pirate Captain NPC for Chapter 10
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/NPC_Roxus")]
    public class NPC_Roxus : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting = false;

        public NPC_Roxus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            try
            {
                // Try to use existing sprite or create fallback
                Add(sprite = GFX.SpriteBank.Create("theo")); // Using Theo sprite as placeholder
                sprite.Play("idle");
                sprite.Scale = new Vector2(-1, 1); // Face left for pirate look
                
                IngesteLogger.Debug("NPC_Roxus sprite setup complete");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Roxus sprite");
                // Create simple rectangle as fallback
                sprite = null;
            }
        }

        private void SetupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-16, -8, 32, 16),
                Vector2.Zero,
                OnTalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            IngesteLogger.Debug("NPC_Roxus added to scene");
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (!isInteracting)
            {
                isInteracting = true;
                
                // Check if already talked to Roxus
                Level level = Scene as Level;
                if (level?.Session.GetFlag("roxus_met") == true)
                {
                    // Already met, different dialogue
                    Scene.Add(new CS10_RoxusStart(player));
                }
                else
                {
                    // First meeting
                    level?.Session.SetFlag("roxus_met");
                    Scene.Add(new CS10_RoxusStart(player));
                }
                
                isInteracting = false;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Simple animation logic
            if (sprite != null && Scene?.OnInterval(2f) == true)
            {
                sprite.Play("idle");
            }
        }
    }
}



