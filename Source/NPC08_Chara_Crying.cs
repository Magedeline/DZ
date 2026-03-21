namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC08_Chara_Crying")]
    public class Npc08CharaCrying : Entity
    {
        private const string donetalking = "chara_crying";
        private const string connectionFlag = "chara_connection"; // Match cutscene flag

        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        // Single sprite property to match cutscene expectations
        public Sprite Sprite { get; private set; }

        public Npc08CharaCrying(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(Sprite = GFX.SpriteBank.Create("chara_boss"));
            Sprite.Play(Sprite.Has("scaredIdle") ? "scaredIdle" : "idle");
        }

        private void setupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-20, -8, 40, 16),
                new Vector2(0f, -24f),
                ontalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                // Check both flags for compatibility
                if (level.Session.GetFlag(donetalking) || level.Session.GetFlag(connectionFlag))
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

            yield return Textbox.Say("CHARA_08_CRYING");

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

            if (Sprite != null && !isInteracting)
            {
                Sprite.Play("idle");
            }
        }

        public override void Removed(Scene scene)
        {
            talkRoutine?.RemoveSelf();
            base.Removed(scene);
        }

        // Method used by Cs08CharaBossEnd cutscene
        public IEnumerator TurnWhite(float duration)
        {
            float t = 0f;
            Color startColor = Sprite.Color;
            while (t < duration)
            {
                t += Engine.DeltaTime;
                float progress = t / duration;
                Sprite.Color = Color.Lerp(startColor, Color.White, progress);
                yield return null;
            }
            Sprite.Color = Color.White;
        }

        // Method used by Cs08CharaBossEnd cutscene - now returns IEnumerator
        public IEnumerator Disperse()
        {
            // Add dispersal animation/effect here if needed
            float t = 0f;
            Color startColor = Sprite.Color;
            while (t < 0.5f) // Quick fade to transparent
            {
                t += Engine.DeltaTime;
                float alpha = 1f - (t / 0.5f);
                Sprite.Color = startColor * alpha;
                yield return null;
            }
            Sprite.Color = Color.Transparent;
        }
    }
}



