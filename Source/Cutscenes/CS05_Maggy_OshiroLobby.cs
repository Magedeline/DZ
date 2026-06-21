using System.Runtime.CompilerServices;
using Celeste.NPCs;
using Celeste.Entities;
using Monocle;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes;

public class CS05_Maggy_OshiroLobby : CutsceneEntity
{
    public const string Flag = "maggy_oshiro_resort_talked_1";

    private CelestePlayer player;

    private NPC oshiro;

    private float startLightAlpha;

    private bool createSparks;

    private SoundSource sfx = new SoundSource();

    private NPC ralsei;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS05_Maggy_OshiroLobby(CelestePlayer player, NPC oshiro)
    {
        this.player = player;
        this.oshiro = oshiro;
        Add(sfx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (createSparks && Level.OnInterval(0.025f))
        {
            Vector2 vector = oshiro.Position + new Vector2(0f, -12f) + new Vector2(Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1), Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1));
            Level.Particles.Emit(NPC05_Maggy_Oshiro_Lobby.P_AppearSpark, vector, (vector - oshiro.Position).Angle());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        startLightAlpha = level.Lighting.Alpha;
        float endLightAlpha = 1f;
        float from = oshiro.Y;
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        yield return 0.5f;
        yield return player.DummyWalkTo(oshiro.X - 16f);
        player.Facing = Facings.Right;
        sfx.Play("event:/game/03_resort/sequence_oshiro_intro");
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        yield return 1.4f;
        level.Shake();
        level.Lighting.Alpha += 0.5f;
        while (level.Lighting.Alpha > startLightAlpha)
        {
            level.Lighting.Alpha -= Engine.DeltaTime * 4f;
            yield return null;
        }
        VertexLight light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 32, 64);
        BloomPoint bloom = new BloomPoint(new Vector2(0f, -8f), 1f, 16f);
        level.Lighting.SetSpotlight(light);
        oshiro.Add(light);
        oshiro.Add(bloom);
        oshiro.Y -= 16f;
        Vector2 target = light.Position;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            light.Alpha = (bloom.Alpha = t.Percent);
            light.Position = Vector2.Lerp(target - Vector2.UnitY * 48f, target, t.Percent);
            level.Lighting.Alpha = MathHelper.Lerp(startLightAlpha, endLightAlpha, t.Eased);
        };
        Add(tween);
        yield return tween.Wait();
        yield return 0.2f;
        yield return level.ZoomTo(new Vector2(170f, 126f), 2f, 0.5f);
        yield return 0.6f;
        level.Shake();
        oshiro.Sprite.Visible = true;
        oshiro.Sprite.Play("appear");
        yield return player.DummyWalkToExact((int)(player.X - 12f), walkBackwards: true);
        player.DummyAutoAnimate = false;
        player.Sprite.Play("shaking");
        Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
        yield return 0.6f;
        createSparks = true;
        yield return 0.4f;
        createSparks = false;
        yield return 0.2f;
        level.Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        yield return 1.4f;
        level.Lighting.UnsetSpotlight();
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.5f, start: true);
        tween2.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            level.Lighting.Alpha = MathHelper.Lerp(endLightAlpha, startLightAlpha, t.Percent);
            bloom.Alpha = 1f - t.Percent;
        };
        Add(tween2);
        while (oshiro.Y != from)
        {
            oshiro.Y = Calc.Approach(oshiro.Y, from, Engine.DeltaTime * 40f);
            yield return null;
        }
        yield return tween2.Wait();
        Audio.SetMusic("event:/pusheen/music/lvl5/oshiro_theme");
        player.DummyAutoAnimate = true;
        yield return Textbox.Say("DZ_CH5_OSHIRO_FRONT_DESK", new Func<IEnumerator>(ZoomOut), new Func<IEnumerator>(SpawnRalsei), new Func<IEnumerator>(OpenOshiroDoor));
        foreach (MaggyOshiroDoor item in Scene.Entities.FindAll<MaggyOshiroDoor>())
        {
            item.Open();
        }
        oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, oshiro.Y));
        oshiro.Add(new SoundSource("event:/char/oshiro/move_01_0xa_exit"));
        yield return 1.5f;
        EndCutscene(level);
    }

    private IEnumerator ZoomOut()
    {
        yield return 0.2f;
        yield return Level.ZoomBack(0.5f);
        yield return 0.2f;
    }

    private IEnumerator SpawnRalsei()
    {
        // Spawn Ralsei next to Badeline at trigger 1
        if (ralsei == null)
        {
            ralsei = new NPC(player.Position + new Vector2(32f, 0f));
            Sprite ralseiSprite = GFX.SpriteBank.Create("ralsei");
            ralseiSprite.Play("idle");
            ralsei.Add(ralseiSprite);
            Scene.Add(ralsei);
            
            // Add spawn effect
            Level.Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
        yield break;
    }

    private IEnumerator OpenOshiroDoor()
    {
        // Open the Oshiro door at trigger 2
        foreach (MaggyOshiroDoor door in Scene.Entities.FindAll<MaggyOshiroDoor>())
        {
            door.Open();
        }
        yield break;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        if (WasSkipped)
        {
            foreach (MaggyOshiroDoor item in base.Scene.Entities.FindAll<MaggyOshiroDoor>())
            {
                item.InstantOpen();
            }
        }
        level.Lighting.Alpha = startLightAlpha;
        level.Lighting.UnsetSpotlight();
        level.Session.SetFlag("maggy_oshiro_resort_talked_1");
        level.Session.Audio.Music.Event = "event:/pusheen/music/lvl5/explore";
        level.Session.Audio.Music.Progress = 1;
        level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        if (WasSkipped)
        {
            level.Remove(oshiro);
        }
        
        // Clean up Ralsei if spawned
        if (ralsei != null)
        {
            Level.Remove(ralsei);
        }
    }
}
