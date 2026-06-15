namespace MaggyHelper.Helpers;

/// <summary>
/// Scans Everest content for audio assets from all loaded mods and warms them up.
/// This helps ensure custom banks and audio files are discovered early.
/// </summary>
public static class GlobalModAudioLoader
{
    private static bool _initialized;
    private static bool _hooksLoaded;
    private static readonly List<string> _audioAssets = new();
    private static readonly List<string> _audioBanks = new();

    public static IReadOnlyList<string> AudioAssets => _audioAssets;
    public static IReadOnlyList<string> AudioBanks => _audioBanks;

    public static void Load()
    {
        if (_hooksLoaded)
            return;

        On.Celeste.Audio.Init += OnAudioInit;
        _hooksLoaded = true;
    }

    public static void Initialize()
    {
        if (_initialized)
            return;

        _audioAssets.Clear();
        _audioBanks.Clear();

        try
        {
            foreach (KeyValuePair<string, ModAsset> pair in Everest.Content.Map)
            {
                string key = pair.Key;
                if (!key.StartsWith("Audio/", StringComparison.OrdinalIgnoreCase))
                    continue;

                _audioAssets.Add(key);

                if (key.EndsWith(".bank", StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith(".strings.bank", StringComparison.OrdinalIgnoreCase))
                {
                    _audioBanks.Add(key);
                }
            }

            _initialized = true;
            TryIngestNewBanks("Initialize");
            Logger.Log(
                LogLevel.Info,
                "MaggyHelper",
                $"GlobalModAudioLoader indexed {_audioAssets.Count} audio assets ({_audioBanks.Count} banks) across loaded mods.");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"GlobalModAudioLoader failed to initialize: {ex.Message}");
        }
    }

    public static void Unload()
    {
        if (_hooksLoaded)
        {
            On.Celeste.Audio.Init -= OnAudioInit;
            _hooksLoaded = false;
        }

        _initialized = false;
        _audioAssets.Clear();
        _audioBanks.Clear();
    }

    [Command("maggy_audio_list", "Lists discovered mod audio assets and banks. Usage: maggy_audio_list [all]")]
    private static void CommandListAudio(string mode = "")
    {
        if (!_initialized)
            Initialize();

        bool printAll = string.Equals(mode, "all", StringComparison.OrdinalIgnoreCase);

        Logger.Log(
            LogLevel.Info,
            "MaggyHelper",
            $"[Audio] Indexed {_audioAssets.Count} assets and {_audioBanks.Count} banks.");

        foreach (string bank in _audioBanks)
        {
            Logger.Log(LogLevel.Info, "MaggyHelper", $"[Audio][Bank] {bank}");
        }

        IEnumerable<string> assetsToPrint = printAll
            ? _audioAssets
            : _audioAssets.Take(50);

        foreach (string asset in assetsToPrint)
        {
            Logger.Log(LogLevel.Verbose, "MaggyHelper", $"[Audio][Asset] {asset}");
        }

        if (!printAll && _audioAssets.Count > 50)
        {
            Logger.Log(
                LogLevel.Info,
                "MaggyHelper",
                $"[Audio] Showing first 50 assets. Run 'maggy_audio_list all' to print all {_audioAssets.Count} assets.");
        }
    }

    [Command("maggy_audio_dump", "Dumps discovered mod audio assets and banks to a file. Usage: maggy_audio_dump [fileName]")]
    private static void CommandDumpAudio(string fileName = "")
    {
        if (!_initialized)
            Initialize();

        string safeName = string.IsNullOrWhiteSpace(fileName)
            ? "MaggyHelper_AudioDump.txt"
            : fileName.Trim();

        if (!safeName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            safeName += ".txt";

        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
        Directory.CreateDirectory(outputDirectory);

        string outputPath = Path.Combine(outputDirectory, safeName);

        using (StreamWriter writer = new StreamWriter(outputPath, false))
        {
            writer.WriteLine("MaggyHelper Audio Dump");
            writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"Banks: {_audioBanks.Count}");
            writer.WriteLine($"Assets: {_audioAssets.Count}");
            writer.WriteLine();

            writer.WriteLine("=== BANKS ===");
            foreach (string bank in _audioBanks)
            {
                writer.WriteLine(bank);
            }

            writer.WriteLine();
            writer.WriteLine("=== ASSETS ===");
            foreach (string asset in _audioAssets)
            {
                writer.WriteLine(asset);
            }
        }

        Logger.Log(LogLevel.Info, "MaggyHelper", $"[Audio] Dumped {_audioBanks.Count} banks and {_audioAssets.Count} assets to: {outputPath}");
    }

    [Command("maggy_audio_reload", "Re-ingests mod audio banks through Celeste audio system")]
    private static void CommandReloadAudio()
    {
        if (!_initialized)
            Initialize();

        TryIngestNewBanks("ConsoleCommand");
    }

    private static void OnAudioInit(On.Celeste.Audio.orig_Init orig)
    {
        try
        {
            orig();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"[Audio] Audio.Init failed during bank ingestion: {ex.Message}");
            Logger.Log(LogLevel.Verbose, "MaggyHelper", $"[Audio] Full exception: {ex}");
        }
    }

    private static void TryIngestNewBanks(string source)
    {
        try
        {
            Audio.IngestNewBanks();
            Logger.Log(LogLevel.Info, "MaggyHelper", $"[Audio] Ingested mod banks ({source}).");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"[Audio] Failed to ingest mod banks ({source}): {ex.Message}");
        }
    }
}
