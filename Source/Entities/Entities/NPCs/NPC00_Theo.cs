using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "DZ/NPC00_Theo, MaggyHelper/NPC00_Theo")]
    [Tracked(true)]
    public class Npc00Theo : Entity
    {
        private string flagName;
        private string dialogKey;
        private string spriteId;

        private Sprite sprite;
        public Sprite TheoSprite => sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private VertexLight light;
        private bool isInteracting = false;

        public Npc00Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            // Read entity data from Loenn
            flagName = data.Attr("flagName", "theo_00_house");
            dialogKey = data.Attr("dialogKey", "ingeste_theo_00_house");
            spriteId = data.Attr("spriteId", "theo");

            SetupSprite();
            SetupCollision();

            // Add vertex light like other NPCs
            Add(light = new VertexLight(Color.White, 1f, 16, 32));

            Depth = 0; // Match Loenn depth
        }

        private void SetupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create(spriteId));
            // Set justification to bottom-center (0.5, 1.0) to match Loenn
            sprite.Justify = new Vector2(0.5f, 1.0f);
            sprite.Play("idle");
        }

        private void SetupCollision()
        {
            // Hitbox centered on sprite position, adjusted for bottom-center justification
            Collider = new Hitbox(16f, 24f, -8f, -24f);

            Add(talker = new TalkComponent(
                new Rectangle(-16, -32, 32, 32),
                new Vector2(0f, -32f),
                OnTalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                // Check if the cutscene has been completed
                if (level.Session.GetFlag(flagName))
                {
                    talker.Enabled = false;
                    return;
                }
            }

            talker.Enabled = true;
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (isInteracting) return;

            if (Scene is Level level)
            {
                isInteracting = true;

                // Start the full cutscene instead of simple dialog
                level.StartCutscene(OnTalkEnd);

                // Create and start the CS00_Theo cutscene
                var cutscene = new Cs00Theo(player);
                level.Add(cutscene);
            }
        }

        private void OnTalkEnd(Level level)
        {
            isInteracting = false;

            // The cutscene sets the flag, so we check it here
            if (level.Session.GetFlag(flagName))
            {
                talker.Enabled = false;
            }

            talkRoutine?.RemoveSelf();
            talkRoutine = null;

            // Player state restoration is handled by the cutscene
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && player.StateMachine.State == global::Celeste.Player.StDummy)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }

        public override void Update()
        {
            base.Update();

            if (sprite != null && !isInteracting)
            {
                // Only set idle if not already playing idle (prevents animation reset)
                if (sprite.CurrentAnimationID != "idle")
                {
                    sprite.Play("idle");
                }
            }

            // Update light visibility during transitions
            if (light != null && Scene is Level level)
            {
                Rectangle bounds = level.Bounds;
                light.Alpha = Calc.Approach(light.Alpha,
                    (X <= bounds.Left - 16 || Y <= bounds.Top - 16 ||
                     X >= bounds.Right + 16 || Y >= bounds.Bottom + 16 ||
                     level.Transitioning)
                        ? 0.0f
                        : 1f, Engine.DeltaTime * 2f);
            }
        }

        public override void Removed(Scene scene)
        {
            talkRoutine?.RemoveSelf();
            base.Removed(scene);
        }
    }
}
