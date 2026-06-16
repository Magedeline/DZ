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

        private void OnTalk(global::Celeste.Player player) => Audio.Play(soundEffect, Position);

        public override void Update()
        {
            if (!talker.Enabled && Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>() == null)
                talker.Enabled = true;
            base.Update();
        }
    }
}
