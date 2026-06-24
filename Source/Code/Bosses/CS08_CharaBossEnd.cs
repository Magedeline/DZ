#nullable enable
using Celeste.Entities;
using Celeste.NPCs;
using FMOD.Studio;

namespace Celeste.Cutscenes;

// Token: 0x02000196 RID: 406
[HotReloadable]
public class Cs08CharaBossEnd : CutsceneEntity
{
    // Token: 0x04000959 RID: 2393
    public const string Flag = "chara_connection";

    // Token: 0x0400095A RID: 2394
    private readonly global::Celeste.Player player;

    // Token: 0x0400095B RID: 2395
    private readonly Npc08CharaCrying chara;

    // Token: 0x0400095C RID: 2396
    private float fade;

    // Token: 0x0400095D RID: 2397
    private float pictureFade;

    // Token: 0x0400095E RID: 2398
    private float pictureGlow;

    // Token: 0x0400095F RID: 2399
    private MTexture? picture;

    // Token: 0x04000960 RID: 2400
    private bool waitForKeyPress;

    // Token: 0x04000961 RID: 2401
    private float timer;

    // Token: 0x04000962 RID: 2402
    private EventInstance sfx;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public Cs08CharaBossEnd(global::Celeste.Player player, Npc08CharaCrying chara) : base(true, false)
#pragma warning restore CS8618
    {
        base.Tag = Tags.HUD;
        this.player = player;
        this.chara = chara;
    }

    public override void OnBegin(Level level)
    {
        base.Add(new Coroutine(this.Cutscene(level), true));
    }

    private IEnumerator Cutscene(Level level)
    {
        this.player.StateMachine.State = Player.StDummy;
        this.player.StateMachine.Locked = true;
        while (!this.player.OnGround(1))
        {
            yield return null;
        }
        this.player.Facing = Facings.Right;
        yield return 1f;
        Level level2 = base.SceneAs<Level>();
        level2.Session.Audio.Music.Event = "event:/music/pusheen/lvl8/chara_acoustic";
        level2.Session.Audio.Apply(false);
        yield return Textbox.Say("MAGGYHELPER_CH8_CHARA_BOSS_ENDING",
        [
            this.StartMusic,
            this.PlayerHug,
            this.CharaCalmDown
        ]);
        yield return 0.5f;
        while ((this.fade += Engine.DeltaTime) < 1f)
        {
            yield return null;
        }
        this.picture = GFX.Portraits["hug1"];
        this.sfx = Audio.Play("event:/game/06_reflection/hug_image_1");
        yield return this.PictureFade(1f, 1f);
        yield return this.WaitForPress();
        this.sfx = Audio.Play("event:/game/06_reflection/hug_image_2");
        yield return this.PictureFade(0f, 0.5f);
        this.picture = GFX.Portraits["hug2"];
        yield return this.PictureFade(1f, 1f);
        yield return this.WaitForPress();
        this.sfx = Audio.Play("event:/game/06_reflection/hug_image_3");
        while ((this.pictureGlow += Engine.DeltaTime / 2f) < 1f)
        {
            yield return null;
        }
        yield return 0.2f;
        yield return this.PictureFade(0f, 0.5f);
        while ((this.fade -= Engine.DeltaTime * 12f) > 0f)
        {
            yield return null;
        }
        level.Session.Audio.Music.Param("levelup", 1f);
        level.Session.Audio.Apply(false);
        base.Add(new Coroutine(this.chara.TurnWhite(1f), true));
        yield return 0.5f;
        this.player.Sprite.Play("idle", false, false);
        yield return 0.25f;
        yield return this.player.DummyWalkToExact((int)this.player.X - 8, true, 1f, false);
        base.Add(new Coroutine(this.CenterCameraOnPlayer(), true));
        yield return this.chara.Disperse();
        (base.Scene as Level)?.Session.SetFlag("chara_connection", true);
        level.Flash(Color.White, false);
        level.Session.Inventory.Dashes = 5;
        this.chara.RemoveSelf();
        yield return 0.1f;
        level.Add(new LevelUpEffect(this.player.Position));
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        yield return 2f;
        yield return level.ZoomBack(0.5f);
        base.EndCutscene(level, true);
        yield break;
    }

