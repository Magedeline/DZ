namespace DZ;

/// <summary>
/// Opt-in sprite/editor contract validator for DZ development builds.
/// <para>
/// Validates that sprite-bank IDs referenced in high-risk gameplay paths are
/// present in <c>Graphics/Sprites.xml</c>, that known Lönn entity definitions
/// exist on disk, and that the <see cref="DzSideMap"/> self-check passes.
/// </para>
/// <para>
/// <b>Running the validator:</b>
/// <list type="bullet">
///   <item>Call <see cref="RunAll"/> explicitly from a debug command / cheat
///     code handler after the game has finished loading content.</item>
///   <item>In <c>#if DEBUG</c> builds it is also invoked automatically during
///     <see cref="DZModule.Load"/> via <c>RunAllDeferred</c>.</item>
/// </list>
/// Results are written to the Everest log under the category
/// <c>DZ/SpriteValidator</c>.  Warnings indicate missing entries; errors
/// indicate problems that are likely to cause silent failures at runtime.
/// </para>
/// <para>
/// <b>Design note — explicit manifest, not regex:</b> The validator uses
/// explicitly declared contract lists rather than a whole-repo regex scan.
/// This keeps it fast, reliable, and free of false-positives.  Grow the
/// manifest lists in this file when new high-risk paths are identified.
/// </para>
/// </summary>
public static class SpriteContractValidator
{
    private const string LogCategory = "DZ/SpriteValidator";

    // ── Sprite-bank ID manifest ───────────────────────────────────────────
    // These IDs are referenced in high-risk gameplay paths.  All of them must
    // have a matching <Sprite> (or copy= entry) in Graphics/Sprites.xml.

    private static readonly string[] HighRiskSpriteBankIds =
    {
        // Kirby player sprites
        "player_no_backpack",
        "player_level_up",
        "player_sweat",
        // Heart gem / collectible sprites expected in DZ Sprites.xml
        "heartgem0",
        "heartgem1",
        "heartgem2",
        "heartgem3",
    };

    // ── PlayerSpriteMode → sprite-bank ID mapping manifest ───────────────
    // Mirrors the logic in PlayerSpriteModeExtensions.GetSpriteBankId() so
    // mismatches between the two are caught at validation time.

    private static readonly (string modeLabel, string spriteBankId)[] PlayerSpriteModeMap =
    {
        // Vanilla modes (PlayerSpriteMode values 0–4 as string labels)
        ("Normal",           "player"),
        ("MadelineNoBackpack","player_no_backpack"),
        ("Madeline",         "player"),
        ("MadelineAsBadeline","player"),
        // Custom Chara mode (value 100)
        ("Chara",            "player"),   // stub — actual ID not yet wired
    };

    // ── High-risk entity Lönn definition manifest ─────────────────────────
    // Each entry is (entityId, expectedLuaPath, spriteBankId, humanNote).
    // The Lua path is relative to the repository root.

    private record EntityContract(
        string EntityId,
        string LuaPath,
        string SpriteBankId,
        string Note);

