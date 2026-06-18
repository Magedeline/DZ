using Celeste.Entities;
using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    // Token: 0x0200016B RID: 363
    public class CS19_CharaHelps : CutsceneEntity
    {
        // Token: 0x06000CF2 RID: 3314 RVA: 0x0002BE94 File Offset: 0x0002A094
        public CS19_CharaHelps(global::Celeste.Player player) : base(true, false)
        {
            base.Depth = -8500;
            this.player = player;
        }

        // Token: 0x06000CF3 RID: 3315 RVA: 0x0002BEB0 File Offset: 0x0002A0B0
        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(level), true));
        }

        // Token: 0x06000CF4 RID: 3316 RVA: 0x0002BEC5 File Offset: 0x0002A0C5
        private IEnumerator Cutscene(Level level)
        {
            Vector2 spawn = level.GetSpawnPoint(this.player.Position);
            this.player.Dashes = 2;
            this.player.StateMachine.State = Player.StDummy;
            this.player.DummyGravity = false;
            this.entrySfx = Audio.Play("event:/new_content/char/madeline/screenentry_stubborn", this.player.Position);
            yield return this.player.MoonLanding(spawn);
            yield return level.ZoomTo(new Vector2(spawn.X - level.Camera.X, 134f), 2f, 0.5f);
            yield return 1f;
            yield return this.CharaAppears();
            yield return 0.3f;
            yield return Textbox.Say("DZ_CH19_HELPING_HAND", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(this.KirbyFacesAway),
                new Func<IEnumerator>(this.HeartBreak),
                new Func<IEnumerator>(this.KirbyFacesChara),
                new Func<IEnumerator>(this.KirbyStepsForwards)
            });
            if (this.chara != null)
            {
                yield return this.CharaVanishes();
            }
            yield return level.ZoomBack(0.5f);
            base.EndCutscene(level, true);
            yield break;
        }

        // Token: 0x06000CF5 RID: 3317 RVA: 0x0002BEDB File Offset: 0x0002A0DB
        private IEnumerator CharaAppears()
        {
            this.StartMusic();
            Audio.Play("event:/char/badeline/maddy_split", this.player.Position);
            this.Level.Add(this.chara = new CharaDummy(this.player.Center));
            this.Level.Displacement.AddBurst(this.chara.Center, 0.5f, 8f, 32f, 0.5f, null, null);
            this.player.Dashes = 1;
            this.chara.Sprite.Scale.X = -1f;
            yield return this.chara.FloatTo(this.player.Center + new Vector2(18f, -10f), new int?(-1), false, false, false);
            yield return 0.2f;
            this.player.Facing = Facings.Right;
            yield return null;
            yield break;
        }

        // Token: 0x06000CF6 RID: 3318 RVA: 0x0002BEEA File Offset: 0x0002A0EA
        private IEnumerator KirbyFacesAway()
        {
            this.Level.NextColorGrade("feelingdown", 0.1f);
            yield return this.player.DummyWalkTo(this.player.X - 16f, false, 1f, false);
            yield break;
        }

        private IEnumerator HeartBreak()
        {
            yield return 0.2f;
            Audio.Play("event:/Mods/pusheen/new_content/char/kirby/heartbreak", this.player.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 0.2f;
            yield break;
        }

        // Token: 0x06000CF7 RID: 3319 RVA: 0x0002BEF9 File Offset: 0x0002A0F9
        private IEnumerator KirbyFacesChara()
        {
            this.player.Facing = Facings.Right;
            yield return 0.2f;
            yield break;
        }

        // Token: 0x06000CF8 RID: 3320 RVA: 0x0002BF08 File Offset: 0x0002A108
        private IEnumerator KirbyStepsForwards()
        {
            Vector2 spawnPoint = this.Level.GetSpawnPoint(this.player.Position);
            base.Add(new Coroutine(this.player.DummyWalkToExact((int)spawnPoint.X, false, 1f, false), true));
            yield return 0.1f;
            yield return this.chara.FloatTo(this.chara.Position + new Vector2(20f, 0f), null, false, false, false);
            yield break;
        }

        // Token: 0x06000CF9 RID: 3321 RVA: 0x0002BF17 File Offset: 0x0002A117
        private IEnumerator CharaVanishes()
        {
            yield return 0.2f;
            this.chara.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            this.chara = null;
            yield return 0.2f;
            yield break;
        }

        // Token: 0x06000CFA RID: 3322 RVA: 0x0002BF28 File Offset: 0x0002A128
        private void StartMusic()
        {
            if (this.Level.Session.Audio.Music.Event != "event:/Mods/pusheen/new_content/music/lvl19/tape_rooms")
            {
                int num = 0;
                TapeBlockManager entity = this.Level.Tracker.GetEntity<TapeBlockManager>();
                if (entity != null)
                {
                    num = entity.GetSixteenthNote();
                }
                this.Level.Session.Audio.Music.Event = "event:/Mods/pusheen/new_content/music/lvl19/tape_rooms";
                this.Level.Session.Audio.Music.Param("sixteenth_note", (float)num);
                this.Level.Session.Audio.Apply(true);
                this.Level.Session.Audio.Music.Param("sixteenth_note", 7f);
            }
        }

        // Token: 0x06000CFB RID: 3323 RVA: 0x0002BFF8 File Offset: 0x0002A1F8
        public override void OnEnd(Level level)
        {
            this.Level.Session.Inventory.Dashes = 1;
            this.player.Dashes = 1;
            this.player.Depth = 0;
            this.player.Dashes = 1;
            this.player.Speed = Vector2.Zero;
            this.player.Position = level.GetSpawnPoint(this.player.Position);
            this.player.Position -= Vector2.UnitY * 12f;
            this.player.MoveVExact(100, null, null);
            this.player.Active = true;
            this.player.Visible = true;
            this.player.StateMachine.State = Player.StNormal;
            if (this.chara != null)
            {
                this.chara.RemoveSelf();
            }
            level.ResetZoom();
            level.Session.SetFlag("chara_helps", true);
            if (this.WasSkipped)
            {
                Audio.Stop(this.entrySfx, true);
                this.StartMusic();
                level.SnapColorGrade("feelingdown");
            }
        }

        // Token: 0x04000844 RID: 2116
        public const string Flag = "chara_helps";

        // Token: 0x04000845 RID: 2117
        private global::Celeste.Player player;

        // Token: 0x04000846 RID: 2118
        private CharaDummy chara;

        // Token: 0x04000847 RID: 2119
        private EventInstance entrySfx;
    }
}

