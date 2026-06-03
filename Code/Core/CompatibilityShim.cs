using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Utils
{
    public static class Util
    {
        public static Rectangle Rectangle(Vector2 from, Vector2 to)
        {
            int left = (int)Math.Min(from.X, to.X);
            int top = (int)Math.Min(from.Y, to.Y);
            int right = (int)Math.Max(from.X, to.X);
            int bottom = (int)Math.Max(from.Y, to.Y);
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public static float Mod(float value, float modulo)
        {
            if (modulo == 0f)
            {
                return 0f;
            }

            float result = value % modulo;
            return result < 0f ? result + modulo : result;
        }

        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }
    }
}

namespace Celeste
{
    public sealed class TimeRateModifier(float multiplier = 1f, bool active = true) : Component(active, false)
    {
        public float Multiplier { get; set; } = multiplier;
    }

    public static class ShapeRenderer
    {
        public static void DrawCircleOutline(Vector2 center, float radius, Color color, float thickness = 1f, int segments = 12)
        {
            segments = Math.Max(3, segments);
            Vector2 previous = center + Calc.AngleToVector(0f, radius);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i / (float)segments * MathHelper.TwoPi;
                Vector2 current = center + Calc.AngleToVector(angle, radius);
                Draw.Line(previous, current, color, thickness);
                previous = current;
            }
        }

