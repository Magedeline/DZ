using MaggyHelper.Entities;

namespace MaggyHelper.Cutscenes
{
    public class CS19_EdgeOfUniverse : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS19_EdgeOfUniverse(global::Celeste.Player player) : base()
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
            yield return 0.3f;

            yield return level.ZoomTo(
                (player.Position + new Vector2(0f, -16f)) - level.Camera.Position,
                2f, 0.5f);

            yield return Textbox.Say("CH19_EDGE_OF_UNIVERSE",
                new Func<IEnumerator>(GeneratorShatters),
                new Func<IEnumerator>(BlindingLight),
                new Func<IEnumerator>(CameraPansVoid)
            );

            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        // Trigger 0: Final generator shatters, reality cracks open
        private IEnumerator GeneratorShatters()
        {
            Level.Shake(0.5f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/new_content/game/10_farewell/glitch_medium", player.Position);
            Glitch.Value = 0.4f;
            yield return 0.8f;
            Glitch.Value = 0f;
            yield return 0.3f;
        }

        // Trigger 1: Blinding light pours through the cracks
        private IEnumerator BlindingLight()
        {
            Level.Flash(Color.White, drawPlayerOver: true);
            Audio.Play("event:/new_content/game/10_farewell/lightning_strike", player.Position);
            yield return 1.0f;
        }

        // Trigger 2: Camera pans to reveal the void beyond
        private IEnumerator CameraPansVoid()
        {
            Vector2 target = (player.Position + new Vector2(120f, -80f)) - Level.Camera.Position;
            yield return Level.ZoomTo(target, 1.5f, 1.5f);
            yield return 1.0f;
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.StateMachine.State = 0;
            Glitch.Value = 0f;
            level.ResetZoom();
            level.Session.SetFlag("edge_of_universe");
        }
    }
}
