using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Meta Knight NPC for Chapter 13 - Can be corrupted or normal
    /// </summary>
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC_MetaKnight")]
    public class NPC_MetaKnight : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting;
        private bool isCorrupted;
        private string encounterType;
#pragma warning disable CS0414
        private string cutsceneId;
#pragma warning restore CS0414

        public NPC_MetaKnight(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            encounterType = data.Attr("encounterType", "normal");
            isCorrupted = encounterType == "corrupted";
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            try
            {
                // Use Badeline sprite for dark Meta Knight look
                Add(sprite = GFX.SpriteBank.Create("maggy_badeline"));
                sprite.Play("idle");

                if (isCorrupted)
                {
                    // Corrupted Meta Knight - darker, more mechanical look
                    sprite.Color = Color.DarkBlue;
                    cutsceneId = "DZ_CH13_METAMINATOR_KNIGHT_INTRO";
                }
                else
                {
                    // Normal Meta Knight
                    sprite.Color = Color.Blue;
                }

                IngesteLogger.Debug($"NPC_MetaKnight sprite setup complete - Corrupted: {isCorrupted}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_MetaKnight sprite");
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
                IngesteLogger.Error(ex, "Error setting up NPC_MetaKnight collision");
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

            if (isCorrupted && level?.Session.GetFlag("meta_knight_restored") != true)
            {
                // Corrupted Meta Knight encounter
                Scene.Add(new CS13_MetaKnightEncounter(player));

                // After battle, restore Meta Knight
                level?.Session.SetFlag("meta_knight_restored");
                isCorrupted = false;
                sprite.Color = Color.Blue;
            }
            else
            {
                // Normal Meta Knight dialogue
                // MultiCharacterCutscene doesn't exist
                // Scene.Add(new MultiCharacterCutscene(player, "DZ_CH13_META_KNIGHT_NORMAL"));
            }

            isInteracting = false;
        }

        public override void Update()
        {
            base.Update();

            // Meta Knight's cape-like animation
            if (sprite != null && isCorrupted)
            {
                // Glitchy movement for corrupted state
                if (Scene.OnInterval(0.1f))
                {
                    sprite.Position = new Vector2(
                        Calc.Random.Range(-1f, 1f),
                        sprite.Position.Y
                    );
                }
            }
        }
    }
}
