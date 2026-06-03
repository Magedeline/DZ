using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// An in-game menu for the Hybrid PCG system, opened with a keybind.
/// Provides one-press buttons for Generate, Extract, Inspect, and Load.
/// </summary>
public class PCGQuickMenu : Entity
{
    private TextMenu menu;
    private bool focused;
    private string statusMessage = "";
    private float statusTimer;

    public PCGQuickMenu()
    {
        AddTag(Tags.HUD);
        AddTag(Tags.FrozenUpdate);
        BuildMenu();
    }

    private void BuildMenu()
    {
        menu = new TextMenu();
        menu.AutoScroll = false;

        menu.Add(new TextMenu.Header("MAGPCG - Quick Menu"));
        menu.Add(new TextMenu.SubHeader("Generate maps without typing commands"));

        // Generate & Play (CelesteRandomizer-style dynamic area registration)
        menu.Add(new TextMenu.Button("Generate & Play Now")
        {
            OnPressed = () => OnGenerateAndPlay()
        });

        // Generate (file only)
        menu.Add(new TextMenu.Button("Generate New Map (File Only)")
        {
            OnPressed = () => OnGenerate()
        });

        // Extract
        menu.Add(new TextMenu.Button("Extract from Current Map")
        {
            OnPressed = () => OnExtract()
        });

        // Inspect
        menu.Add(new TextMenu.Button("Inspect Latest Map (JSON)")
        {
            OnPressed = () => OnInspect()
        });

        // Load
        menu.Add(new TextMenu.Button("Copy Latest to Test Folder")
        {
            OnPressed = () => OnLoad()
        });

        menu.Add(new TextMenu.SubHeader("---"));

        // Settings shortcuts
        menu.Add(new TextMenu.OnOff("Auto-build library if missing", true)
        {
            OnValueChange = v => SettingsAutoBuild = v
        });

        menu.Add(new TextMenu.SubHeader("---"));

        // Close
        menu.Add(new TextMenu.Button("Close (ESC)")
        {
            OnPressed = Close
        });

        menu.Selection = 1;
        focused = true;
    }

    public bool SettingsAutoBuild { get; set; } = true;

    public override void Update()
    {
        if (!focused)
        {
            RemoveSelf();
            return;
        }

        if (menu != null)
        {
            menu.Update();
            if (Input.MenuCancel.Pressed || Input.Pause.Pressed)
            {
                Close();
                return;
            }
        }

        if (statusTimer > 0)
            statusTimer -= Engine.RawDeltaTime;
        else
            statusMessage = "";

        base.Update();
    }

