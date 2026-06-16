namespace Celeste.NPCs
{
    [Tracked]
    [CustomEntity(ids: "DZ/NPC04_Theo")]
    public class NPC04_Theo : Entity
    {
        private const string donetalking = "theoDoneTalking";
        private const string sharedCutsceneDone = "ch4MagolorAndTheoDone";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        public NPC04_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("theo"));
            sprite.Play("idle");
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

            var magolor = Scene.Tracker.GetEntity<NPC04_Magolor>();
            if (magolor != null && Scene is Level lvl && !lvl.Session.GetFlag(sharedCutsceneDone))
            {
                yield return Textbox.Say("DZ_CH4_MAGOLOR_AND_THEO", onMagolorFlyoff, onTheoWalkoff);
                lvl.Session.SetFlag(sharedCutsceneDone, true);
            }
            else
            {
                yield return Textbox.Say("DZ_CH4_MAGOLOR_AND_THEO");
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

        private IEnumerator onMagolorFlyoff()
        {
            var magolor = Scene.Tracker.GetEntity<NPC04_Magolor>();
            if (magolor != null)
            {
                yield return magolor.FlyOffRight();
            }
        }

        private IEnumerator onTheoWalkoff()
        {
            yield return WalkOffLeft();
        }

        public IEnumerator WalkOffLeft()
        {
            sprite.Play("walk");
            sprite.Scale.X = -1f;
            float startX = Position.X;
            float targetX = (Scene as Level).Bounds.Left - 64;
            float duration = 2f;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float t = Ease.SineIn(Math.Min(timer / duration, 1f));
                Position.X = MathHelper.Lerp(startX, targetX, t);
                yield return null;
            }
            RemoveSelf();
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
            
            if (sprite != null && !isInteracting)
            {
                sprite.Play("idle");
            }
        }

        public override void Removed(Scene scene)
        {
            talkRoutine?.RemoveSelf();
            base.Removed(scene);
        }
    }
}




