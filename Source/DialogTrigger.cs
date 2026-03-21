namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that displays dialog with NPC support
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DialogTrigger")]
    [Tracked]
    public class DialogTrigger : Trigger
    {
        private string dialogKey;
        private bool triggerOnce;
        private bool requireInteraction;
        private string npcName;
        private bool hasTriggered;

        public DialogTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            dialogKey = data.Attr(nameof(dialogKey), "DIALOG_DEFAULT");
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            requireInteraction = data.Bool(nameof(requireInteraction), false);
            npcName = data.Attr(nameof(npcName), "");
            hasTriggered = false;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!requireInteraction && (!triggerOnce || !hasTriggered))
            {
                triggerDialog();
            }
        }

        public override void OnStay(global::Celeste.Player player)
        {
            base.OnStay(player);

            if (requireInteraction && Input.Talk.Pressed && (!triggerOnce || !hasTriggered))
            {
                triggerDialog();
            }
        }

        private void triggerDialog()
        {
            hasTriggered = true;

            var level = Scene as Level;
            if (level != null)
            {
                // Create dialog with optional NPC name
                string finalDialogKey = dialogKey;
                if (!string.IsNullOrEmpty(npcName))
                {
                    // You could extend this to show NPC portraits or special formatting
                    finalDialogKey = $"{npcName}_{dialogKey}";
                }

                level.StartCutscene((Player) => showDialog(finalDialogKey));
            }
        }

        private IEnumerator showDialog(string dialogKey)
        {
            yield return Textbox.Say(dialogKey);
        }
    }
}




