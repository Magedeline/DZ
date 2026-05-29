using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Editor;

public class EnhancedMapEditor : Scene
{
    private enum MouseModes
    {
        Hover,
        Pan,
        Select,
        Move,
        Resize
    }

    private enum PCGMode
    {
        Disabled,
        TerrainGeneration,
        PatternGeneration,
        ImageToMap
    }

    private static Color gridColor;

    private static Camera Camera;

    private static AreaKey area;

    private static float saveFlash;

    private MapData mapData;

    private List<LevelTemplate> levels;

    private Vector2 mousePosition;

    private MouseModes mouseMode;

    private Vector2 lastMouseScreenPosition;

    private Vector2 mouseDragStart;

    private HashSet<LevelTemplate> selection;

    private HashSet<LevelTemplate> hovered;

    private float fade;

    private List<Vector2[]> undoStack;

    private List<Vector2[]> redoStack;

    private const string ManualText = "Right Click:  Teleport to the room\nConfirm:      Teleport to the room\nHold Control: Restart Chapter before teleporting\nHold Shift:   Teleport to the mouse position\nCancel:       Exit debug map\nQ:            Show red berries\nF1:           Show keys\nF2:           Center on current respawn point\nF6:           Exit debug map\nP:            Toggle PCG Mode\nG:            Generate PCG Content";

    private struct KeyInstruction
    {
        public string Key;
        public string Description;
        public string Modifier;

        public KeyInstruction(string key, string description, string modifier = null)
        {
            Key = key;
            Description = description;
            Modifier = modifier;
        }
    }

    private static readonly KeyInstruction[] VisualInstructions = new KeyInstruction[]
    {
        new KeyInstruction("RMB", "Teleport to room"),
        new KeyInstruction("Enter", "Teleport to room"),
        new KeyInstruction("Ctrl", "Restart chapter", "Hold"),
        new KeyInstruction("Shift", "Teleport to mouse", "Hold"),
        new KeyInstruction("ESC", "Exit debug map"),
        new KeyInstruction("Q", "Show red berries"),
        new KeyInstruction("F1", "Show keys"),
        new KeyInstruction("F2", "Center on respawn"),
        new KeyInstruction("F6", "Exit debug map"),
        new KeyInstruction("P", "Toggle PCG Mode"),
        new KeyInstruction("G", "Generate PCG Content")
    };

    private const string MinimalManualText = "F6: Exit debug map";

    private static readonly int ZoomIntervalFrames = 6;

    private Session CurrentSession;

    private int zoomWaitFrames;

    private List<Vector2> keys;

    // PCG State
    private PCGMode pcgMode = PCGMode.Disabled;
    private bool showPCGMenu = false;
    private int pcgMenuSelection = 0;
    private string[] pcgMenuOptions = new string[]
    {
        "Generate Terrain Map",
        "Generate Room from Pattern",
        "Generate from Image",
        "Back to Editor"
    };

    // PCG Parameters
    private int pcgSeed = -1;
    private int pcgDifficulty = 3;
    private int pcgWidthRooms = 4;
    private int pcgHeightRooms = 3;
    private string pcgStrategy = "balanced";
    private string pcgBiomeSet = "";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public EnhancedMapEditor(AreaKey area, bool reloadMapData = true)
    {
        AreaKey areaKey = EnhancedMapEditor.area;
        orig_ctor(area, reloadMapData);
        CurrentSession = (Engine.Scene as Level)?.Session ?? SaveData.Instance?.CurrentSession_Safe;
        if (CurrentSession == null || CurrentSession.Area != area)
        {
            CurrentSession = null;
        }
        else if (areaKey != area)
        {
            CenterViewOnCurrentRespawn();
        }
    }

