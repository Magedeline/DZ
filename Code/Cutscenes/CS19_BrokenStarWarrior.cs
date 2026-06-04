using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS19_BrokenStarWarrior : CutsceneEntity
    {
        private readonly global::Celeste.Player player;
        private CharaDummy chara;

        public CS19_BrokenStarWarrior(global::Celeste.Player player) : base()
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

            yield return Textbox.Say("MAGGYHELPER_CH19_BROKEN_STAR_WARRIOR",
                new Func<IEnumerator>(KirbyAloneInDark),
                new Func<IEnumerator>(KirbyDropsToKnees),
                new Func<IEnumerator>(CharaAndOthersAppear),
                new Func<IEnumerator>(KirbyLooksTowardBird)
            );

            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        // Trigger 0: Kirby alone in the dark
        private IEnumerator KirbyAloneInDark()
        {
            Level.NextColorGrade("feelingdown", 0.25f);
            yield return 0.8f;
        }

        // Trigger 1: Kirby drops to his knees
        private IEnumerator KirbyDropsToKnees()
        {
            player.DummyAutoAnimate = false;
            Audio.Play("event:/new_content/char/pusheen/kirby/heartbreak", player.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 0.6f;
        }

        // Trigger 2: Chara and others appear
        private IEnumerator CharaAndOthersAppear()
        {
            Vector2 charaPos = player.Position + new Vector2(24f, 0f);
            Level.Displacement.AddBurst(charaPos, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("guid://{450fb5b3-e9e3-45d8-9f34-ba05e292958f}", charaPos);
            Level.Add(chara = new CharaDummy(charaPos));
            chara.Sprite.Scale.X = -1f;
            yield return 0.4f;
        }

        // Trigger 3: Kirby looks toward the bird
        private IEnumerator KirbyLooksTowardBird()
        {
            player.DummyAutoAnimate = true;
            player.Facing = Facings.Right;
            Level.NextColorGrade(null, 0.5f);
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.DummyAutoAnimate = true;
            player.StateMachine.State = Player.StNormal;
            if (chara != null)
                chara.RemoveSelf();
            level.ResetZoom();
            level.SnapColorGrade(null);
            level.Session.SetFlag("broken_star_warrior");
        }
    }
}

