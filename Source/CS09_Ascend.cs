using Celeste.Entities;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    public class CS09_Ascend : CutsceneEntity
    {
        private int index;
        private string cutscene;
        private bool dark;
        private BadelineDummy badeline;
        private RalseiDummy ralsei;
        private CharaDummy chara;
        private global::Celeste.Player player;
        private Vector2 origin;
        private bool spinning;

        public CS09_Ascend(int index, string cutscene, bool dark) : base(true, false)
        {
            this.index = index;
            this.cutscene = cutscene;
            this.dark = dark;
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(), true));
        }

        private IEnumerator Cutscene()
        {
            while ((this.player = base.Scene.Tracker.GetEntity<global::Celeste.Player>()) == null)
            {
                yield return null;
            }
            this.origin = this.player.Position;
            Audio.Play("event:/char/badeline/maddy_split");
            this.player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            this.Level.Displacement.AddBurst(this.player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
            this.player.Dashes = 5;
            this.player.Facing = Facings.Right;
            base.Scene.Add(this.badeline = new BadelineDummy(this.player.Position));
            base.Scene.Add(this.ralsei = new RalseiDummy(this.player.Position));
            base.Scene.Add(this.chara = new CharaDummy(this.player.Position));
            this.badeline.AutoAnimator.Enabled = true;
            this.spinning = true;
            base.Add(new Coroutine(this.SpinCharacters(), true));
            yield return Textbox.Say(this.cutscene, new Func<IEnumerator>[0]);
            Audio.Play("event:/char/badeline/maddy_join");
            this.spinning = false;
            yield return 0.25f;
            this.badeline.RemoveSelf();
            this.ralsei.RemoveSelf();
            this.chara.RemoveSelf();
            this.player.Dashes = 5;
            this.player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            this.Level.Displacement.AddBurst(this.player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
            base.EndCutscene(this.Level, true);
            yield break;
        }

        private IEnumerator SpinCharacters()
        {
            float dist = 0f;
            Vector2 center = this.player.Position;
            float timer = 1.5707964f; // pi/2
            this.player.Sprite.Play("spin", false, false);
            this.badeline.Sprite.Play("spin", false, false);
            this.ralsei.Sprite.Play("spin", false, false);
            (this.chara.Sprite as Sprite)?.Play("spin", false, false);
            this.badeline.Sprite.Scale.X = 1f;
            this.ralsei.Sprite.Scale.X = 1.5f;
            var charaSprite = this.chara.Sprite as Sprite;
            if (charaSprite != null) charaSprite.Scale.X = 2f;
            while (this.spinning || dist > 0f)
            {
                dist = Calc.Approach(dist, this.spinning ? 2f : 0f, Engine.DeltaTime * 4f);
                int frame = (int)(timer / 6.2831855f * 14f + 10f);
                float s = (float)Math.Sin(timer);
                float c = (float)Math.Cos(timer);
                float radius = Ease.CubeOut(dist) * 32f;
                this.player.Sprite.SetAnimationFrame(frame);
                this.badeline.Sprite.SetAnimationFrame(frame + 7);
                this.ralsei.Sprite.SetAnimationFrame(frame + 7);
                (this.chara.Sprite as Sprite)?.SetAnimationFrame(frame + 7);
                // Distribute characters evenly around the circle
                this.player.Position = center + new Vector2(s * radius, c * dist * 8f);
                this.badeline.Position = center + new Vector2((float)Math.Sin(timer + Math.PI / 3) * radius, (float)Math.Cos(timer + Math.PI / 3) * dist * 8f);
                this.ralsei.Position = center + new Vector2((float)Math.Sin(timer + 2 * Math.PI / 3) * radius, (float)Math.Cos(timer + 2 * Math.PI / 3) * dist * 8f);
                this.chara.Position = center + new Vector2((float)Math.Sin(timer + Math.PI) * radius, (float)Math.Cos(timer + Math.PI) * dist * 8f);
                timer -= Engine.DeltaTime * 2f;
                if (timer <= 0f)
                {
                    timer += 6.2831855f;
                }
                yield return null;
            }
            yield break;
        }

        public override void OnEnd(Level level)
        {
            if (this.badeline != null)
            {
                this.badeline.RemoveSelf();
            }
            if (this.ralsei != null)
            {
                this.ralsei.RemoveSelf();
            }
            if (this.chara != null)
            {
                this.chara.RemoveSelf();
            }
            if (this.player != null)
            {
                this.player.Dashes = 5;
                this.player.Position = this.origin;
            }
            if (!this.dark)
            {
                level.Add(new HeightDisplayMod(this.index));
            }
        }
    }
}
