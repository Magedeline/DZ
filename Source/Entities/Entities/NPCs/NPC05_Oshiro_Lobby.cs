using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using ModOshiroSprite = DZ.MaggyOshiroSprite;

namespace Celeste.NPCs;

[Tracked(true)]
[CustomEntity(ids: "DZ/NPC05_Oshiro_Lobby")]
public class NPC05_Oshiro_Lobby : NPC
{
    public static ParticleType P_AppearSpark;

    private float startX;
    protected new TalkComponent Talker;
    private bool isInteracting;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC05_Oshiro_Lobby(Vector2 position)
        : base(position)
    {
        Add(Sprite = new ModOshiroSprite(-1));
        Sprite.Visible = false;
        MTexture texture = GFX.Gui["hover/resort"];
        if (GFX.Gui.Has("hover/resort_" + global::Celeste.Settings.Instance.Language))
        {
            texture = GFX.Gui["hover/resort_" + global::Celeste.Settings.Instance.Language];
        }
        Add(Talker = new TalkComponent(new Rectangle(-30, -16, 42, 32), new Vector2(-12f, -24f), OnTalk, new TalkComponent.HoverDisplay
        {
            Texture = texture,
            InputPosition = new Vector2(0f, -75f),
            SfxIn = "event:/ui/game/hotspot_note_in",
            SfxOut = "event:/ui/game/hotspot_note_out"
        }));
        Talker.PlayerMustBeFacing = false;
        MoveAnim = "move";
        IdleAnim = "idle";
        base.Depth = 9001;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (base.Session.GetFlag("oshiro_resort_talked_1"))
        {
            base.Session.Audio.Music.Event = "event:/pusheen/music/lvl5/explore";
            base.Session.Audio.Music.Progress = 1;
            base.Session.Audio.Apply(forceSixteenthNoteHack: false);
            RemoveSelf();
        }
        else
        {
            base.Session.Audio.Music.Event = null;
            base.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        scene.Add(new OshiroLobbyBell(new Vector2(base.X - 14f, base.Y)));
        startX = Position.X;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnTalk(CelestePlayer player)
    {
        if (isInteracting) return;
        isInteracting = true;
        base.Scene.Add(new CS05_OshiroLobby(player, this));
        Talker.Enabled = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (base.X >= startX + 12f)
        {
            base.Depth = 1000;
        }
        if (isInteracting && Talker.Enabled)
        {
            isInteracting = false;
        }
    }
}