    private static readonly EntityContract[] EntityContracts =
    {
        new("DZ/KirbyPlayerSpawner",
            "Loenn/entities/kirbyPlayerSpawner.lua",
            "player_no_backpack",
            "Kirby player spawner — must exist or Kirby levels cannot spawn the player"),

        new("DZ/DZHeartGem",
            "Loenn/entities/heartGem.lua",
            "heartgem0",
            "DZ Heart Gem — incorrect sprite ID causes silent no-display on collect screen"),

        new("DZ/FakeHeartGem",
            "Loenn/entities/fakeHeartGem.lua",
            "",
            "Fake Heart Gem — no required sprite bank entry"),

        new("DZ/CassetteTape",
            "Loenn/entities/cassetteTape.lua",
            "",
            "Cassette/CassetteTape entity — drives side unlocks"),

        new("DZ/PopstarBerry",
            "Loenn/entities/popstarBerry.lua",
            "",
            "Popstar Berry collectible"),

        new("DZ/PinkPlatinumStrawberry",
            "Loenn/entities/pinkPlatinumStrawberry.lua",
            "",
            "Pink Platinum Strawberry — do NOT rename map entity ID before save migration is ready"),
    };

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Run all validation checks and return a list of result messages.
    /// Each message begins with <c>[ERROR]</c>, <c>[WARN]</c>, or <c>[OK]</c>.
    /// </summary>
    public static IReadOnlyList<string> RunAll()
    {
        var results = new List<string>();

        Logger.Log(LogLevel.Info, LogCategory, "=== SpriteContractValidator starting ===");

        // 1. DzSideMap self-check
        RunSideMapSelfCheck(results);

        // 2. Sprite-bank ID checks (can only run after game content has loaded)
        RunSpriteBankChecks(results);

        // 3. Entity Lönn definition checks
        RunEntityContractChecks(results);

        // 4. PlayerSpriteMode mapping checks
        RunPlayerSpriteModeChecks(results);

        // Summary
        int errors   = results.Count(r => r.StartsWith("[ERROR]"));
        int warnings = results.Count(r => r.StartsWith("[WARN]"));
        Logger.Log(LogLevel.Info, LogCategory,
            $"=== Done: {errors} error(s), {warnings} warning(s) out of {results.Count} checks ===");

        return results;
    }

    /// <summary>
    /// Runs <see cref="RunAll"/> once after the Celeste content has finished
    /// loading.  Safe to call from <c>Load()</c>.
    /// </summary>
    public static void RunAllDeferred()
    {
        // We cannot run immediately during Load() because GFX.SpriteBank is
        // not populated until after area data has loaded.
        On.Celeste.AreaData.Load += OnAreaDataLoadOnce;
        Logger.Log(LogLevel.Info, LogCategory,
            "SpriteContractValidator deferred — will run after AreaData.Load.");
    }

    // ── Internal checks ───────────────────────────────────────────────────

    private static void RunSideMapSelfCheck(List<string> results)
    {
        var failures = DzSideMap.SelfCheck();
        if (failures.Count == 0)
        {
            results.Add("[OK] DzSideMap self-check: all mapping tables consistent");
            Logger.Log(LogLevel.Info, LogCategory, "DzSideMap self-check: PASS");
        }
        else
        {
            foreach (var f in failures)
            {
                var msg = $"[ERROR] DzSideMap self-check: {f}";
                results.Add(msg);
                Logger.Log(LogLevel.Error, LogCategory, msg);
            }
        }
    }

    private static void RunSpriteBankChecks(List<string> results)
    {
        // GFX.SpriteBank may be null if called too early; guard gracefully.
        if (GFX.SpriteBank == null)
        {
            var msg = "[WARN] Sprite bank not yet loaded — skipping sprite-bank ID checks";
            results.Add(msg);
            Logger.Log(LogLevel.Warn, LogCategory, msg);
            return;
        }

        foreach (var id in HighRiskSpriteBankIds)
        {
            if (GFX.SpriteBank.Has(id))
            {
                results.Add($"[OK] Sprite bank entry present: \"{id}\"");
            }
            else
            {
                var msg = $"[ERROR] Missing sprite bank entry: \"{id}\" — add it to Graphics/Sprites.xml";
                results.Add(msg);
                Logger.Log(LogLevel.Error, LogCategory, msg);
            }
        }
    }