    private IEnumerator StartMusic()
    {
        Level level = base.SceneAs<Level>();
        level.Session.Audio.Music.Event = "event:/music/pusheen/lvl8/chara_acoustic";
        level.Session.Audio.Apply(false);
        yield return 0.5f;
        yield break;
    }

    private IEnumerator PlayerHug()
    {
        base.Add(new Coroutine(this.Level.ZoomTo(this.chara.Center + new Vector2(0f, -24f) - this.Level.Camera.Position, 2f, 0.5f), true));
        yield return 0.6f;
        yield return this.player.DummyWalkToExact((int)this.chara.X - 10, false, 1f, false);
        this.player.Facing = Facings.Right;
        yield return 0.25f;
        this.player.DummyAutoAnimate = true;
        this.player.Sprite.Play("hug", false, false);
        yield return 0.5f;
        yield break;
    }

    private IEnumerator CharaCalmDown()
    {
        Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "charapostboss", 0f);
        yield return 0.5f;
        this.chara.Sprite.Play("scaredTransition");
        yield return 0.5f;
        Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
        CharaBossStarfield charabossBg = this.Level.Background.Get<CharaBossStarfield>();
        if (charabossBg != null)
        {
            while (charabossBg.Alpha > 0f)
            {
                charabossBg.Alpha -= Engine.DeltaTime;
                yield return null;
            }
        }
        yield break;
    }

    private IEnumerator CenterCameraOnPlayer()
    {
        yield return 0.5f;
        Vector2 from = this.Level.ZoomFocusPoint;
        Vector2 to = new Vector2((float)(this.Level.Bounds.Left + 580), (float)(this.Level.Bounds.Top + 124)) - this.Level.Camera.Position;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            this.Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
            yield return null;
        }
        yield break;
    }

    private IEnumerator PictureFade(float to, float duration = 1f)
    {
        while ((this.pictureFade = Calc.Approach(this.pictureFade, to, Engine.DeltaTime / duration)) != to)
        {
            yield return null;
        }
        yield break;
    }

    private IEnumerator WaitForPress()
    {
        this.waitForKeyPress = true;
        while (!Input.MenuConfirm.Pressed)
        {
            yield return null;
        }
        this.waitForKeyPress = false;
        yield break;
    }

    public override void OnEnd(Level level)
    {
        if (this.WasSkipped && this.sfx != null)
        {
            Audio.Stop(this.sfx, true);
        }
        Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "charapostboss", 0f);
        level.ResetZoom();
        level.Session.Inventory.Dashes = 5;
        level.Session.Audio.Music.Event = "event:/music/pusheen/lvl8/chara_acoustic";
        if (this.WasSkipped)
        {
            level.Session.Audio.Music.Param("levelup", 2f);
        }
        level.Session.Audio.Apply(false);
        if (this.WasSkipped)
        level.Session.SetFlag("chara_connection", true);
    }

    public override void Update()
    {
        this.timer += Engine.DeltaTime;
        base.Update();
    }

    public override void Render()
    {
        if (this.fade > 0f)
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(this.fade) * 0.8f);
            if (this.picture != null && this.pictureFade > 0f)
            {
                float num = Ease.CubeOut(this.pictureFade);
                Vector2 position = new(960f, 540f);
                float scale = 1f + (1f - num) * 0.025f;
                this.picture.DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade), scale, 0f);
                if (this.pictureGlow > 0f)
                {
                    GFX.Portraits["hug-light2a"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    GFX.Portraits["hug-light2b"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    GFX.Portraits["hug-light2c"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    HiresRenderer.EndRender();
                    HiresRenderer.BeginRender(BlendState.Additive, null);
                    GFX.Portraits["hug-light2a"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    GFX.Portraits["hug-light2b"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    GFX.Portraits["hug-light2c"].DrawCentered(position, Color.White * Ease.CubeOut(this.pictureFade * this.pictureGlow), scale);
                    HiresRenderer.EndRender();
                    HiresRenderer.BeginRender(null, null);
                }
                if (this.waitForKeyPress)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1520f, (float)(880 + ((this.timer % 1f < 0.25f) ? 6 : 0))));
                }
            }
        }
    }
}




