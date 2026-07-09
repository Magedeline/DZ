using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

public class CS02DZ_CHaraIntro : CutsceneEntity
{
    public const string Flag = "evilDZ_CHara_intro";

    private Player player;

    private CharaChaser chara;

    private Vector2 charaEndPosition = new Vector2(84f, 135f);

    private float anxietyFade;

    private float anxietyFadeTarget;

    private SineWave anxietySine;

    private float anxietyJitter;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS02DZ_CHaraIntro(CharaChaser chara)
    {
        this.chara = chara;
        charaEndPosition = chara.Position + new Vector2(8f, -24f);
        Add(anxietySine = new SineWave(0.3f, 0f));
        Distort.AnxietyOrigin = new Vector2(0.5f, 0.75f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        anxietyFade = Calc.Approach(anxietyFade, anxietyFadeTarget, 2.5f * Engine.DeltaTime);
        if (base.Scene.OnInterval(0.1f))
        {
            anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
        }
        Distort.Anxiety = anxietyFade * Math.Max(0f, 0f + anxietyJitter + anxietySine.Value * 0.3f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]

    private IEnumerator Cutscene(Level level)
    {
        anxietyFadeTarget = 1f;
        while (true)
        {
            player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                break;
            }
            yield return null;
        }
        while (!player.OnGround())
        {
            yield return null;
        }
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;
        yield return 1f;
        if (level.Session.Area.Mode == AreaMode.Normal)
        {
            Audio.SetMusic("event:/DZ/music/lvl2/evil_madeline");
        }
        yield return Textbox.Say("DZ_CH2_CHARA_INTRO", TurnAround, RevealChara, StartLaughing, StopLaughing);
        anxietyFadeTarget = 0f;
        yield return Level.ZoomBack(0.5f);
        EndCutscene(level);
    }

    private IEnumerator TurnAround()
    {
        player.Facing = Facings.Left;
        yield return 0.2f;
        Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(Level.Bounds.X, Level.Camera.Y), 0.5f)));
        yield return Level.ZoomTo(new Vector2(84f, 135f), 2f, 0.5f);
        yield return 0.2f;
    }

    private IEnumerator RevealChara()
    {
        Audio.Play("event:/DZ/game/02_old_site/sequenceDZ_CHara_intro", chara.Position);
        yield return 0.1f;
        Level.Displacement.AddBurst(chara.Position + new Vector2(0f, -4f), 0.8f, 8f, 48f, 0.5f);
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        yield return 0.1f;
        chara.Hovering = true;
        if (chara.Hair != null) chara.Hair.Visible = true;
        chara.Sprite.Play("fallSlow");
        Vector2 from = chara.Position;
        Vector2 to = charaEndPosition;
        for (float t = 0f; t < 1f; t += Engine.DeltaTime)
        {
            chara.Position = from + (to - from) * Ease.CubeInOut(t);
            yield return null;
        }
        player.Facing = (Facings)Math.Sign(chara.X - player.X);
        yield return 1f;
    }

    private IEnumerator StartLaughing()
    {
        yield return 0.2f;
        chara.Sprite.Play("laugh", restart: true);
        yield return null;
    }

    private IEnumerator StopLaughing()
    {
        chara.Sprite.Play("fallSlow", restart: true);
        yield return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        Audio.SetMusic(null);
        Distort.Anxiety = 0f;
        if (player != null)
        {
            player.StateMachine.Locked = false;
            player.Facing = Facings.Left;
            player.StateMachine.State = 0;
            player.JustRespawned = true;
        }
        chara.Position = charaEndPosition;
        chara.Visible = true;
        chara.Sprite.Play("fallSlow");
        chara.Hovering = false;
        chara.Add(new Coroutine(chara.StartChasingRoutine(level)));
        level.Session.SetFlag("evilDZ_CHara_intro");
    }
}
