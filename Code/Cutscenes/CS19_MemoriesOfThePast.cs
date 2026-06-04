using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS19_MemoriesOfThePast : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS19_MemoriesOfThePast(global::Celeste.Player player) : base()
        {
            Depth = -8500;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            yield return 0.5f;

            yield return level.ZoomTo(
                (player.Position + new Vector2(0f, -16f)) - level.Camera.Position,
                2f, 0.5f);

            yield return Textbox.Say("MAGGYHELPER_CH19_MEMORIES_OF_THE_PAST",
                new Func<IEnumerator>(SilhouettesAppear),
                new Func<IEnumerator>(KirbyRushesForward),
                new Func<IEnumerator>(KirbyStops),
                new Func<IEnumerator>(MemoriesFlicker),
                new Func<IEnumerator>(RealizationHits)
            );

            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        // Trigger 0: Two silhouettes appear ahead
        private IEnumerator SilhouettesAppear()
        {
            Level.Flash(Color.White * 0.3f, drawPlayerOver: true);
            Audio.Play("guid://{42d600c5-32c3-4433-8a1e-be394b30bcde}", player.Position);
            yield return 0.8f;
        }

        // Trigger 1: Kirby rushes forward
        private IEnumerator KirbyRushesForward()
        {
            yield return player.DummyWalkTo(player.X + 40f, false, 2f, false);
            yield return 0.1f;
        }

        // Trigger 2: Kirby stops, something is wrong
        private IEnumerator KirbyStops()
        {
            player.Speed = Vector2.Zero;
            player.DummyAutoAnimate = true;
            yield return 0.5f;
        }

        // Trigger 3: Memory-Madeline and Memory-Badeline flicker
        private IEnumerator MemoriesFlicker()
        {
            for (int i = 0; i < 3; i++)
            {
                Level.Flash(Color.White * 0.15f, drawPlayerOver: true);
                yield return 0.4f;
            }
            yield return 0.3f;
        }

        // Trigger 4: Realization hits
        private IEnumerator RealizationHits()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("guid://{42d600c5-32c3-4433-8a1e-be394b30bcde}", player.Position);
            Level.Shake(0.3f);
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.DummyAutoAnimate = true;
            player.StateMachine.State = Player.StNormal;
            level.ResetZoom();
            level.Session.SetFlag("memories_of_the_past");
        }
    }
}

