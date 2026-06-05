using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.UI
{
    /// <summary>
    /// Debug room-warp menu for testing cycles.
    /// Enabled when DeveloperBypass or DebugMode is active.
    /// Provides quick room navigation without completing normal progression.
    /// </summary>
    public class DebugRoomWarpMenu : Entity
    {
        private bool active = false;
        private int selectedIndex = 0;
        private List<string> roomNames = new List<string>();
        private Level level;

        // Keybind: F12 to open debug warp menu
        private const Keys WARP_MENU_KEY = Keys.F12;

        public DebugRoomWarpMenu()
        {
            Tag = Tags.HUD | Tags.PauseUpdate;
            Depth = -20000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            if (level?.Session?.MapData != null)
            {
                ScanRooms();
            }
        }

        private void ScanRooms()
        {
            roomNames.Clear();
            if (level?.Session?.MapData?.Levels == null) return;

            foreach (var levelData in level.Session.MapData.Levels)
            {
                if (!string.IsNullOrEmpty(levelData.Name))
                {
                    roomNames.Add(levelData.Name);
                }
            }

            roomNames.Sort();
            Logger.Log(LogLevel.Info, "MaggyHelper/Debug", $"DebugRoomWarpMenu: scanned {roomNames.Count} rooms");
        }

        public override void Update()
        {
            base.Update();

            if (!active)
            {
                // Only allow opening if DeveloperBypass or DebugMode is enabled
                var settings = global::Celeste.Mod.MaggyHelper.MaggyHelperModule.Settings;
                if ((settings?.DeveloperBypass ?? false) || (settings?.DebugMode ?? false))
                {
                    if (MInput.Keyboard.Pressed(WARP_MENU_KEY))
                    {
                        Open();
                    }
                }
                return;
            }

            // Menu navigation
            if (Input.MenuUp.Pressed && selectedIndex > 0)
            {
                selectedIndex--;
                Audio.Play("event:/ui/main/rollover_down");
            }
            else if (Input.MenuDown.Pressed && selectedIndex < roomNames.Count - 1)
            {
                selectedIndex++;
                Audio.Play("event:/ui/main/rollover_down");
            }
            else if (Input.MenuConfirm.Pressed)
            {
                WarpToRoom(roomNames[selectedIndex]);
                Close();
            }
            else if (Input.MenuCancel.Pressed || Input.Pause.Pressed)
            {
                Close();
            }
        }

        private void Open()
        {
            if (level == null) return;

            active = true;
            level.Paused = true;
            ScanRooms(); // Refresh room list
            selectedIndex = Math.Max(0, roomNames.IndexOf(level.Session.Level));
            Logger.Log(LogLevel.Info, "MaggyHelper/Debug", "DebugRoomWarpMenu opened");
        }

        private void Close()
        {
            active = false;
            if (level != null)
            {
                level.Paused = false;
            }
            Logger.Log(LogLevel.Info, "MaggyHelper/Debug", "DebugRoomWarpMenu closed");
        }

        private void WarpToRoom(string roomName)
        {
            if (level == null || string.IsNullOrEmpty(roomName)) return;

            try
            {
                Logger.Log(LogLevel.Info, "MaggyHelper/Debug", $"Warping to room: {roomName}");

                // Find the room's spawn point
                var targetLevelData = level.Session.MapData.Get(roomName);
                if (targetLevelData == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/Debug", $"Room '{roomName}' not found in map data");
                    return;
                }

                // Use the room's default spawn point
                Vector2 spawnPoint = targetLevelData.Spawns.Count > 0
                    ? targetLevelData.Spawns[0]
                    : targetLevelData.Bounds.Center.ToVector2();

                // Perform the warp
                level.OnEndOfFrame += () =>
                {
                    level.TeleportTo(level.Tracker.GetEntity<Player>(), roomName, Player.IntroTypes.Transition);
                };

                Logger.Log(LogLevel.Info, "MaggyHelper/Debug", $"Warp to '{roomName}' scheduled");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper/Debug", $"Failed to warp to '{roomName}': {ex.Message}");
            }
        }

        public override void Render()
        {
            if (!active || roomNames.Count == 0) return;

            // Dark overlay
            Draw.Rect(0, 0, Engine.ViewWidth, Engine.ViewHeight, Color.Black * 0.85f);

            // Title
            string title = "DEBUG ROOM WARP";
            ActiveFont.Draw(title, new Vector2(Engine.ViewWidth / 2f, 50f), new Vector2(0.5f, 0.5f), Vector2.One * 1.2f, Color.Gold);

            // Current room indicator
            string currentRoom = $"Current: {level?.Session?.Level ?? "?"}";
            ActiveFont.Draw(currentRoom, new Vector2(Engine.ViewWidth / 2f, 90f), new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, Color.Gray);

            // Render warp list on left, debug tools on right
            RenderRoomList();
            RenderDebugTools();

            // Instructions
            string instructions = "UP/DOWN = Select | CONFIRM = Warp | JUMP = Heal | GRAB = Invincible | DASH = +1 Dash | CANCEL = Close";
            ActiveFont.Draw(instructions, new Vector2(Engine.ViewWidth / 2f, Engine.ViewHeight - 30f), new Vector2(0.5f, 0.5f), Vector2.One * 0.4f, Color.LightGray * 0.8f);
        }

        private void RenderRoomList()
        {
            float startY = 130f;
            float lineHeight = 34f;
            int maxVisible = 16;
            int startIdx = Math.Max(0, Math.Min(selectedIndex - maxVisible / 2, roomNames.Count - maxVisible));
            int endIdx = Math.Min(roomNames.Count, startIdx + maxVisible);

            ActiveFont.Draw("Rooms", new Vector2(Engine.ViewWidth / 4f, startY - 30f), new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, Color.Gray);

            for (int i = startIdx; i < endIdx; i++)
            {
                float y = startY + (i - startIdx) * lineHeight;
                bool isSelected = (i == selectedIndex);

                Color color = isSelected ? Color.Gold : Color.White * 0.7f;
                float scale = isSelected ? 1.0f : 0.75f;

                if (isSelected)
                {
                    ActiveFont.Draw(">", new Vector2(40f, y), new Vector2(0f, 0.5f), Vector2.One * scale, color);
                }

                ActiveFont.Draw(
                    roomNames[i],
                    new Vector2(70f, y),
                    new Vector2(0f, 0.5f),
                    Vector2.One * scale,
                    color);
            }
        }

        private void RenderDebugTools()
        {
            float x = Engine.ViewWidth * 0.6f;
            float y = 130f;

            ActiveFont.Draw("Debug Tools", new Vector2(x, y - 30f), new Vector2(0f, 0.5f), Vector2.One * 0.7f, Color.Gray);

            var player = level?.Tracker?.GetEntity<Player>();
            if (player != null)
            {
                var tools = new (string label, string value)[]
                {
                    ("Position", $"{player.X:F0}, {player.Y:F0}"),
                    ("Speed", $"{player.Speed.X:F1}, {player.Speed.Y:F1}"),
                    ("OnGround", player.OnGround() ? "Yes" : "No"),
                    ("State", player.StateMachine != null ? player.StateMachine.State.ToString() : "?"),
                    ("Depth", player.Depth.ToString()),
                    ("Visible", player.Visible ? "Yes" : "No"),
                };

                for (int i = 0; i < tools.Length; i++)
                {
                    float lineY = y + i * 30f;
                    ActiveFont.Draw(tools[i].label + ":", new Vector2(x, lineY), new Vector2(0f, 0.5f), Vector2.One * 0.55f, Color.LightGray);
                    ActiveFont.Draw(tools[i].value, new Vector2(x + 140f, lineY), new Vector2(0f, 0.5f), Vector2.One * 0.55f, Color.White);
                }

                // Inventory / session flags
                y += tools.Length * 30f + 20f;
                var session = level?.Session;
                if (session != null)
                {
                    ActiveFont.Draw("Session", new Vector2(x, y), new Vector2(0f, 0.5f), Vector2.One * 0.6f, Color.Gray);
                    y += 25f;

                    var flags = new (string label, bool value)[]
                    {
                        ("Grabbed Golden", session.GrabbedGolden),
                        ("HitCheckpoint", session.HitCheckpoint),
                        ("Cassette", session.Cassette),
                        ("Heart Gem", session.HeartGem),
                    };

                    for (int i = 0; i < flags.Length; i++)
                    {
                        Color c = flags[i].value ? Color.Green : Color.Red;
                        ActiveFont.Draw($"{flags[i].label}: {(flags[i].value ? "Yes" : "No")}", new Vector2(x, y + i * 24f), new Vector2(0f, 0.5f), Vector2.One * 0.5f, c);
                    }
                }
            }
            else
            {
                ActiveFont.Draw("No player found", new Vector2(x, y), new Vector2(0f, 0.5f), Vector2.One * 0.6f, Color.Red);
            }
        }
    }
}

