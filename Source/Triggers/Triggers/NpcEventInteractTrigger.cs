using System;
using System.Collections;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// A zone-based trigger version of NpcEventInteract. Supports triggering automatically
    /// when the player enters the trigger area (OnEnter), or requiring the player to press
    /// the Talk/Interact button while inside the trigger area (OnInteract), just like an NPC.
    /// </summary>
    [CustomEntity(ids: "DZ/NpcEventInteractTrigger")]
    [Tracked]
    [HotReloadable]
    public class NpcEventInteractTrigger : Trigger
    {
        private enum InteractMode
        {
            OnEnter,
            OnInteract
        }

        public TalkComponent Talker;

        private readonly InteractMode mode;
        private readonly string dialogKey;
        private readonly string csFlag;
        private readonly string triggerFlag;
        private readonly string requireFlag;
        private readonly bool onlyOnce;
        private readonly string npcName;

        private bool hasTriggered;
        private bool isCurrentlyInteracting;

        public NpcEventInteractTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            npcName = data.Attr("npcName", "NPC");
            dialogKey = data.Attr("dialogKey", "");
            csFlag = data.Attr("csFlag", "");
            triggerFlag = data.Attr("triggerFlag", "");
            requireFlag = data.Attr("requireFlag", "");
            onlyOnce = data.Bool("onlyOnce", true);

            mode = ParseMode(data.Attr("mode", nameof(InteractMode.OnEnter)));

            if (mode == InteractMode.OnInteract)
            {
                Add(Talker = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), new Vector2(Width / 2f, 0f), OnTalk));
                Talker.PlayerMustBeFacing = false;
            }
        }

        private static InteractMode ParseMode(string value)
        {
            return Enum.TryParse(value, true, out InteractMode result) ? result : InteractMode.OnEnter;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                if (!string.IsNullOrEmpty(requireFlag) && !level.Session.GetFlag(requireFlag))
                {
                    if (Talker != null)
                    {
                        Talker.Enabled = false;
                    }

                    Collidable = false;
                    return;
                }

                if (onlyOnce && !string.IsNullOrEmpty(csFlag) && level.Session.GetFlag(csFlag))
                {
                    hasTriggered = true;

                    if (Talker != null)
                    {
                        Talker.Enabled = false;
                    }

                    Collidable = false;
                }
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (mode != InteractMode.OnEnter)
            {
                return;
            }

            if (!CanTrigger())
            {
                return;
            }

            StartInteraction(SceneAs<Level>(), player);
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (mode != InteractMode.OnInteract || !CanTrigger())
            {
                return;
            }

            StartInteraction(SceneAs<Level>(), player);
        }

        private bool CanTrigger()
        {
            if (isCurrentlyInteracting)
            {
                return false;
            }

            if (onlyOnce && hasTriggered)
            {
                return false;
            }

            Level level = SceneAs<Level>();

            if (level == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(requireFlag) && !level.Session.GetFlag(requireFlag))
            {
                return false;
            }

            return true;
        }

        private void StartInteraction(Level level, global::Celeste.Player player)
        {
            isCurrentlyInteracting = true;

            Logger.Log(LogLevel.Info, nameof(NpcEventInteractTrigger),
                $"Triggering {npcName} (mode: {mode})");

            level.StartCutscene(EndInteraction);
            Add(new Coroutine(InteractionRoutine(level, player)));
        }

        private IEnumerator InteractionRoutine(Level level, global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            string key = !string.IsNullOrEmpty(dialogKey) ? dialogKey : $"DEFAULT_NPC_DIALOG_{npcName.ToUpper()}";
            yield return Textbox.Say(key);

            EndInteraction(level);
        }

        private void EndInteraction(Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();

            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            level.EndCutscene();

            if (!string.IsNullOrEmpty(csFlag))
            {
                level.Session.SetFlag(csFlag, true);
            }

            if (!string.IsNullOrEmpty(triggerFlag))
            {
                level.Session.SetFlag(triggerFlag, true);
            }

            hasTriggered = true;
            isCurrentlyInteracting = false;

            if (onlyOnce)
            {
                if (Talker != null)
                {
                    Talker.Enabled = false;
                }

                Collidable = false;
            }
        }
    }
}
