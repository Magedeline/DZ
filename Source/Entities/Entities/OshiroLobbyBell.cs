using Celeste.Cutscenes;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    [Tracked(true)]
    [CustomEntity("DZ/OshiroLobbyBell")]
    public class OshiroLobbyBell : Entity
    {
        private TalkComponent talker;
        private string soundEffect;

        private bool isInteracting;

        public OshiroLobbyBell(Vector2 position)
            : base(position)
        {
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -24f), OnTalk));
        }

        public OshiroLobbyBell(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            soundEffect = data.Attr("soundEffect", "event:/game/03_resort/deskbell_again");
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
        }

        private void OnTalk(CelestePlayer player)
        {
            if (isInteracting) return;
            isInteracting = true;
            talker.Enabled = false;
            Audio.Play(soundEffect ?? "event:/game/03_resort/deskbell_again", Position);
            var target = base.Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>();
            target?.TriggerCutscene(player);
        }

        public override void Update()
        {
            if (isInteracting)
            {
                var npc = base.Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>();
                if (npc == null || !npc.IsInteracting)
                {
                    isInteracting = false;
                    talker.Enabled = true;
                }
            }
            base.Update();
        }
    }
}
