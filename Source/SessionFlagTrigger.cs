namespace MaggyHelper.Triggers
{
    [CustomEntity(IngesteConstants.EntityNames.SESSION_FLAG_TRIGGER, IngesteConstants.EntityNames.SAMPLE_TRIGGER)]
    [HotReloadable]
    public class SessionFlagTrigger : Trigger
    {
        private readonly string sessionFlag;
        private readonly bool flagState;
        private readonly bool triggerOnce;
        private readonly string requiredFlag;
        private readonly bool requiredFlagState;
        private readonly FlagAction flagAction;
        private readonly TriggerMode triggerMode;
        private bool hasTriggered;

        public SessionFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            int sampleProperty = data.Int(nameof(sampleProperty), 0);
            sessionFlag = data.Attr(nameof(sessionFlag), $"sample_trigger_{sampleProperty}");
            flagState = data.Bool(nameof(flagState), true);
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            requiredFlag = data.Attr(nameof(requiredFlag), string.Empty);
            requiredFlagState = data.Bool(nameof(requiredFlagState), true);
            flagAction = ParseFlagAction(data.Attr(nameof(flagAction), nameof(FlagAction.SetValue)));
            triggerMode = ParseTriggerMode(data.Attr(nameof(triggerMode), nameof(TriggerMode.OnEnter)));
            hasTriggered = false;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerMode == TriggerMode.OnEnter)
            {
                ApplyFlag();
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);

            if (triggerMode == TriggerMode.OnLeave)
            {
                ApplyFlag();
            }
        }

        private void ApplyFlag()
        {
            if (string.IsNullOrWhiteSpace(sessionFlag))
            {
                return;
            }

            if (triggerOnce && hasTriggered)
            {
                return;
            }

            Level level = SceneAs<Level>();
            if (level == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(requiredFlag) && level.Session.GetFlag(requiredFlag) != requiredFlagState)
            {
                return;
            }

            bool nextState = flagAction switch
            {
                FlagAction.Toggle => !level.Session.GetFlag(sessionFlag),
                _ => flagState
            };

            level.Session.SetFlag(sessionFlag, nextState);
            hasTriggered = true;
        }

        private static FlagAction ParseFlagAction(string value)
        {
            return Enum.TryParse(value, true, out FlagAction result) ? result : FlagAction.SetValue;
        }

        private static TriggerMode ParseTriggerMode(string value)
        {
            return Enum.TryParse(value, true, out TriggerMode result) ? result : TriggerMode.OnEnter;
        }

        private enum FlagAction
        {
            SetValue,
            Toggle
        }

        private enum TriggerMode
        {
            OnEnter,
            OnLeave
        }
    }
}