using System;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that sets or toggles a session flag when the player enters or leaves.
    /// Supports conditional triggering based on another flag's state.
    /// </summary>
    [CustomEntity("DZ/SessionFlagTrigger")]
    [Tracked]
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
            // Read the session flag name directly without using sampleProperty
            sessionFlag = data.Attr(nameof(sessionFlag), "");
            flagState = data.Bool(nameof(flagState), true);
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            requiredFlag = data.Attr(nameof(requiredFlag), "");
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
            // Validate that a flag name was provided
            if (string.IsNullOrWhiteSpace(sessionFlag))
            {
                return;
            }

            // Check if we should only trigger once
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            Level level = SceneAs<Level>();
            if (level?.Session == null)
            {
                return;
            }

            // Check required flag condition if specified
            if (!string.IsNullOrEmpty(requiredFlag))
            {
                bool currentRequiredFlagState = level.Session.GetFlag(requiredFlag);
                if (currentRequiredFlagState != requiredFlagState)
                {
                    return;
                }
            }

            // Determine the next flag state based on the action
            bool nextState = flagAction switch
            {
                FlagAction.Toggle => !level.Session.GetFlag(sessionFlag),
                _ => flagState
            };

            // Apply the flag change
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