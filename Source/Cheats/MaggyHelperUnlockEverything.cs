using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Cheat listener that unlocks all content when the Konami-style code is entered.
/// Code: lrLRuudlRA (left, right, L, R, up, up, down, left, R, A)
/// For returning players who have played the mod before and want to skip progression.
/// </summary>
public class DZUnlockEverything : CheatListener
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public DZUnlockEverything()
    {
        // MIDI-derived input pattern: C7=l, D7=r, E7=d, F7=r, G7=u, high=L/R/A
        // Sequence: l,u,u,l,l,u,r,d,r,r,Z,X,C
        AddInput('l', [MethodImpl(MethodImplOptions.NoInlining)] () => Input.MenuLeft.Pressed && !Input.MenuLeft.Repeating);
        AddInput('r', [MethodImpl(MethodImplOptions.NoInlining)] () => Input.MenuRight.Pressed && !Input.MenuRight.Repeating);
        AddInput('u', [MethodImpl(MethodImplOptions.NoInlining)] () => Input.MenuUp.Pressed && !Input.MenuUp.Repeating);
        AddInput('d', [MethodImpl(MethodImplOptions.NoInlining)] () => Input.MenuDown.Pressed && !Input.MenuDown.Repeating);
        AddInput('Z', () => Input.Dash.Pressed);
        AddInput('X', [MethodImpl(MethodImplOptions.NoInlining)] () => Input.Grab.Pressed);
        AddInput('C', () => Input.Jump.Pressed);
        AddCheat("luulurdrrZXC", EnteredCheat);
        Logging = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnteredCheat()
    {
        Level level = SceneAs<Level>();
        level.PauseLock = true;
        level.Frozen = true;
        level.Flash(Color.White);
        Audio.Play("event:/pusheen/new_content/game/general/cheat_activate", level.Camera.Position + new Vector2(160f, 90f));
        new FadeWipe(level, wipeIn: false, delegate
        {
            UnlockEverything(level);
        }).Duration = 2f;
        RemoveSelf();
    }

    // Room ID for automatic unlock trigger in the DZ prologue
    private const string TriggerRoomID = "z1";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (scene is not Level level)
            return;

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        // Skip if already triggered (HasSeenModIntro is set synchronously before EnteredCheat fires,
        // so any re-added entity sees the flag and exits — no re-trigger loop).
        if (save == null || save.HasSeenModIntro)
            return;

        // Check we're in a DZ map (SID prefix "Maggy/") in the trigger room
        var area = AreaData.Get(level.Session.Area);
        bool isOurMap = area?.SID?.StartsWith(
            AreaModeExtender.MAP_PREFIX + "/", StringComparison.OrdinalIgnoreCase) == true;

        if (isOurMap && level.Session.Level == TriggerRoomID)
        {
            save.HasSeenModIntro = true; // Set now so any re-added entity won't re-trigger
            EnteredCheat();
        }
    }

    public void UnlockEverything(Level level)
    {
        // Use the KIRBY_CELESTE module to unlock everything
        global::Celeste.Mod.DZ.DZModule.TriggerUnlockEverythingCheat();

        // Also unlock vanilla content if in Celeste level set
        SaveData saveData = SaveData.Instance;
        if (saveData.LevelSet == AreaModeExtender.MAP_PREFIX)
        {
            foreach (LevelSetStats levelSet in saveData.LevelSets)
            {
                levelSet.UnlockedAreas = levelSet.MaxArea;
            }
            SaveData.Instance.RevealedChapter9 = true;
            Settings.Instance.VariantsUnlocked = true;
            Settings.Instance.Pico8OnMainMenu = true;
        }
        else
        {
            saveData.LevelSetStats.UnlockedAreas = saveData.LevelSetStats.MaxArea;
        }

        saveData.CheatMode = true;
        level.Session.InArea = false;
        Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session);
    }
}
