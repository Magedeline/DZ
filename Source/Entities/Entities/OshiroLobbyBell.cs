using Celeste.Cutscenes;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity("DZ/OshiroLobbyBell")]
    public class OshiroLobbyBell : Entity
    {
        private TalkComponent talker;
        private string soundEffect;
        private bool isInteracting;

        public OshiroLobbyBell(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            soundEffect = data.Attr("soundEffect", "event:/game/03_resort/deskbell_again");
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
            talker.Enabled = false;
        }

        public OshiroLobbyBell(Vector2 position)
            : base(position)
        {
            soundEffect = "event:/game/03_resort/deskbell_again";
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
            talker.Enabled = false;
        }

        private void OnTalk(global::Celeste.Player player)
        {
            NPC05_Oshiro_Lobby npc = Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>();
            if (npc != null)
            {
                // Delegate to the NPC so both the talker and the bell share the same cutscene logic.
                npc.TriggerCutscene(player);
                isInteracting = true;
                talker.Enabled = false;
            }
            else
            {
                // Fallback: NPC is already gone, just ring the bell.
                Audio.Play(soundEffect, Position);
            }
        }

        public override void Update()
        {
            // Enable the talker once the NPC has left (either removed or never present).
            if (!talker.Enabled && !isInteracting && Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>() == null)
                talker.Enabled = true;

            // Reset interacting state once the talker is re-enabled externally.
            if (isInteracting && talker.Enabled)
                isInteracting = false;

            base.Update();
        }
    }
}