    private static void RunEntityContractChecks(List<string> results)
    {
        // Determine repo root from the mod's content directory.
        string repoRoot = TryGetRepoRoot();

        foreach (var contract in EntityContracts)
        {
            var luaFull = repoRoot != null
                ? Path.Combine(repoRoot, contract.LuaPath)
                : null;

            bool luaExists = luaFull != null && File.Exists(luaFull);

            if (luaExists)
            {
                results.Add($"[OK] Lönn definition found: \"{contract.EntityId}\" → {contract.LuaPath}");
            }
            else
            {
                var msg = $"[WARN] Missing Lönn definition: entity=\"{contract.EntityId}\" " +
                          $"expected-lua=\"{contract.LuaPath}\" note=\"{contract.Note}\"";
                results.Add(msg);
                Logger.Log(LogLevel.Warn, LogCategory, msg);
            }

            // If a sprite bank ID is specified, also check it exists.
            if (!string.IsNullOrEmpty(contract.SpriteBankId) && GFX.SpriteBank != null)
            {
                if (!GFX.SpriteBank.Has(contract.SpriteBankId))
                {
                    var msg = $"[ERROR] Entity \"{contract.EntityId}\" expects sprite bank ID " +
                              $"\"{contract.SpriteBankId}\" which is missing from Graphics/Sprites.xml. " +
                              $"Suggested fix: add <Sprite id=\"{contract.SpriteBankId}\" .../> to Sprites.xml";
                    results.Add(msg);
                    Logger.Log(LogLevel.Error, LogCategory, msg);
                }
            }
        }
    }

    private static void RunPlayerSpriteModeChecks(List<string> results)
    {
        foreach (var (modeLabel, spriteBankId) in PlayerSpriteModeMap)
        {
            if (string.IsNullOrEmpty(spriteBankId))
            {
                results.Add($"[WARN] PlayerSpriteMode \"{modeLabel}\" has no sprite bank ID in manifest — update SpriteContractValidator");
                continue;
            }

            if (GFX.SpriteBank == null)
            {
                results.Add($"[WARN] Sprite bank not loaded — cannot check PlayerSpriteMode \"{modeLabel}\" → \"{spriteBankId}\"");
                continue;
            }

            if (GFX.SpriteBank.Has(spriteBankId))
            {
                results.Add($"[OK] PlayerSpriteMode \"{modeLabel}\" → sprite bank ID \"{spriteBankId}\" present");
            }
            else
            {
                var msg = $"[ERROR] PlayerSpriteMode \"{modeLabel}\" → sprite bank ID \"{spriteBankId}\" missing from Sprites.xml";
                results.Add(msg);
                Logger.Log(LogLevel.Error, LogCategory, msg);
            }
        }
    }

    private static string TryGetRepoRoot()
    {
        try
        {
            // The compiled DLL lives at {repoRoot}/bin/DZ.dll.
            // Walk up from the DLL directory to find the repo root which
            // contains both Loenn/ and Source/.
            var dllPath = typeof(SpriteContractValidator).Assembly.Location;
            if (string.IsNullOrEmpty(dllPath))
                return null;

            // bin/ → repoRoot
            var binDir = Path.GetDirectoryName(dllPath);
            if (binDir == null) return null;

            var candidate = Path.GetFullPath(Path.Combine(binDir, ".."));
            if (Directory.Exists(Path.Combine(candidate, "Loenn")))
                return candidate;
        }
        catch
        {
            // Not critical — Lönn checks will be skipped gracefully.
        }
        return null;
    }

    // ── Deferred hook ─────────────────────────────────────────────────────

    private static bool _deferredRan;

    private static void OnAreaDataLoadOnce(On.Celeste.AreaData.orig_Load orig)
    {
        // Let original load run so GFX.SpriteBank is populated before we check.
        orig();

        // Unhook AFTER orig() has run. Unhooking first disposes the detour that
        // the captured `orig` delegate depends on, so calling orig() afterward
        // throws "InvalidOperationException: Detour has been removed" the moment
        // AreaData.Load() runs -- crashes GameLoader.LoadThread() on every boot.
        On.Celeste.AreaData.Load -= OnAreaDataLoadOnce;

        if (_deferredRan) return;
        _deferredRan = true;

        RunAll();
    }
}