    public override void GainFocus()
    {
        base.GainFocus();
        SaveAndReload();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SelectAll()
    {
        selection.Clear();
        foreach (LevelTemplate level in levels)
        {
            selection.Add(level);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Rename(string oldName, string newName)
    {
        LevelTemplate levelTemplate = null;
        LevelTemplate levelTemplate2 = null;
        foreach (LevelTemplate level in levels)
        {
            if (levelTemplate == null && level.Name == oldName)
            {
                levelTemplate = level;
                if (levelTemplate2 != null)
                {
                    break;
                }
            }
            else if (levelTemplate2 == null && level.Name == newName)
            {
                levelTemplate2 = level;
                if (levelTemplate != null)
                {
                    break;
                }
            }
        }
        string path = Path.Combine("..", "..", "..", "Content", "Levels", mapData.Filename);
        if (levelTemplate2 == null)
        {
            File.Move(Path.Combine(path, levelTemplate.Name + ".xml"), Path.Combine(path, newName + ".xml"));
            levelTemplate.Name = newName;
        }
        else
        {
            string text = Path.Combine(path, "TEMP.xml");
            File.Move(Path.Combine(path, levelTemplate.Name + ".xml"), text);
            File.Move(Path.Combine(path, levelTemplate2.Name + ".xml"), Path.Combine(path, oldName + ".xml"));
            File.Move(text, Path.Combine(path, newName + ".xml"));
            levelTemplate.Name = newName;
            levelTemplate2.Name = oldName;
        }
        Save();
    }

    private void Save()
    {
    }

    private void SaveAndReload()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UpdateMouse()
    {
        mousePosition = Vector2.Transform(MInput.Mouse.Position, Matrix.Invert(Camera.Matrix));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        MakeMapEditorBetter();
        HandlePCGControls();
        
        if (MInput.Keyboard.Pressed(Keys.F2))
        {
            CenterViewOnCurrentRespawn();
        }
        orig_Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetEditorColor(int index)
    {
        foreach (LevelTemplate item in selection)
        {
            item.EditorColorIndex = index;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        orig_Render();
        RenderKeys();
        RenderHighlightCurrentRoom();
        
        if (showPCGMenu)
        {
            RenderPCGMenu();
        }
        
        // Render our visual manual text on top of everything
        RenderManualText();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LoadLevel(LevelTemplate level, Vector2 at)
    {
        Save();
        KeyboardState state = Keyboard.GetState();
        bool num = state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl);
        bool flag = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
        Session session = ((!num && CurrentSession != null) ? CurrentSession : ((AreaData.GetCheckpoint(area, level.Name) == null) ? new Session(area) : new Session(area, level.Name)
        {
            StartCheckpoint = null
        }));
        session.Level = level.Name;
        session.RespawnPoint = (flag ? new Vector2?(at) : ((Vector2?)null));
        session.FirstLevel = false;
        session.StartedFromBeginning = false;
        if (num && !flag && (session.FirstLevel = level.Name == session.MapData.StartLevel().Name))
        {
            Vector2 vector;
            if (session.Area.GetLevelSet() == "Celeste")
            {
                Rectangle bounds = session.LevelData.Bounds;
                vector = session.GetSpawnPoint(new Vector2(bounds.Left, bounds.Bottom));
            }
            else
            {
                vector = session.LevelData.Spawns[0];
            }
            session.StartedFromBeginning = session.GetSpawnPoint(at) == vector;
        }
        Engine.Scene = new LevelLoader(session, at);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void StoreUndo()
    {
        Vector2[] array = new Vector2[levels.Count];
        for (int i = 0; i < levels.Count; i++)
        {
            array[i] = new Vector2(levels[i].X, levels[i].Y);
        }
        undoStack.Add(array);
        while (undoStack.Count > 30)
        {
            undoStack.RemoveAt(0);
        }
        redoStack.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Undo()
    {
        if (undoStack.Count > 0)
        {
            Vector2[] array = new Vector2[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                array[i] = new Vector2(levels[i].X, levels[i].Y);
            }
            redoStack.Add(array);
            Vector2[] array2 = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            for (int j = 0; j < array2.Length; j++)
            {
                levels[j].X = (int)array2[j].X;
                levels[j].Y = (int)array2[j].Y;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Redo()
    {
        if (redoStack.Count > 0)
        {
            Vector2[] array = new Vector2[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                array[i] = new Vector2(levels[i].X, levels[i].Y);
            }
            undoStack.Add(array);
            Vector2[] array2 = redoStack[undoStack.Count - 1];
            redoStack.RemoveAt(undoStack.Count - 1);
            for (int j = 0; j < array2.Length; j++)
            {
                levels[j].X = (int)array2[j].X;
                levels[j].Y = (int)array2[j].Y;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Rectangle GetMouseRect(Vector2 a, Vector2 b)
    {
        Vector2 vector = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        Vector2 vector2 = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        return new Rectangle((int)vector.X, (int)vector.Y, (int)(vector2.X - vector.X), (int)(vector2.Y - vector.Y));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private LevelTemplate TestCheck(Vector2 point)
    {
        foreach (LevelTemplate level in levels)
        {
            if (!level.Dummy && level.Check(point))
            {
                return level;
            }
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool LevelCheck(Vector2 point)
    {
        foreach (LevelTemplate level in levels)
        {
            if (level.Check(point))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool SelectionCheck(Vector2 point)
    {
        foreach (LevelTemplate item in selection)
        {
            if (item.Check(point))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool SetSelection(Vector2 point)
    {
        selection.Clear();
        foreach (LevelTemplate level in levels)
        {
            if (level.Check(point))
            {
                selection.Add(level);
            }
        }
        return selection.Count > 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool ToggleSelection(Vector2 point)
    {
        bool result = false;
        foreach (LevelTemplate level in levels)
        {
            if (level.Check(point))
            {
                result = true;
                if (selection.Contains(level))
                {
                    selection.Remove(level);
                }
                else
                {
                    selection.Add(level);
                }
            }
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetSelection(Rectangle rect)
    {
        selection.Clear();
        foreach (LevelTemplate level in levels)
        {
            if (level.Check(rect))
            {
                selection.Add(level);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ToggleSelection(Rectangle rect)
    {
        foreach (LevelTemplate level in levels)
        {
            if (level.Check(rect))
            {
                if (selection.Contains(level))
                {
                    selection.Remove(level);
                }
                else
                {
                    selection.Add(level);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static EnhancedMapEditor()
    {
        orig_ctor_EnhancedMapEditor();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void orig_ctor(AreaKey area, bool reloadMapData = true)
    {
        levels = new List<LevelTemplate>();
        selection = new HashSet<LevelTemplate>();
        hovered = new HashSet<LevelTemplate>();
        undoStack = new List<Vector2[]>();
        redoStack = new List<Vector2[]>();
        area.ID = Calc.Clamp(area.ID, 0, AreaData.Areas.Count - 1);
        mapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
        if (reloadMapData)
        {
            mapData.Reload();
        }
        foreach (LevelData level in mapData.Levels)
        {
            levels.Add(new LevelTemplate(level));
        }
        foreach (Rectangle item in mapData.Filler)
        {
            levels.Add(new LevelTemplate(item.X, item.Y, item.Width, item.Height));
        }
        if (area != EnhancedMapEditor.area)
        {
            EnhancedMapEditor.area = area;
            Camera = new Camera();
            Camera.Zoom = 6f;
            Camera.CenterOrigin();
        }
        if (SaveData.Instance == null)
        {
            SaveData.InitializeDebugMode();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void orig_LoadLevel(LevelTemplate level, Vector2 at)
    {
        Save();
        Engine.Scene = new LevelLoader(new Session(area)
        {
            FirstLevel = false,
            Level = level.Name,
            StartedFromBeginning = false
        }, at);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void orig_Update()
    {
        Vector2 vector = default(Vector2);
        vector.X = (lastMouseScreenPosition.X - MInput.Mouse.Position.X) / Camera.Zoom;
        vector.Y = (lastMouseScreenPosition.Y - MInput.Mouse.Position.Y) / Camera.Zoom;
        if (MInput.Keyboard.Pressed(Keys.Space) && MInput.Keyboard.Check(Keys.LeftControl))
        {
            Camera.Zoom = 6f;
            Camera.Position = Vector2.Zero;
        }
        int num = Math.Sign(MInput.Mouse.WheelDelta);
        if ((num > 0 && Camera.Zoom >= 1f) || Camera.Zoom > 1f)
        {
            Camera.Zoom += num;
        }
        else
        {
            Camera.Zoom += (float)num * 0.25f;
        }
        Camera.Zoom = Math.Max(0.25f, Math.Min(24f, Camera.Zoom));
        Camera.Position += new Vector2(Input.MoveX.Value, Input.MoveY.Value) * 300f * Engine.DeltaTime;
        UpdateMouse();
        hovered.Clear();
        if (mouseMode == MouseModes.Hover)
        {
            mouseDragStart = mousePosition;
            if (MInput.Mouse.PressedLeftButton)
            {
                bool flag = LevelCheck(mousePosition);
                if (MInput.Keyboard.Check(Keys.Space))
                {
                    mouseMode = MouseModes.Pan;
                }
                else if (MInput.Keyboard.Check(Keys.LeftControl))
                {
                    if (flag)
                    {
                        ToggleSelection(mousePosition);
                    }
                    else
                    {
                        mouseMode = MouseModes.Select;
                    }
                }
                else if (MInput.Keyboard.Check(Keys.F))
                {
                    levels.Add(new LevelTemplate((int)mousePosition.X, (int)mousePosition.Y, 32, 32));
                }
                else if (flag)
                {
                    if (!SelectionCheck(mousePosition))
                    {
                        SetSelection(mousePosition);
                    }
                    bool flag2 = false;
                    if (selection.Count == 1)
                    {
                        foreach (LevelTemplate item in selection)
                        {
                            if (item.ResizePosition(mousePosition) && item.Type == LevelTemplateType.Filler)
                            {
                                flag2 = true;
                            }
                        }
                    }
                    if (flag2)
                    {
                        foreach (LevelTemplate item2 in selection)
                        {
                            item2.StartResizing();
                        }
                        mouseMode = MouseModes.Resize;
                    }
                    else
                    {
                        StoreUndo();
                        foreach (LevelTemplate item3 in selection)
                        {
                            item3.StartMoving();
                        }
                        mouseMode = MouseModes.Move;
                    }
                }
                else
                {
                    mouseMode = MouseModes.Select;
                }
            }
            else if (MInput.Mouse.PressedRightButton)
            {
                LevelTemplate levelTemplate = TestCheck(mousePosition);
                if (levelTemplate != null)
                {
                    if (levelTemplate.Type == LevelTemplateType.Filler)
                    {
                        if (MInput.Keyboard.Check(Keys.F))
                        {
                            levels.Remove(levelTemplate);
                        }
                    }
                    else
                    {
                        LoadLevel(levelTemplate, mousePosition * 8f);
                    }
                    return;
                }
            }
            else if (MInput.Mouse.PressedMiddleButton)
            {
                mouseMode = MouseModes.Pan;
            }
            else if (!MInput.Keyboard.Check(Keys.Space))
            {
                foreach (LevelTemplate level in levels)
                {
                    if (level.Check(mousePosition))
                    {
                        hovered.Add(level);
                    }
                }
                if (MInput.Keyboard.Check(Keys.LeftControl))
                {
                    if (MInput.Keyboard.Pressed(Keys.Z))
                    {
                        Undo();
                    }
                    else if (MInput.Keyboard.Pressed(Keys.Y))
                    {
                        Redo();
                    }
                    else if (MInput.Keyboard.Pressed(Keys.A))
                    {
                        SelectAll();
                    }
                }
            }
        }
        else if (mouseMode == MouseModes.Pan)
        {
            Camera.Position += vector;
            if (!MInput.Mouse.CheckLeftButton && !MInput.Mouse.CheckMiddleButton)
            {
                mouseMode = MouseModes.Hover;
            }
        }
        else if (mouseMode == MouseModes.Select)
        {
            Rectangle mouseRect = GetMouseRect(mouseDragStart, mousePosition);
            foreach (LevelTemplate level2 in levels)
            {
                if (level2.Check(mouseRect))
                {
                    hovered.Add(level2);
                }
            }
            if (!MInput.Mouse.CheckLeftButton)
            {
                if (MInput.Keyboard.Check(Keys.LeftControl))
                {
                    ToggleSelection(mouseRect);
                }
                else
                {
                    SetSelection(mouseRect);
                }
                mouseMode = MouseModes.Hover;
            }
        }
        else if (mouseMode == MouseModes.Move)
        {
            Vector2 relativeMove = (mousePosition - mouseDragStart).Round();
            bool snap = selection.Count == 1 && !MInput.Keyboard.Check(Keys.LeftAlt);
            foreach (LevelTemplate item4 in selection)
            {
                item4.Move(relativeMove, levels, snap);
            }
            if (!MInput.Mouse.CheckLeftButton)
            {
                mouseMode = MouseModes.Hover;
            }
        }
        else if (mouseMode == MouseModes.Resize)
        {
            Vector2 relativeMove2 = (mousePosition - mouseDragStart).Round();
            foreach (LevelTemplate item5 in selection)
            {
                item5.Resize(relativeMove2);
            }
            if (!MInput.Mouse.CheckLeftButton)
            {
                mouseMode = MouseModes.Hover;
            }
        }
        // Room color selection (24 colors total)
        // D1-D9: colors 0-8
        // D0: color 9
        // Shift + D1-D9: colors 10-18
        // Shift + D0: color 19
        // Ctrl + D1-D5: colors 20-24
        
        bool shiftHeld = MInput.Keyboard.Check(Keys.LeftShift) || MInput.Keyboard.Check(Keys.RightShift);
        bool ctrlHeld = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);
        
        if (ctrlHeld)
        {
            // Colors 20-24 with Ctrl modifier
            if (MInput.Keyboard.Pressed(Keys.D1)) SetEditorColor(20);
            else if (MInput.Keyboard.Pressed(Keys.D2)) SetEditorColor(21);
            else if (MInput.Keyboard.Pressed(Keys.D3)) SetEditorColor(22);
            else if (MInput.Keyboard.Pressed(Keys.D4)) SetEditorColor(23);
            else if (MInput.Keyboard.Pressed(Keys.D5)) SetEditorColor(24);
        }
        else if (shiftHeld)
        {
            // Colors 10-19 with Shift modifier
            if (MInput.Keyboard.Pressed(Keys.D1)) SetEditorColor(10);
            else if (MInput.Keyboard.Pressed(Keys.D2)) SetEditorColor(11);
            else if (MInput.Keyboard.Pressed(Keys.D3)) SetEditorColor(12);
            else if (MInput.Keyboard.Pressed(Keys.D4)) SetEditorColor(13);
            else if (MInput.Keyboard.Pressed(Keys.D5)) SetEditorColor(14);
            else if (MInput.Keyboard.Pressed(Keys.D6)) SetEditorColor(15);
            else if (MInput.Keyboard.Pressed(Keys.D7)) SetEditorColor(16);
            else if (MInput.Keyboard.Pressed(Keys.D8)) SetEditorColor(17);
            else if (MInput.Keyboard.Pressed(Keys.D9)) SetEditorColor(18);
            else if (MInput.Keyboard.Pressed(Keys.D0)) SetEditorColor(19);
        }
        else
        {
            // Colors 0-9 without modifiers
            if (MInput.Keyboard.Pressed(Keys.D1)) SetEditorColor(0);
            else if (MInput.Keyboard.Pressed(Keys.D2)) SetEditorColor(1);
            else if (MInput.Keyboard.Pressed(Keys.D3)) SetEditorColor(2);
            else if (MInput.Keyboard.Pressed(Keys.D4)) SetEditorColor(3);
            else if (MInput.Keyboard.Pressed(Keys.D5)) SetEditorColor(4);
            else if (MInput.Keyboard.Pressed(Keys.D6)) SetEditorColor(5);
            else if (MInput.Keyboard.Pressed(Keys.D7)) SetEditorColor(6);
            else if (MInput.Keyboard.Pressed(Keys.D8)) SetEditorColor(7);
            else if (MInput.Keyboard.Pressed(Keys.D9)) SetEditorColor(8);
            else if (MInput.Keyboard.Pressed(Keys.D0)) SetEditorColor(9);
        }
        if (MInput.Keyboard.Pressed(Keys.F1) || (MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S)))
        {
            SaveAndReload();
            return;
        }
        if (saveFlash > 0f)
        {
            saveFlash -= Engine.DeltaTime * 4f;
        }
        lastMouseScreenPosition = MInput.Mouse.Position;
        base.Update();
    }

    private void MakeMapEditorBetter()
    {
        if ((Input.ESC.Pressed || Input.MenuCancel.Pressed) && CurrentSession != null)
        {
            Input.ESC.ConsumePress();
            Input.MenuCancel.ConsumePress();
            Engine.Scene = new LevelLoader(CurrentSession);
        }
        if (Input.MenuConfirm.Pressed)
        {
            Input.MenuConfirm.ConsumePress();
            LevelTemplate levelTemplate = TestCheck(mousePosition);
            if (levelTemplate != null)
            {
                if (levelTemplate.Type == LevelTemplateType.Filler)
                {
                    return;
                }
                LoadLevel(levelTemplate, mousePosition * 8f);
            }
        }
        if (Camera != null)
        {
            Vector2 vector = new Vector2(Input.MoveX.Value, Input.MoveY.Value) * 300f * Engine.DeltaTime;
            Camera.Position -= vector;
            int num = ((!Input.MoveX.Inverted) ? 1 : (-1));
            int num2 = ((!Input.MoveY.Inverted) ? 1 : (-1));
            Vector2 vector2 = new Vector2(vector.X * (float)num, vector.Y * (float)num2);
            if (Camera.Zoom < 6f)
            {
                Camera.Position += vector2 * (float)Math.Pow(1.3, 6f - Camera.Zoom);
            }
            else
            {
                Camera.Position += vector2;
            }
        }
        GamePadState currentState = MInput.GamePads[Input.Gamepad].CurrentState;
        if (zoomWaitFrames <= 0 && Camera != null)
        {
            float num3 = 0f;
            if (Math.Abs(currentState.ThumbSticks.Right.X) >= 0.5f)
            {
                num3 = Camera.Zoom + (float)Math.Sign(currentState.ThumbSticks.Right.X) * 1f;
            }
            else if (Math.Abs(currentState.ThumbSticks.Right.Y) >= 0.5f)
            {
                num3 = Camera.Zoom + (float)Math.Sign(currentState.ThumbSticks.Right.Y) * 1f;
            }
            if (num3 >= 1f)
            {
                Camera.Zoom = num3;
                zoomWaitFrames = ZoomIntervalFrames;
            }
        }
    }

    private void HandlePCGControls()
    {
        // F6 exits the map editor (disables it)
        if (MInput.Keyboard.Pressed(Keys.F6))
        {
            if (CurrentSession != null)
            {
                Input.ESC.ConsumePress();
                Input.MenuCancel.ConsumePress();
                Engine.Scene = new LevelLoader(CurrentSession);
                return;
            }
        }

        // Toggle PCG menu with P key
        if (MInput.Keyboard.Pressed(Keys.P))
        {
            showPCGMenu = !showPCGMenu;
            pcgMenuSelection = 0;
        }

        if (!showPCGMenu)
        {
            return;
        }

        // Handle menu navigation
        if (MInput.Keyboard.Pressed(Keys.Up))
        {
            pcgMenuSelection = Math.Max(0, pcgMenuSelection - 1);
        }
        else if (MInput.Keyboard.Pressed(Keys.Down))
        {
            pcgMenuSelection = Math.Min(pcgMenuOptions.Length - 1, pcgMenuSelection + 1);
        }
        else if (MInput.Keyboard.Pressed(Keys.Enter) || MInput.Keyboard.Pressed(Keys.Space))
        {
            ExecutePCGMenuSelection();
        }
        else if (MInput.Keyboard.Pressed(Keys.Escape))
        {
            showPCGMenu = false;
        }

        // Adjust parameters with number keys
        if (MInput.Keyboard.Pressed(Keys.D1))
        {
            pcgDifficulty = Math.Max(1, Math.Min(5, pcgDifficulty + 1));
        }
        if (MInput.Keyboard.Pressed(Keys.D2))
        {
            pcgDifficulty = Math.Max(1, Math.Min(5, pcgDifficulty - 1));
        }
        if (MInput.Keyboard.Pressed(Keys.D3))
        {
            pcgWidthRooms = Math.Max(2, Math.Min(10, pcgWidthRooms + 1));
        }
        if (MInput.Keyboard.Pressed(Keys.D4))
        {
            pcgWidthRooms = Math.Max(2, Math.Min(10, pcgWidthRooms - 1));
        }
        if (MInput.Keyboard.Pressed(Keys.D5))
        {
            pcgHeightRooms = Math.Max(2, Math.Min(10, pcgHeightRooms + 1));
        }
        if (MInput.Keyboard.Pressed(Keys.D6))
        {
            pcgHeightRooms = Math.Max(2, Math.Min(10, pcgHeightRooms - 1));
        }
        if (MInput.Keyboard.Pressed(Keys.R))
        {
            pcgSeed = new Random().Next();
        }
    }

    private void ExecutePCGMenuSelection()
    {
        switch (pcgMenuSelection)
        {
            case 0: // Generate Terrain Map
                GenerateTerrainMap();
                break;
            case 1: // Generate Room from Pattern
                GenerateRoomFromPattern();
                break;
            case 2: // Generate from Image
                GenerateFromImage();
                break;
            case 3: // Back to Editor
                showPCGMenu = false;
                break;
        }
    }

    private async void GenerateTerrainMap()
    {
        string outputPath = Path.Combine("Maps", "TerrainGen", $"seed_{pcgSeed}.bin");
        
        bool success = await PCGService.GenerateTerrainMapAsync(
            outputPath,
            pcgSeed,
            pcgDifficulty,
            pcgWidthRooms,
            pcgHeightRooms,
            frequency: 8.0,
            voronoiPoints: 12,
            biomeSet: pcgBiomeSet,
            packageName: "TerrainGen"
        );
        
        if (success)
        {
            // Reload the map data to show the new rooms
            mapData.Reload();
            levels.Clear();
            foreach (LevelData level in mapData.Levels)
            {
                levels.Add(new LevelTemplate(level));
            }
            foreach (Rectangle item in mapData.Filler)
            {
                levels.Add(new LevelTemplate(item.X, item.Y, item.Width, item.Height));
            }
        }
        
        showPCGMenu = false;
    }

    private async void GenerateRoomFromPattern()
    {
        string roomName = $"pcg_room_{DateTime.Now.Ticks}";
        string mapPath = mapData?.Filename ?? "Maps/Temp.bin";
        
        bool success = await PCGService.GenerateRoomFromPatternAsync(
            mapPath,
            roomName,
            width: 320,
            height: 184,
            x: 0,
            y: 0,
            seed: pcgSeed,
            strategy: pcgStrategy,
            modelProfile: "creative",
            libraryPath: "PCG/patterns.json"
        );
        
        if (success)
        {
            // Reload the map data to show the new room
            mapData.Reload();
            levels.Clear();
            foreach (LevelData level in mapData.Levels)
            {
                levels.Add(new LevelTemplate(level));
            }
            foreach (Rectangle item in mapData.Filler)
            {
                levels.Add(new LevelTemplate(item.X, item.Y, item.Width, item.Height));
            }
        }
        
        showPCGMenu = false;
    }

    private async void GenerateFromImage()
    {
        // For now, we'll use a placeholder path
        // In a real implementation, this would open a file dialog
        string imagePath = "PCG/input_image.png";
        string outputPath = Path.Combine("Maps", "ImageGen", $"image_map_{DateTime.Now.Ticks}.bin");
        
        if (!File.Exists(imagePath))
        {
            Logger.Log(LogLevel.Warn, "EnhancedMapEditor", 
                "No image file found. Please place an image at PCG/input_image.png");
            showPCGMenu = false;
            return;
        }
        
        bool success = await PCGService.GenerateMapFromImageAsync(
            imagePath,
            outputPath,
            scale: 1,
            tolerance: 64,
            roomWidthTiles: 40,
            roomHeightTiles: 23,
            colorMapJson: "",
            packageName: "ImageMap"
        );
        
        if (success)
        {
            // Reload the map data to show the new map
            mapData.Reload();
            levels.Clear();
            foreach (LevelData level in mapData.Levels)
            {
                levels.Add(new LevelTemplate(level));
            }
            foreach (Rectangle item in mapData.Filler)
            {
                levels.Add(new LevelTemplate(item.X, item.Y, item.Width, item.Height));
            }
        }
        
        showPCGMenu = false;
    }

    private void RenderPCGMenu()
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 
            null, Engine.ScreenMatrix);

        // Draw semi-transparent background
        Draw.Rect(100, 100, 400, 350, Color.Black * 0.85f);
        Draw.HollowRect(100, 100, 400, 350, Color.Cyan);

        // Draw title
        ActiveFont.DrawOutline("PCG Generation Menu", new Vector2(120, 120), 
            Vector2.Zero, Vector2.One * 0.9f, Color.Cyan, 2f, Color.Black);

        // Draw menu options
        for (int i = 0; i < pcgMenuOptions.Length; i++)
        {
            Vector2 position = new Vector2(120, 160 + i * 30);
            Color color = (i == pcgMenuSelection) ? Color.Yellow : Color.White;
            
            if (i == pcgMenuSelection)
            {
                Draw.Rect(115, position.Y - 5, 370, 25, Color.Cyan * 0.3f);
            }
            
            ActiveFont.DrawOutline(pcgMenuOptions[i], position, 
                Vector2.Zero, Vector2.One * 0.7f, color, 2f, Color.Black);
        }

        // Draw parameters
        Vector2 paramPos = new Vector2(120, 300);
        ActiveFont.DrawOutline($"Difficulty: {pcgDifficulty} (1/2 to adjust)", paramPos, 
            Vector2.Zero, Vector2.One * 0.5f, Color.Orange, 2f, Color.Black);
        ActiveFont.DrawOutline($"Width: {pcgWidthRooms} rooms (3/4 to adjust)", 
            new Vector2(paramPos.X, paramPos.Y + 20), Vector2.Zero, 
            Vector2.One * 0.5f, Color.Orange, 2f, Color.Black);
        ActiveFont.DrawOutline($"Height: {pcgHeightRooms} rooms (5/6 to adjust)", 
            new Vector2(paramPos.X, paramPos.Y + 40), Vector2.Zero, 
            Vector2.One * 0.5f, Color.Orange, 2f, Color.Black);
        ActiveFont.DrawOutline($"Seed: {(pcgSeed >= 0 ? pcgSeed.ToString() : "Random")} (R to randomize)", 
            new Vector2(paramPos.X, paramPos.Y + 60), Vector2.Zero, 
            Vector2.One * 0.5f, Color.Lime, 2f, Color.Black);

        // Draw instructions
        ActiveFont.DrawOutline("Arrow keys: Navigate | Enter: Select | ESC: Close", 
            new Vector2(120, 410), Vector2.Zero, Vector2.One * 0.5f, 
            Color.LightGray, 2f, Color.Black);

        Draw.SpriteBatch.End();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void orig_Render()
    {
        UpdateMouse();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix * Engine.ScreenMatrix);
        float num = 1920f / Camera.Zoom;
        float num2 = 1080f / Camera.Zoom;
        int num3 = 5;
        float num4 = (float)Math.Floor(Camera.Left / (float)num3 - 1f) * (float)num3;
        float num5 = (float)Math.Floor(Camera.Top / (float)num3 - 1f) * (float)num3;
        for (float num6 = num4; num6 <= num4 + num + 10f; num6 += 5f)
        {
            Draw.Line(num6, Camera.Top, num6, Camera.Top + num2, gridColor);
        }
        for (float num7 = num5; num7 <= num5 + num2 + 10f; num7 += 5f)
        {
            Draw.Line(Camera.Left, num7, Camera.Left + num, num7, gridColor);
        }
        Draw.Line(0f, Camera.Top, 0f, Camera.Top + num2, Color.DarkSlateBlue, 1f / Camera.Zoom);
        Draw.Line(Camera.Left, 0f, Camera.Left + num, 0f, Color.DarkSlateBlue, 1f / Camera.Zoom);
        foreach (LevelTemplate level in levels)
        {
            level.RenderContents(Camera, levels);
        }
        foreach (LevelTemplate level2 in levels)
        {
            level2.RenderOutline(Camera);
        }
        foreach (LevelTemplate level3 in levels)
        {
            level3.RenderHighlight(Camera, selection.Contains(level3), hovered.Contains(level3));
        }
        if (mouseMode == MouseModes.Hover)
        {
            Draw.Line(mousePosition.X - 12f / Camera.Zoom, mousePosition.Y, mousePosition.X + 12f / Camera.Zoom, mousePosition.Y, Color.Yellow, 3f / Camera.Zoom);
            Draw.Line(mousePosition.X, mousePosition.Y - 12f / Camera.Zoom, mousePosition.X, mousePosition.Y + 12f / Camera.Zoom, Color.Yellow, 3f / Camera.Zoom);
        }
        else if (mouseMode == MouseModes.Select)
        {
            Draw.Rect(GetMouseRect(mouseDragStart, mousePosition), Color.Lime * 0.25f);
        }
        if (saveFlash > 0f)
        {
            Draw.Rect(Camera.Left, Camera.Top, num, num2, Color.White * Ease.CubeInOut(saveFlash));
        }
        if (fade > 0f)
        {
            Draw.Rect(0f, 0f, 320f, 180f, Color.Black * fade);
        }
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
        Draw.Rect(0f, 0f, 1920f, 72f, Color.Black);
        Vector2 position = new Vector2(16f, 4f);
        Vector2 position2 = new Vector2(1904f, 4f);
        if (MInput.Keyboard.Check(Keys.Q))
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.25f);
            foreach (LevelTemplate level4 in levels)
            {
                int num8 = 0;
                while (level4.Strawberries != null && num8 < level4.Strawberries.Count)
                {
                    Vector2 vector = level4.Strawberries[num8];
                    ActiveFont.DrawOutline(level4.StrawberryMetadata[num8], (new Vector2((float)level4.X + vector.X, (float)level4.Y + vector.Y) - Camera.Position) * Camera.Zoom + new Vector2(960f, 532f), new Vector2(0.5f, 1f), Vector2.One * 1f, Color.Red, 2f, Color.Black);
                    num8++;
                }
            }
        }
        if (hovered.Count == 0)
        {
            if (selection.Count > 0)
            {
                ActiveFont.Draw(selection.Count + " levels selected", position, Color.Red);
            }
            else
            {
                ActiveFont.Draw(Dialog.Clean(mapData.Data.Name), position, Color.Aqua);
                ActiveFont.Draw(string.Concat(mapData.Area.Mode, " MODE"), position2, Vector2.UnitX, Vector2.One, Color.Red);
            }
        }
        else if (hovered.Count == 1)
        {
            LevelTemplate levelTemplate = null;
            using (HashSet<LevelTemplate>.Enumerator enumerator2 = hovered.GetEnumerator())
            {
                if (enumerator2.MoveNext())
                {
                    levelTemplate = enumerator2.Current;
                }
            }
            string text = levelTemplate.ActualWidth.ToString() + "x" + levelTemplate.ActualHeight.ToString() + "   " + levelTemplate.X + "," + levelTemplate.Y + "   " + levelTemplate.X * 8 + "," + levelTemplate.Y * 8;
            ActiveFont.Draw(levelTemplate.Name, position, Color.Yellow);
            ActiveFont.Draw(text, position2, Vector2.UnitX, Vector2.One, Color.Green);
        }
        else
        {
            ActiveFont.Draw(hovered.Count + " levels", position, Color.Yellow);
        }
        Draw.SpriteBatch.End();
    }

    private void RenderManualText()
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);

        // First, cover the original text area with black
        Vector2 originalTextSize = Draw.DefaultFont.MeasureString(ManualText);
        float originalX = Engine.ViewWidth - originalTextSize.X - 10f;
        float originalY = Engine.ViewHeight - originalTextSize.Y - 10f;
        Draw.Rect(originalX - 10f, originalY - 10f, originalTextSize.X + 20f, originalTextSize.Y + 20f, Color.Black);

        // Calculate panel dimensions
        const float keyWidth = 50f;
        const float keyHeight = 24f;
        const float keySpacing = 8f;
        const float rowSpacing = 32f;
        const float padding = 16f;
        const float modifierOffset = 45f;

        float panelWidth = 380f;
        float panelHeight = padding * 2 + (VisualInstructions.Length * rowSpacing) - rowSpacing + padding + 20f;
        float panelX = Engine.ViewWidth - panelWidth - 10f;
        float panelY = Engine.ViewHeight - panelHeight - 10f;

        // Draw opaque background panel to cover original text
        Draw.Rect(panelX, panelY, panelWidth, panelHeight, Color.Black);
        Draw.HollowRect(panelX, panelY, panelWidth, panelHeight, Color.Cyan * 0.6f);

        // Draw title
        ActiveFont.DrawOutline("DEBUG MAP CONTROLS", new Vector2(panelX + padding, panelY + padding - 5f),
            Vector2.Zero, Vector2.One * 0.6f, Color.Cyan, 2f, Color.Black);

        // Draw each instruction with visual key representation
        for (int i = 0; i < VisualInstructions.Length; i++)
        {
            KeyInstruction instruction = VisualInstructions[i];
            float rowY = panelY + padding + 20f + (i * rowSpacing);
            float keyX = panelX + padding;

            // Draw modifier if present
            if (!string.IsNullOrEmpty(instruction.Modifier))
            {
                DrawKeySprite(keyX, rowY, instruction.Modifier, keyWidth * 0.7f, keyHeight, Color.Orange);
                keyX += modifierOffset;
            }

            // Draw main key
            DrawKeySprite(keyX, rowY, instruction.Key, keyWidth, keyHeight, Color.White);

            // Draw description text
            Vector2 textPos = new Vector2(keyX + keyWidth + keySpacing, rowY + keyHeight / 2f);
            ActiveFont.Draw(instruction.Description, textPos, Vector2.Zero, Vector2.One * 0.5f, Color.LightGray);
        }

        Draw.SpriteBatch.End();
    }

    private void DrawKeySprite(float x, float y, string text, float width, float height, Color color)
    {
        // Draw key background with rounded corners effect
        Color bgColor = color * 0.3f;
        Color borderColor = color * 0.8f;

        // Main key body
        Draw.Rect(x, y, width, height, bgColor);
        Draw.HollowRect(x, y, width, height, borderColor);

        // Draw key text centered
        Vector2 textPos = new Vector2(x + width / 2f, y + height / 2f + 2f);
        ActiveFont.Draw(text, textPos, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, color);

        // Add subtle highlight for 3D effect
        Draw.Rect(x + 2f, y + 2f, width - 4f, 2f, color * 0.15f);
    }

    private void RenderKeys()
    {
        if (keys == null && mapData?.Levels != null)
        {
            keys = new List<Vector2>();
            foreach (LevelData level in mapData.Levels)
            {
                Rectangle bounds = level.Bounds;
                Vector2 vector = new Vector2(bounds.X, bounds.Y);
                foreach (EntityData item in level.Entities.Where((EntityData entityData) => entityData.Name == "key"))
                {
                    keys.Add((vector + item.Position) / 8f);
                }
            }
        }
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix * Engine.ScreenMatrix);
        if (keys != null && keys.Count > 0)
        {
            for (int num = 0; num < keys.Count; num++)
            {
                Draw.HollowRect(keys[num].X - 1f, keys[num].Y - 2f, 3f, 3f, Color.Gold);
            }
        }
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
        if (MInput.Keyboard.Check(Keys.F1))
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.25f);
            if (keys != null && keys.Count > 0)
            {
                for (int num2 = 0; num2 < keys.Count; num2++)
                {
                    ActiveFont.DrawOutline((num2 + 1).ToString(), (keys[num2] - Camera.Position + Vector2.UnitX) * Camera.Zoom + new Vector2(960f, 540f), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Gold, 2f, Color.Black);
                }
            }
        }
        Draw.SpriteBatch.End();
    }

    private void RenderHighlightCurrentRoom()
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix * Engine.ScreenMatrix);
        if (CurrentSession != null)
        {
            levels.Find((LevelTemplate template) => template.Name == CurrentSession.Level)?.RenderHighlight(Camera, hovered: false, selected: true);
        }
        Draw.SpriteBatch.End();
    }

    private void CenterViewOnCurrentRespawn()
    {
        Session currentSession = CurrentSession;
        if (currentSession != null && currentSession.RespawnPoint.HasValue)
        {
            Camera.Position = CurrentSession.RespawnPoint.Value / 8f;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void orig_ctor_EnhancedMapEditor()
    {
        gridColor = new Color(0.1f, 0.1f, 0.1f);
        area = AreaKey.None;
        saveFlash = 0f;
    }
}
