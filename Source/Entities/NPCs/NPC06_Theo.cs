using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

[CustomEntity(ids: "MaggyHelper/NPC06_Theo")]
public class NPC06_Theo : NPC
{
    public Hahaha Hahaha;

    private bool cutscene;

    private Coroutine talkRoutine;

    private const string talkedFlagA = "theo_2";

    private const string talkedFlagB = "theo_3";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC06_Theo(Vector2 position)
        : base(position)
    {
        Add(Sprite = GFX.SpriteBank.Create("theo"));
        Sprite.Scale.X = -1f;
        Sprite.Play("idle");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC06_Theo(EntityData data, Vector2 offset)
        : this(data.Position + offset)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
        Hahaha.Enabled = false;
        if (base.Session.GetFlag("theo_1") && !base.Session.GetFlag("theo_2"))
        {
            Sprite.Play("laugh");
        }
        if (!base.Session.GetFlag("theo_3"))
        {
            Add(Talker = new TalkComponent(new Rectangle(-20, -16, 40, 16), new Vector2(0f, -24f), OnTalk));
            if (!base.Session.GetFlag("theo_1"))
            {
                Talker.Enabled = false;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Player entity = Level.Tracker.GetEntity<Player>();
        if (entity != null && !base.Session.GetFlag("theo_1") && !cutscene && entity.X > base.X - 40f)
        {
            cutscene = true;
            base.Scene.Add(new CS06_Stronghold(this, entity));
            if (Talker != null)
            {
                Talker.Enabled = true;
            }
        }
        Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
        base.Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnTalk(Player player)
    {
        Level.StartCutscene(TalkEnd);
        Add(talkRoutine = new Coroutine(TalkRoutine(player)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator TalkRoutine(Player player)
    {
        Sprite.Play("idle");
        player.ForceCameraUpdate = true;
        yield return PlayerApproachLeftSide(player, turnToFace: true, 20f);
        yield return Level.ZoomTo(new Vector2((player.X + X) / 2f - Level.Camera.X, 116f), 2f, 0.5f);
        if (!Session.GetFlag("theo_2"))
        {
            yield return Textbox.Say("CH6_THEO_2");
        }
        else
        {
            yield return Textbox.Say("CH6_THEO_3");
        }
        yield return Level.ZoomBack(0.5f);
        Level.EndCutscene();
        TalkEnd(Level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TalkEnd(Level level)
    {
        if (!base.Session.GetFlag("theo_2"))
        {
            base.Session.SetFlag("theo_2");
        }
        else if (!base.Session.GetFlag("theo_3"))
        {
            base.Session.SetFlag("theo_3");
            Remove(Talker);
        }
        if (talkRoutine != null)
        {
            talkRoutine.RemoveSelf();
            talkRoutine = null;
        }
        Player entity = Level.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
            entity.ForceCameraUpdate = false;
        }
    }
}