        public static void DrawTrail(Vector2 position, Vector2 direction, Color color,
            int count = 3, float spacing = 8f, float startRadius = 4f, float radiusShrink = 1f,
            float alphaFade = 0.15f, int segments = 12)
        {
            Vector2 normalized = direction.SafeNormalize();

            for (int i = 0; i < count; i++)
            {
                float alpha = Math.Max(0f, 1f - alphaFade * (i + 1));
                float radius = Math.Max(0.5f, startRadius - radiusShrink * i);
                Vector2 trailPos = position - normalized * spacing * (i + 1);
                DrawCircleOutline(trailPos, radius, color * alpha, 1f, segments);
            }
        }
    }

    public static class LuaCutsceneManager
    {
        public static bool IsInitialized => true;

        public static void CallLuaFunction(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", $"LuaCutsceneManager compatibility call: {command}");
        }
    }

    public static class PlayerCharacterIds
    {
        public const string Default = Madeline;
        public const string Madeline = "madeline";
        public const string Kirby = "kirby";
    }

    public static class PlayerCharacter
    {
        public static string NormalizeId(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return PlayerCharacterIds.Default;
            }

            return characterId.Trim().ToLowerInvariant() switch
            {
                "default" => PlayerCharacterIds.Default,
                "maggy" => PlayerCharacterIds.Madeline,
                _ => characterId.Trim().ToLowerInvariant()
            };
        }
    }

    public static class AtlasPathHelper
    {
        private static readonly string[] SupportedRootFolders =
        {
            "characters",
            "objects",
            "collectables",
            "collectibles"
        };

        public static string ResolveTexturePath(string path)
        {
            return ResolvePath(path, candidate => GFX.Game != null && GFX.Game.Has(candidate));
        }

        public static string ResolveAtlasPath(string path)
        {
            return ResolvePath(path, candidate => GFX.Game != null && (GFX.Game.Has(candidate) || GFX.Game.HasAtlasSubtextures(candidate)));
        }

        public static bool HasTexture(string path)
        {
            string resolvedPath = ResolveTexturePath(path);
            return GFX.Game != null && GFX.Game.Has(resolvedPath);
        }

        public static MTexture TryGetTexture(string path)
        {
            if (GFX.Game == null)
            {
                return null;
            }

            string resolvedPath = ResolveTexturePath(path);
            return GFX.Game.Has(resolvedPath) ? GFX.Game[resolvedPath] : null;
        }

        public static MTexture GetTexture(string path)
        {
            return GFX.Game[ResolveTexturePath(path)];
        }

        public static List<MTexture> GetAtlasSubtextures(string path)
        {
            return GFX.Game.GetAtlasSubtextures(ResolveAtlasPath(path));
        }

        public static Sprite CreateSprite(string path)
        {
            return new Sprite(GFX.Game, ResolveAtlasPath(path));
        }

        private static string ResolvePath(string path, Func<string, bool> exists)
        {
            string normalizedPath = NormalizePath(path);
            if (string.IsNullOrEmpty(normalizedPath) || GFX.Game == null)
            {
                return normalizedPath;
            }

            foreach (string candidate in GetCandidates(normalizedPath))
            {
                if (exists(candidate))
                {
                    return candidate;
                }
            }

            return normalizedPath;
        }

        private static IEnumerable<string> GetCandidates(string path)
        {
            yield return path;

            string alternatePath = ToggleMaggyHelperSegment(path);
            if (!string.Equals(alternatePath, path, StringComparison.OrdinalIgnoreCase))
            {
                yield return alternatePath;
            }
        }

        private static string ToggleMaggyHelperSegment(string path)
        {
            string normalizedPath = NormalizePath(path);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return normalizedPath;
            }

            bool hasTrailingSlash = normalizedPath.EndsWith("/", StringComparison.Ordinal);
            string[] parts = normalizedPath.Trim('/').Split('/');
            if (parts.Length < 2 || !Array.Exists(SupportedRootFolders,
                    folder => string.Equals(folder, parts[0], StringComparison.OrdinalIgnoreCase)))
            {
                return normalizedPath;
            }

            List<string> updatedParts = new List<string>(parts);
            if (string.Equals(updatedParts[1], "KIRBY_CELESTE", StringComparison.OrdinalIgnoreCase))
            {
                updatedParts.RemoveAt(1);
            }
            else
            {
                updatedParts.Insert(1, "KIRBY_CELESTE");
            }

            string updatedPath = string.Join("/", updatedParts);
            if (hasTrailingSlash)
            {
                updatedPath += "/";
            }

            return updatedPath;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            bool hasTrailingSlash = path.EndsWith("/", StringComparison.Ordinal) || path.EndsWith("\\", StringComparison.Ordinal);
            string normalizedPath = path.Trim().Replace('\\', '/').Trim('/');
            if (hasTrailingSlash)
            {
                normalizedPath += "/";
            }

            return normalizedPath;
        }
    }

    public static class KIRBY_CELESTESaveFacade
    {
        private const string ChapterUnlockPrefix = "chapter_unlocked:";

        public static bool IsLoaded => SaveData.Instance != null;
        public static bool HasModSave => KIRBY_CELESTEModule.SaveData != null;

        public static int SelectedAreaId
        {
            get
            {
                object lastArea = GetMember(SaveData.Instance, "LastArea") ?? GetMember(SaveData.Instance, "lastArea");
                return GetIntMember(lastArea, "ID") ?? GetIntMember(lastArea, "id") ?? -1;
            }
        }

        public static void TrySelectArea(int areaId)
        {
            object save = SaveData.Instance;
            if (save == null || areaId < 0)
            {
                return;
            }

            object areaKey = CreateAreaKey(areaId);
            if (areaKey == null)
            {
                return;
            }

            SetMember(save, "LastArea", areaKey);
            SetMember(save, "lastArea", areaKey);
        }

        public static void UnlockChapter(string sid)
        {
            if (!string.IsNullOrWhiteSpace(sid))
            {
                KIRBY_CELESTEModule.SaveData?.UnlockedModes.Add(ChapterUnlockPrefix + sid);
            }
        }

        public static bool IsChapterUnlocked(string sid)
        {
            return !string.IsNullOrWhiteSpace(sid)
                && KIRBY_CELESTEModule.SaveData?.UnlockedModes.Contains(ChapterUnlockPrefix + sid) == true;
        }

        public static string BuildExtendedHeartId(string sid, int mode)
        {
            return $"{sid}_{AreaModeExtender.GetModeName(mode)}";
        }

        public static void TryRecordExtendedHeartGem(Session session)
        {
            if (session == null)
            {
                return;
            }

            string sid = AreaData.Get(session.Area)?.SID;
            if (!string.IsNullOrEmpty(sid))
            {
                KIRBY_CELESTEModule.SaveData?.CollectHeartGem(BuildExtendedHeartId(sid, (int)session.Area.Mode));
            }
        }

        public static bool HasHeartGem(Session session)
        {
            if (session == null)
            {
                return false;
            }

            AreaData area = AreaData.Get(session.Area);
            if (area?.SID == null)
            {
                return false;
            }

            int mode = (int)session.Area.Mode;
            if (mode >= AreaModeExtender.MODE_DSIDE)
            {
                return KIRBY_CELESTEModule.SaveData?.HasCollectedHeartGem(BuildExtendedHeartId(area.SID, mode)) == true;
            }

            return TryGetVanillaHeartGem(area.ID, mode);
        }

        public static int CountHeartsForChapter(int areaId)
        {
            AreaData area = AreaData.Get(areaId);
            if (area?.SID == null)
            {
                return 0;
            }

            int total = 0;
            int modeCount = area.Mode?.Length ?? 0;
            for (int mode = 0; mode < modeCount; mode++)
            {
                if (mode >= AreaModeExtender.MODE_DSIDE)
                {
                    if (KIRBY_CELESTEModule.SaveData?.HasCollectedHeartGem(BuildExtendedHeartId(area.SID, mode)) == true)
                    {
                        total++;
                    }
                }
                else if (TryGetVanillaHeartGem(areaId, mode))
                {
                    total++;
                }
            }

            return total;
        }

        private static bool TryGetVanillaHeartGem(int areaId, int mode)
        {
            object save = SaveData.Instance;
            if (save == null)
            {
                return false;
            }

            if (GetMember(save, "Areas") is not IList areas || areaId < 0 || areaId >= areas.Count)
            {
                return false;
            }

            object areaStats = areas[areaId];
            if (GetMember(areaStats, "Modes") is not IList modes || mode < 0 || mode >= modes.Count)
            {
                return false;
            }

            object modeStats = modes[mode];
            return GetBoolMember(modeStats, "HeartGem") ?? GetBoolMember(modeStats, "heartGem") ?? false;
        }

        private static object CreateAreaKey(int areaId)
        {
            ConstructorInfo ctor = typeof(AreaKey).GetConstructor(new[] { typeof(int) });
            if (ctor != null)
            {
                return ctor.Invoke(new object[] { areaId });
            }

            ctor = typeof(AreaKey).GetConstructor(new[] { typeof(int), typeof(AreaMode) });
            return ctor?.Invoke(new object[] { areaId, AreaMode.Normal });
        }

        private static object GetMember(object target, string name)
        {
            if (target == null)
            {
                return null;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();
            return type.GetProperty(name, Flags)?.GetValue(target) ?? type.GetField(name, Flags)?.GetValue(target);
        }

        private static void SetMember(object target, string name, object value)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, Flags);
            if (property?.CanWrite == true)
            {
                property.SetValue(target, value);
                return;
            }

            type.GetField(name, Flags)?.SetValue(target, value);
        }

        private static int? GetIntMember(object target, string name)
        {
            object value = GetMember(target, name);
            return value is int intValue ? intValue : null;
        }

        private static bool? GetBoolMember(object target, string name)
        {
            object value = GetMember(target, name);
            return value is bool boolValue ? boolValue : null;
        }
    }

    public static class KIRBY_CELESTEProgressionManager
    {
        public static void RefreshProgression()
        {
        }

        public static void RecordCheckpoint(Level level, Vector2 checkpoint, string checkpointId)
        {
            if (level?.Session == null)
            {
                return;
            }

            level.Session.RespawnPoint = checkpoint;
            if (!string.IsNullOrEmpty(checkpointId))
            {
                level.Session.SetFlag($"checkpoint_{checkpointId}", true);
                KIRBY_CELESTEModule.SaveData?.UnlockAchievement($"checkpoint:{level.Session.Area.SID}:{checkpointId}");
            }
        }

        public static void RecordPinkPlatinumBerry(Level level, string berryId)
        {
            if (level?.Session != null && !string.IsNullOrEmpty(berryId))
            {
                KIRBY_CELESTEModule.SaveData?.UnlockAchievement($"pink_platinum:{level.Session.Area.SID}:{berryId}");
            }
        }

        public static void RecordMiniHeart(Level level, string gemId)
        {
            if (level?.Session != null && !string.IsNullOrEmpty(gemId))
            {
                KIRBY_CELESTEModule.SaveData?.UnlockAchievement($"mini_heart:{level.Session.Area.SID}:{gemId}");
            }
        }

        public static void RecordCassette(Level level)
        {
            if (level?.Session != null)
            {
                KIRBY_CELESTEModule.SaveData?.UnlockAchievement($"cassette:{level.Session.Area.SID}");
            }
        }
    }
}

