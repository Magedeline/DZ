namespace MaggyHelper
{
    internal sealed class GameplayPredictor
    {
        public void LoadModel(string path) { }
    }

    internal sealed class KirbyBehaviorPredictor
    {
        public void LoadModel(string path) { }
        public string SuggestAbility(float speed, float jumpHeight) => string.Empty;
    }

    internal static class GameplayLogger
    {
        public static void Log(float speed, float jumpHeight, float deaths, bool levelComplete) { }
    }

    internal static class KirbyLogger
    {
        public static void Log(float speed, float jumpHeight, int abilityUsed, bool levelComplete) { }
    }

    internal static class IngesteConstants
    {
        internal static class EntityNames
        {
            public const string ANCIENT_SWITCH = "MaggyHelper/AncientSwitch";
            public const string SESSION_FLAG_TRIGGER = "MaggyHelper/SessionFlagTrigger";
            public const string SAMPLE_TRIGGER = "MaggyHelper/SampleTrigger";
        }
    }
}

namespace MaggyHelper.Utils
{
    internal sealed class Pcg32Random : Random
    {
        public Pcg32Random(uint seed) : base(unchecked((int)seed)) { }
    }

    public struct Cooldown
    {
        private readonly float duration;
        public float Progress;
        private float remaining;

        public Cooldown(float duration, bool startReady = false)
        {
            this.duration = duration;
            remaining = startReady ? 0f : duration;
        }

        public bool Update(float deltaTime)
        {
            if (remaining <= 0f)
            {
                Progress = 1f;
                return true;
            }

            remaining -= deltaTime;
            if (remaining <= 0f)
            {
                remaining = 0f;
                Progress = 1f;
                return true;
            }

            Progress = duration <= 0f ? 1f : 1f - remaining / duration;

            return false;
        }
    }
}