    public override void Render()
    {
        if (!focused || menu == null) return;

        // Draw semi-transparent backdrop
        Draw.Rect(0, 0, 1920, 1080, Color.Black * 0.6f);

        menu.Render();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            Vector2 pos = new Vector2(960, 1000);
            ActiveFont.DrawOutline(statusMessage, pos, new Vector2(0.5f, 0.5f), Vector2.One, Color.White, 2f, Color.Black);
        }
    }

    private void Close()
    {
        focused = false;
        menu.RemoveSelf();
        menu = null;
    }

    private void ShowStatus(string msg)
    {
        statusMessage = msg;
        statusTimer = 4f;
        Logger.Log(LogLevel.Info, "PCGQuickMenu", msg);
    }

    private void OnGenerate()
    {
        string outputPath = $"PCG/Generated/pcg_map_{DateTime.Now:yyyyMMdd_HHmmss}.bin";
        string templateLibrary = "PCG/Templates/library.json";

        if (!File.Exists(templateLibrary) && SettingsAutoBuild)
        {
            string mapsDir = "Maps";
            if (Directory.Exists(mapsDir))
            {
                var mapFiles = Directory.GetFiles(mapsDir, "*.bin", SearchOption.AllDirectories).Take(5).ToArray();
                if (mapFiles.Length > 0)
                {
                    ShowStatus("Building template library from maps...");
                    Task.Run(async () =>
                    {
                        await PCGService.BuildTemplateLibraryAsync(mapFiles, templateLibrary);
                        await GenerateNow(templateLibrary, outputPath);
                    });
                    return;
                }
            }
        }

        if (!File.Exists(templateLibrary))
        {
            ShowStatus("No template library! Run 'Extract' first.");
            return;
        }

        ShowStatus("Generating map...");
        Task.Run(async () => await GenerateNow(templateLibrary, outputPath));
    }

    private async Task GenerateNow(string library, string output)
    {
        try
        {
            bool ok = await PCGService.GenerateHybridMapAsync(library, output, -1, 8, 2, "pathway", "balanced");
            if (ok)
                ShowStatus($"Map saved: {Path.GetFileName(output)}");
            else
                ShowStatus("Generation failed. Check log.");
        }
        catch (Exception ex)
        {
            ShowStatus($"Error: {ex.Message}");
        }
    }

    private void OnExtract()
    {
        string mapPath = "";
        if (Engine.Scene is Level level && level.Session?.MapData?.Filename != null)
        {
            mapPath = level.Session.MapData.Filename + ".bin";
        }

        if (string.IsNullOrEmpty(mapPath) || !File.Exists(mapPath))
        {
            ShowStatus("No current map found. Provide a path via console.");
            return;
        }

        ShowStatus("Extracting templates...");
        Task.Run(() =>
        {
            try
            {
                var templates = RoomTemplateLoader.ExtractTemplatesFromMap(mapPath, "PCG/Templates");
                ShowStatus($"Extracted {templates.Count} templates.");
            }
            catch (Exception ex)
            {
                ShowStatus($"Extract failed: {ex.Message}");
            }
        });
    }

    private void OnInspect()
    {
        var generatedDir = "PCG/Generated";
        if (!Directory.Exists(generatedDir))
        {
            ShowStatus("No generated maps yet. Generate first.");
            return;
        }

        var newest = Directory.GetFiles(generatedDir, "*.bin")
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTime)
            .FirstOrDefault();

        if (newest == null)
        {
            ShowStatus("No .bin files in PCG/Generated.");
            return;
        }

        string outputPath = Path.ChangeExtension(newest.FullName, ".inspect.json");
        try
        {
            var root = BinaryPacker.FromBinary(newest.FullName);
            if (root == null)
            {
                ShowStatus("Failed to parse map binary.");
                return;
            }

            var tree = KIRBY_CELESTEModule.SerializeElementStatic(root);
            var json = System.Text.Json.JsonSerializer.Serialize(tree, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
            File.WriteAllText(outputPath, json);
            ShowStatus($"JSON dumped: {Path.GetFileName(outputPath)}");
        }
        catch (Exception ex)
        {
            ShowStatus($"Inspect failed: {ex.Message}");
        }
    }

    private void OnLoad()
    {
        var generatedDir = "PCG/Generated";
        if (!Directory.Exists(generatedDir))
        {
            ShowStatus("No generated maps. Generate first.");
            return;
        }

        var newest = Directory.GetFiles(generatedDir, "*.bin")
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTime)
            .FirstOrDefault();

        if (newest == null)
        {
            ShowStatus("No .bin files found.");
            return;
        }

        string testMapDir = Path.Combine("Maps", "PCG_Test");
        Directory.CreateDirectory(testMapDir);
        string destPath = Path.Combine(testMapDir, $"pcg_test_{Path.GetFileName(newest.FullName)}");
        try
        {
            File.Copy(newest.FullName, destPath, true);
            ShowStatus($"Copied to {destPath}");
        }
        catch (Exception ex)
        {
            ShowStatus($"Copy failed: {ex.Message}");
        }
    }

    private void OnGenerateAndPlay()
    {
        string templateLibrary = "PCG/Templates/library.json";
        if (!File.Exists(templateLibrary) && SettingsAutoBuild)
        {
            string mapsDir = "Maps";
            if (Directory.Exists(mapsDir))
            {
                var mapFiles = Directory.GetFiles(mapsDir, "*.bin", SearchOption.AllDirectories).Take(5).ToArray();
                if (mapFiles.Length > 0)
                {
                    ShowStatus("Building template library from maps...");
                    Task.Run(async () =>
                    {
                        await PCGService.BuildTemplateLibraryAsync(mapFiles, templateLibrary);
                        await GenerateAndPlayNow(templateLibrary);
                    });
                    return;
                }
            }
        }

        if (!File.Exists(templateLibrary))
        {
            ShowStatus("No template library! Run Extract first.");
            return;
        }

        ShowStatus("Generating map and preparing warp...");
        Task.Run(async () => await GenerateAndPlayNow(templateLibrary));
    }

    private async Task GenerateAndPlayNow(string templateLibrary)
    {
        try
        {
            var key = await PCGAreaRegistrar.GenerateAndPlay(templateLibrary, -1, 8, 2, autoWarp: true);
            if (key != AreaKey.None)
                ShowStatus("Warping to generated map...");
            else
                ShowStatus("Failed to generate or register map.");
        }
        catch (Exception ex)
        {
            ShowStatus($"Generate & Play error: {ex.Message}");
        }
    }
}
