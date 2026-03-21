using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/FinalTitanSummitBackgroundManager")]
[Tracked(true)]
[HotReloadable]
public class FinalTitanSummitBackgroundManager : Entity
{
    private const string BeginSwapFlag = "finaltitansummit_beginswap_";
    private const string BgSwapFlag = "finaltitansummit_bgswap_";

    private static readonly string[] ActorCycle =
    {
        "asriel",
        "badeline",
        "seven_souls",
        "bird",
        "seven_goner_birds",
        "madeline",
        "player"
    };

    private readonly bool dark;
    private readonly bool introLaunch;
    private readonly int index;
    private readonly string cutscene;
    private readonly string ambience;

    private Level level;
    private Player player;
    private Vector2 origin;
    private BadelineDummy badeline;
    private RalseiDummy ralsei;
    private CharaDummy chara;
    private float fade;
    private bool outTheTop;
    private bool spinning;
    private Color background;

    public FinalTitanSummitBackgroundManager(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Tag = (int)Tags.TransitionUpdate;
        Depth = 8900;

        index = data.Int(nameof(index));
        cutscene = data.Attr(nameof(cutscene));
        introLaunch = data.Bool("intro_launch");
        dark = data.Bool("dark");
        ambience = data.Attr(nameof(ambience));
        background = dark ? Color.Black : Calc.HexToColor("1f2f4d");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = Scene as Level;
        Add(new Coroutine(Routine()));
    }

    private IEnumerator Routine()
    {
        player = Scene.Tracker.GetEntity<Player>();
        while (player == null || player.Y > Y)
        {
            player = Scene.Tracker.GetEntity<Player>();
            yield return null;
        }

        origin = player.Position;

        level.Session.SetFlag(BeginSwapFlag + index);
        level.Session.SetFlag($"finaltitansummit_actor_{ActorCycle[Math.Abs(index) % ActorCycle.Length]}");

        if (!string.IsNullOrWhiteSpace(ambience))
        {
            if (ambience.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                Audio.SetAmbience(null);
            else
                Audio.SetAmbience(SFX.EventnameByHandle(ambience));
        }

        if (introLaunch)
            yield return FadeTo(1f, dark ? 1.2f : 0.8f);
        else
            yield return FadeTo(1f, dark ? 0.8f : 0.5f);

        yield return RunAscendCutscene();

        if (!dark)
            level.Add(new global::MaggyHelper.HeightDisplayMod(index));

        player.Sprite.Play("launch");
        Audio.Play("event:/char/madeline/summit_flytonext", player.Position);

        level.Session.SetFlag(BgSwapFlag + index);
        level.NextTransitionDuration = 0.05f;
        outTheTop = true;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        if (level == null)
            return;

        Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, background * fade);
    }

    public override void Removed(Scene scene)
    {
        FadeSnapTo(0f);
        spinning = false;
        CleanupDummies();
        level?.Session.SetFlag(BgSwapFlag + index, false);
        level?.Session.SetFlag(BeginSwapFlag + index, false);
        level?.Session.SetFlag($"finaltitansummit_actor_{ActorCycle[Math.Abs(index) % ActorCycle.Length]}", false);

        if (outTheTop)
            ScreenWipe.WipeColor = dark ? Color.Black : Color.White;

        ScreenWipe.WipeColor = Color.Black;
        base.Removed(scene);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        while ((fade = Calc.Approach(fade, target, Engine.DeltaTime / duration)) != target)
        {
            FadeSnapTo(fade);
            yield return null;
        }

        FadeSnapTo(target);
    }

    private IEnumerator RunAscendCutscene()
    {
        Audio.Play("event:/char/badeline/maddy_split", player.Position);
        player.CreateSplitParticles();
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
        player.Dashes = 5;
        player.Facing = Facings.Right;

        Scene.Add(badeline = new BadelineDummy(player.Position));
        Scene.Add(ralsei = new RalseiDummy(player.Position));
        Scene.Add(chara = new CharaDummy(player.Position));
        badeline.AutoAnimator.Enabled = true;

        spinning = true;
        Add(new Coroutine(SpinCharacters()));

        if (!string.IsNullOrWhiteSpace(cutscene))
            yield return Textbox.Say(cutscene);
        else if (index >= 0 && index <= 12)
            yield return Textbox.Say($"CH20_ASCEND_VS_ELS_{index}");

        Audio.Play("event:/char/badeline/maddy_join", player.Position);
        spinning = false;
        yield return 0.25f;

        CleanupDummies();
        player.Position = origin;
        player.Dashes = 5;
        player.CreateSplitParticles();
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f, null, null);
    }

    private IEnumerator SpinCharacters()
    {
        float dist = 0f;
        Vector2 center = player.Position;
        float timer = 1.5707964f;

        player.Sprite.Play("spin");
        badeline.Sprite.Play("spin");
        ralsei.Sprite.Play("spin");
        chara.Sprite?.Play("spin");
        badeline.Sprite.Scale.X = 1f;
        ralsei.Sprite.Scale.X = 1.5f;
        if (chara.Sprite != null) chara.Sprite.Scale.X = 2f;

        while (spinning || dist > 0f)
        {
            dist = Calc.Approach(dist, spinning ? 2f : 0f, Engine.DeltaTime * 4f);
            int frame = (int)(timer / 6.2831855f * 14f + 10f);
            float sin = (float)Math.Sin(timer);
            float cos = (float)Math.Cos(timer);
            float radius = Ease.CubeOut(dist) * 32f;

            player.Sprite.SetAnimationFrame(frame);
            badeline.Sprite.SetAnimationFrame(frame + 7);
            ralsei.Sprite.SetAnimationFrame(frame + 7);
            chara.Sprite?.SetAnimationFrame(frame + 7);

            player.Position = center + new Vector2(sin * radius, cos * dist * 8f);
            badeline.Position = center + new Vector2((float)Math.Sin(timer + Math.PI / 3) * radius, (float)Math.Cos(timer + Math.PI / 3) * dist * 8f);
            ralsei.Position = center + new Vector2((float)Math.Sin(timer + 2 * Math.PI / 3) * radius, (float)Math.Cos(timer + 2 * Math.PI / 3) * dist * 8f);
            chara.Position = center + new Vector2((float)Math.Sin(timer + Math.PI) * radius, (float)Math.Cos(timer + Math.PI) * dist * 8f);

            timer -= Engine.DeltaTime * 2f;
            if (timer <= 0f)
                timer += 6.2831855f;

            yield return null;
        }
    }

    private void FadeSnapTo(float target)
    {
        fade = target;
        if (level != null)
            level.Bloom.Base = AreaData.Get(level).BloomBase + fade * 0.1f;
    }

    private void CleanupDummies()
    {
        badeline?.RemoveSelf();
        ralsei?.RemoveSelf();
        chara?.RemoveSelf();
        badeline = null;
        ralsei = null;
        chara = null;
    }
}