namespace Celeste.Mod.KIRBY_CELESTE
{
    public static class Util
    {
        public static Rectangle Rectangle(Vector2 from, Vector2 to) => global::Celeste.Utils.Util.Rectangle(from, to);
        public static float Mod(float value, float modulo) => global::Celeste.Utils.Util.Mod(value, modulo);
        public static Vector2 Min(Vector2 a, Vector2 b) => global::Celeste.Utils.Util.Min(a, b);
        public static Vector2 Max(Vector2 a, Vector2 b) => global::Celeste.Utils.Util.Max(a, b);
    }

    public static class CustomSFX
    {
        public const string game_dreamZipMover_move = "event:/game/01_forsaken_city/zip_mover";
        public const string game_dreamZipMover_return = "event:/game/01_forsaken_city/zip_mover_return";
        public const string game_dreamZipMover_tick = "event:/game/01_forsaken_city/zip_mover_tick";
        public const string game_dreamZipMover_finish = "event:/game/01_forsaken_city/zip_mover_finish";
        public const string game_dreamZipMover_impact = "event:/game/01_forsaken_city/zip_mover_impact";
    }
}

public static class Util
{
    public static Rectangle Rectangle(Vector2 from, Vector2 to) => global::Celeste.Utils.Util.Rectangle(from, to);
    public static float Mod(float value, float modulo) => global::Celeste.Utils.Util.Mod(value, modulo);
    public static Vector2 Min(Vector2 a, Vector2 b) => global::Celeste.Utils.Util.Min(a, b);
    public static Vector2 Max(Vector2 a, Vector2 b) => global::Celeste.Utils.Util.Max(a, b);
}

