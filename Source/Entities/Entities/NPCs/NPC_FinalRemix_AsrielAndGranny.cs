using System.Collections;
using Celeste.Entities;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "DZ/NPC_FinalRemix_AsrielAndGranny")]
    public class NPC_FinalRemix_AsrielAndGranny : Entity
    {
        private const string donetalking = "finalRemixAsrielAndGrannyDoneTalking";

        private Sprite asrielSprite;
        private Sprite grannySprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        private Vector2 grannyOffset = new Vector2(32f, 0f);

        public NPC_FinalRemix_AsrielAndGranny(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprites();
            setupCollision();
            Depth = 100;
        }

        private void setupSprites()
        {
            Add(asrielSprite = GFX.SpriteBank.Create("asriel"));
            asrielSprite.Play("idle");

            Add(grannySprite = GFX.SpriteBank.Create("granny"));
            grannySprite.Play("idle");
            grannySprite.Position = grannyOffset;
        }

        private void setupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-20, -8, 72, 16),
                new Vector2(16f, -24f),
                ontalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                if (level.Session.GetFlag(donetalking))
                {
                    talker.Enabled = false;
                    return;
                }
            }

            talker.Enabled = true;
        }

        private void ontalk(global::Celeste.Player player)
        {
            if (isInteracting) return;

            if (Scene is Level level)
            {
                isInteracting = true;
                level.StartCutscene(ontalkend);
                Add(talkRoutine = new Coroutine(talkcoroutine(player)));
            }
        }

        private IEnumerator talkcoroutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            // Check if player is carrying a berry
            bool hasGolden = false;
            bool hasPinkPlat = false;

            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Entities.GoldenStrawberry)
                {
                    hasGolden = true;
                }
                else if (follower.Entity is PinkPlatinumBerry)
                {
                    hasPinkPlat = true;
                }
            }

            // Play appropriate dialog based on berry type (Pink Platinum takes priority)
            if (hasPinkPlat)
            {
                yield return Textbox.Say("DZ_CH19_21_PINK_PLATINUM_BERRY");
            }
            else if (hasGolden)
            {
                yield return Textbox.Say("DZ_CH19_21_GOLDEN_BERRY");
            }
            else
            {
                // Default dialog if no berry
                yield return Textbox.Say("DZ_CH19_21_GOLDEN_BERRY");
            }

            endcutscene();
        }

        private void endcutscene()
        {
            if (Scene is Level level)
            {
                level.EndCutscene();
                ontalkend(level);
            }
        }

        private void ontalkend(Level level)
        {
            isInteracting = false;
            level.Session.SetFlag(donetalking, true);
            talker.Enabled = false;

            talkRoutine?.RemoveSelf();
            talkRoutine = null;

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            player?.StateMachine.SetStateName(global::Celeste.Player.StNormal, "idle");
        }

        public override void Update()
        {
            base.Update();

            if (!isInteracting)
            {
                asrielSprite?.Play("idle");
                grannySprite?.Play("idle");
            }
        }

        public override void Removed(Scene scene)
        {
            talkRoutine?.RemoveSelf();
            base.Removed(scene);
        }
    }
}
