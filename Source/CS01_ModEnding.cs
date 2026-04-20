namespace Celeste.Cutscenes
{
    [HotReloadable]
    [Tracked]
    public class Cs01ModEnding : CutsceneEntity
    {
        private Bonfire bonfire;
        private global::Celeste.Player player;

        public Cs01ModEnding(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bonfire = Scene.Tracker.GetEntity<Bonfire>();
        }

        // Prepare the player for the cutscene
        private void SetupPlayer()
        {
            if (player == null)
                return;

            try
            {
                player.StateMachine.State = 11; // dummy state used across cutscenes
                player.StateMachine.Locked = true;
            }
            catch
            {
            }

            player.ForceCameraUpdate = true;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene(level)));
        }

        private IEnumerator cutscene(Level level)
        {
            SetupPlayer();
            player.Dashes = 1;
            level.Session.Audio.Music.Layer(3, false);
            level.Session.Audio.Apply();
            yield return 0.5f;

            if (bonfire != null)
                yield return player.DummyWalkToExact((int)(bonfire.X + 40f));

            yield return 1.5f;
            player.Facing = Facings.Left;
            yield return 0.5f;

            yield return Textbox.Say("CH1_ENDMADELINE", ModEndCityTrigger);
            yield return 0.3f;

            EndCutscene(level);
        }

        private IEnumerator ModEndCityTrigger()
        {
            yield return 0.2f;

            if (bonfire != null)
            {
                yield return player.DummyWalkToExact((int)(bonfire.X - 12f));
                yield return 0.2f;
                player.Facing = Facings.Right;
                player.DummyAutoAnimate = false;
                player.Sprite.Play("duck");
                yield return 0.5f;
                bonfire.SetMode(Bonfire.Mode.Lit);
                yield return 1f;
                player.Sprite.Play("idle");
                yield return 0.4f;
                player.DummyAutoAnimate = true;
                yield return player.DummyWalkToExact((int)(bonfire.X - 24f));
            }

            yield return 0.4f;
            player.DummyAutoAnimate = false;
            player.Facing = Facings.Right;
            player.Sprite.Play("sleep");
            Audio.Play("event:/desolozantas/char/kirby/campfire_sit", player.Position);
            yield return 4f;

            // Add bird scene
            var bird = new BirdNPC(player.Position + new Vector2(88f, -200f), BirdNPC.Modes.None);
            Scene.Add(bird);
            var instance = Audio.Play("event:/game/general/bird_in", bird.Position);
            bird.Facing = Facings.Left;
            bird.Sprite.Play("fall");

            Vector2 from = bird.Position;
            Vector2 to = player.Position + new Vector2(1f, -12f);
            float percent = 0.0f;

            while (percent < 1.0f)
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
            level.CompleteArea(true, true, true);
        }
    }
}