public static class CustomSFX
{
    public const string game_dreamZipMover_move = "event:/game/01_forsaken_city/zip_mover";
    public const string game_dreamZipMover_return = "event:/game/01_forsaken_city/zip_mover_return";
    public const string game_dreamZipMover_tick = "event:/game/01_forsaken_city/zip_mover_tick";
    public const string game_dreamZipMover_finish = "event:/game/01_forsaken_city/zip_mover_finish";
    public const string game_dreamZipMover_impact = "event:/game/01_forsaken_city/zip_mover_impact";
}

namespace Celeste.Utils
{

    public static class ColorUtils
    {
        public static Color[] GenerateRainbowPalette(int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Color>();
            }

            Color[] colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                colors[i] = FromHsv(i / (float)count * 360f, 1f, 1f);
            }

            return colors;
        }

        public static Color FromHsv(float hue, float saturation, float value)
        {
            hue = ((hue % 360f) + 360f) % 360f;
            saturation = Math.Clamp(saturation, 0f, 1f);
            value = Math.Clamp(value, 0f, 1f);

            if (saturation <= 0f)
            {
                return new Color(value, value, value);
            }

            float sector = hue / 60f;
            int index = (int)MathF.Floor(sector);
            float fraction = sector - index;
            float p = value * (1f - saturation);
            float q = value * (1f - saturation * fraction);
            float t = value * (1f - saturation * (1f - fraction));

            return index switch
            {
                0 => new Color(value, t, p),
                1 => new Color(q, value, p),
                2 => new Color(p, value, t),
                3 => new Color(p, q, value),
                4 => new Color(t, p, value),
                _ => new Color(value, p, q)
            };
        }
    }

    public static class TextureUtil
    {
        public static MTexture TryGetOverworldTexture(string path)
        {
            return GFX.Gui.Has(path) ? GFX.Gui[path] : null;
        }
    }
}

namespace Celeste.Extensions
{
    public static class CompatibilityExtensions
    {
        private sealed class TimeRateState
        {
            public float Value = 1f;
        }

        private static readonly ConditionalWeakTable<TimeRateModifier, TimeRateState> TimeRateStates = new();

        public static Vector2 CorrectJoystickPrecision(this Vector2 value)
        {
            return value;
        }

        public static void SetStateName(this StateMachine stateMachine, int state, string name)
        {
        }

        public static float CurrentTimeRate(this TimeRateModifier modifier)
        {
            return TimeRateStates.GetOrCreateValue(modifier).Value;
        }

        public static void SetTimeRateMultiplier(this TimeRateModifier modifier, float multiplier)
        {
            TimeRateStates.GetOrCreateValue(modifier).Value = multiplier;
#pragma warning disable CS0618 // Engine.TimeRate used intentionally for backward compatibility
            Engine.TimeRate = multiplier;
#pragma warning restore CS0618
        }

        public static void ResetTimeRateMultiplier(this TimeRateModifier modifier)
        {
            modifier.SetTimeRateMultiplier(1f);
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
