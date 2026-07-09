using Celeste.Entities;
using DZ;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS20_TrueAscend : CutsceneEntity
    {
        private readonly int index;
        private readonly string dialogId;
        private readonly bool dark;
        private BadelineDummy badeline;
        private MadelineDummy madeline;
        private AsrielDummy asriel;
        private global::Celeste.Player player;
        private Vector2 origin;
        private bool spinning;

        public CS20_TrueAscend(int index, string dialogId, bool dark)
            : base(true, false)
        {
            this.index = index;
            this.dialogId = dialogId;
            this.dark = dark;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(), true));
        }

        private IEnumerator Cutscene()
        {
            while ((player = Scene.Tracker.GetEntity<global::Celeste.Player>()) == null)
                yield return null;

            origin = player.Position;
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
            player.Dashes = 5;
            player.Facing = Facings.Right;

            Scene.Add(badeline = new BadelineDummy(player.Position));
            Scene.Add(madeline = new MadelineDummy(player.Position));
            Scene.Add(asriel = new AsrielDummy(player.Position));
            badeline.AutoAnimator.Enabled = true;

            spinning = true;
            Add(new Coroutine(SpinCharacters(), true));

            yield return Textbox.Say(dialogId, new Func<IEnumerator>[]
            {
                GrannyGetSerious,   // trigger 0  - granny gets serious on the last dialog line
                KirbyFlyFinal       // trigger 1 - kirby fly final
            });

            Audio.Play("event:/char/badeline/maddy_join", player.Position);
            spinning = false;
            yield return 0.25f;

            CleanupDummies();

            player.Dashes = 5;
            player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f, null, null);

            EndCutscene(Level, true);
        }

        private IEnumerator SpinCharacters()
        {
            if (player?.Sprite == null || badeline?.Sprite == null || madeline?.Sprite == null || asriel == null)
                yield break;

            float dist = 0f;
            Vector2 center = player.Position;
            float timer = 1.5707964f;

            bool playerCanSpin = player.Sprite.Has("spin");
            bool badelineCanSpin = badeline.Sprite.Has("spin");
            bool madelineCanSpin = madeline.Sprite.Has("spin");
            bool asrielCanSpin = asriel.Sprite?.Has("spin") ?? false;

            if (playerCanSpin)
                player.Sprite.Play("spin", false, false);
            if (badelineCanSpin)
                badeline.Sprite.Play("spin", false, false);
            if (madelineCanSpin)
                madeline.Sprite.Play("spin", false, false);
            if (asrielCanSpin)
                asriel.Sprite.Play("spin", false, false);

            badeline.Sprite.Scale.X = 1f;
            madeline.Sprite.Scale.X = 1.5f;
            if (asriel.Sprite != null)
                asriel.Sprite.Scale.X = 2f;

            while (spinning || dist > 0f)
            {
                if (player?.Sprite == null || badeline?.Sprite == null || madeline?.Sprite == null || asriel?.Sprite == null)
                    yield break;

                dist = Calc.Approach(dist, spinning ? 2f : 0f, Engine.DeltaTime * 4f);
                int frame = (int)(timer / 6.2831855f * 14f + 10f);
                float s = (float)Math.Sin(timer);
                float c = (float)Math.Cos(timer);
                float radius = Ease.CubeOut(dist) * 32f;

                if (playerCanSpin)
                    player.Sprite.SetAnimationFrame(frame);
                if (badelineCanSpin)
                    badeline.Sprite.SetAnimationFrame(frame + 7);
                if (madelineCanSpin)
                    madeline.Sprite.SetAnimationFrame(frame + 7);
                if (asrielCanSpin)
                    asriel.Sprite.SetAnimationFrame(frame + 7);

                player.Position = center + new Vector2(s * radius, c * dist * 8f);
                badeline.Position = center + new Vector2((float)Math.Sin(timer + Math.PI / 3) * radius, (float)Math.Cos(timer + Math.PI / 3) * dist * 8f);
                madeline.Position = center + new Vector2((float)Math.Sin(timer + 2 * Math.PI / 3) * radius, (float)Math.Cos(timer + 2 * Math.PI / 3) * dist * 8f);
                asriel.Position = center + new Vector2((float)Math.Sin(timer + Math.PI) * radius, (float)Math.Cos(timer + Math.PI) * dist * 8f);

                timer -= Engine.DeltaTime * 2f;
                if (timer <= 0f)
                    timer += 6.2831855f;

                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            spinning = false;
            CleanupDummies();

            if (player != null)
            {
                player.Dashes = 5;
                player.Position = origin;
                player.CreateSplitParticles();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                level?.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
            }

            if (!dark)
                level.Add(new FinalTitanHeightDisplayMod(index));
        }

        // trigger 0 - placeholder flash until granny sprites are ready
        private IEnumerator GrannyGetSerious()
        {
            Audio.SetMusicParam("progress", 12f);
            Audio.Play("event:/DZ/new_content/char/granny/serious");
            Level.Flash(Color.White, false);
            yield break;
        }

        // trigger 1 - alt sfx replacing flynext
        private IEnumerator KirbyFlyFinal()
        {
            Audio.Play("event:/DZ/new_content/char/kirby/flyfinal");
            yield break;
        }

        private void CleanupDummies()
        {
            badeline?.RemoveSelf();
            madeline?.RemoveSelf();
            asriel?.RemoveSelf();
            badeline = null;
            madeline = null;
            asriel = null;
        }
    }
}

