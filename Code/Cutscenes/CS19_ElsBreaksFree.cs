using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS19_ElsBreaksFree : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS19_ElsBreaksFree(global::Celeste.Player player) : base()
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

            yield return Textbox.Say("MAGGYHELPER_CH19_ELS_BREAKS_FREE",
                new Func<IEnumerator>(BlackholeFlickers),
                new Func<IEnumerator>(ZeroEnergyPulses),
                new Func<IEnumerator>(BlackholeIntensifies),
                new Func<IEnumerator>(ElsVanishes)
            );

            EndCutscene(level);
        }

        // Trigger 0: Grand Sunset Blackhole Zero 3 flickers in the sky
        private IEnumerator BlackholeFlickers()
        {
            for (int i = 0; i < 4; i++)
            {
                Level.Flash(Color.Black * 0.4f, drawPlayerOver: false);
                yield return 0.3f;
            }
            Level.Shake(0.2f);
            yield return 0.5f;
        }

        // Trigger 1: Zero 3 energy pulses
        private IEnumerator ZeroEnergyPulses()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/new_content/game/10_farewell/glitch_short", player.Position);
            Level.Shake(0.3f);
            yield return 0.8f;
        }

        // Trigger 2: Blackhole intensifies
        private IEnumerator BlackholeIntensifies()
        {
            Input.Rumble(RumbleStrength.Climb, RumbleLength.Long);
            Level.Flash(Color.White * 0.6f, drawPlayerOver: true);
            Level.Shake(0.5f);
            Audio.Play("event:/new_content/game/10_farewell/glitch_medium", player.Position);
            yield return 1.2f;
        }

        // Trigger 3: Els vanishes, blackhole stabilizes ominously
        private IEnumerator ElsVanishes()
        {
            Level.Flash(Color.Black * 0.5f, drawPlayerOver: false);
            Audio.Play("event:/char/badeline/disappear", player.Position);
            yield return 0.6f;
            // Stabilize — rumble fades
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            yield return 0.4f;
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.StateMachine.State = Player.StNormal;
            level.Session.SetFlag("els_breaks_free");
        }
    }
}
