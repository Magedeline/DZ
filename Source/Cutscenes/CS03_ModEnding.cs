#nullable enable
using BirdNPC = Celeste.Entities.DZBirdNPC;
namespace Celeste.Cutscenes
{
    [HotReloadable]
    class Cs03ModEnding : CutsceneEntity
    {
        private global::Celeste.Player player;
        private Bonfire? bonfire;
        private BadelineDummy? badeline;

        public Cs03ModEnding(global::Celeste.Player player, Bonfire? bonfire)
            : base(false, true)
        {
            this.player = player;
            this.bonfire = bonfire;
        }

        // Implemented constructor
        public Cs03ModEnding(global::Celeste.Player player)
            : base(false, true)
        {
            this.player = player;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Attempt to get the Bonfire object from the Scene's tracker
            this.bonfire = Scene?.Tracker.GetEntity<Bonfire>();

            // Optional: Handle a case where the Bonfire isn't found
            if (this.bonfire == null)
            {
                throw new InvalidOperationException("Bonfire entity could not be found in the current scene.");
            }
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            bonfire = Scene.Tracker.GetEntity<Bonfire>();
            Add(new Coroutine(cutscene(level)));
        }

        private IEnumerator cutscene(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            player.Dashes = 1;
            level.Session.Audio.Music.Layer(3, false);
            level.Session.Audio.Apply();
            yield return 0.5f;
            if (bonfire != null)
                yield return player.DummyWalkTo(bonfire.X + 40f);
            yield return 1.5f;
            player.Facing = Facings.Left;
            yield return 0.5f;
            yield return Textbox.Say("DZ_CH3_END_KIRBY",
                trigger0BadelineAppears,
                trigger1GiveStrawberry,
                trigger2BadelineLooksLeft,
                trigger3BadelineSitsAndBird);
            yield return 0.3f;
            EndCutscene(level);
        }

        // trigger 0 â€“ Badeline drops from the air and lands next to the player
        private IEnumerator trigger0BadelineAppears()
        {
            Vector2 landPos = player.Position + new Vector2(60f, 0f);
            badeline = new BadelineDummy(player.Position + new Vector2(60f, -200f));
            Scene.Add(badeline);
            Vector2 from = badeline.Position;
            float t = 0f;
            while (t < 1f)
            {
                badeline.Position = from + (landPos - from) * Ease.QuadOut(t);
                t += Engine.DeltaTime * 1.5f;
                yield return null;
            }
            badeline.Position = landPos;
            yield return 0.3f;
        }

        // trigger 1 â€“ give the player a strawberry
        private IEnumerator trigger1GiveStrawberry()
        {
            Audio.Play(SFX.game_gen_strawberry_get, player.Position);
            yield return 0.3f;
        }

        // trigger 2 â€“ Badeline turns to look left
        private IEnumerator trigger2BadelineLooksLeft()
        {
            if (badeline != null)
            {
                badeline.Sprite.Scale = new Vector2(-Math.Abs(badeline.Sprite.Scale.X), badeline.Sprite.Scale.Y);
                badeline.Hair.Facing = Facings.Left;
            }
            yield return null;
        }

        // trigger 3 â€“ Badeline sits to rest; bird swoops in and falls asleep on her head
        private IEnumerator trigger3BadelineSitsAndBird()
        {
            if (badeline != null)
                badeline.Sprite.Play("laugh");

            player.DummyAutoAnimate = false;
            player.Sprite.Play("sleep");
            Audio.Play("event:/DZ/char/kirby/campfire_sit", player.Position);
            yield return 2f;

            Vector2 birdOrigin = (badeline?.Position ?? player.Position) + new Vector2(88f, -200f);
            global::Celeste.Entities.DZBirdNPC bird = new global::Celeste.Entities.DZBirdNPC(birdOrigin, global::Celeste.Entities.DZBirdNPC.Modes.None);
            Scene.Add(bird);
            FMOD.Studio.EventInstance? instance = Audio.Play("event:/game/general/bird_in", bird.Position);
            bird.Facing = Facings.Left;
            bird.Sprite.Play("fall");

            Vector2 from = bird.Position;
            Vector2 to = (badeline?.Position ?? player.Position) + new Vector2(0f, -32f);
            float percent = 0f;
            while (percent < 1f)
            {
                bird.Position = from + (to - from) * Ease.QuadOut(percent);
                Audio.Position(instance, bird.Position);
                if (percent > 0.5f)
                    bird.Sprite.Play("fly");
                percent += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            bird.Position = to;
            bird.Sprite.Play("idle");
            yield return 0.5f;
            bird.Sprite.Play("croak");
            yield return 0.6f;
            Audio.Play("event:/game/general/bird_squawk", bird.Position);
            yield return 0.9f;
            bird.Sprite.Play("sleep");
            yield return 2f;
        }

        public override void OnEnd(Level level)
        {
            level.CompleteArea(true, false, false);
        }
    }
}





