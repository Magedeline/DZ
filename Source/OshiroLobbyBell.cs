using MaggyHelper.NPCs;

namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/OshiroLobbyBell")]
    public class OshiroLobbyBell : Entity
    {
        private TalkComponent talker;

        public OshiroLobbyBell(Vector2 position)
            : base(position)
        {
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
            talker.Enabled = false;
        }

        private void OnTalk(global::Celeste.Player player) =>
            Audio.Play("event:/game/03_resort/deskbell_again", Vector2.Zero);

        public override void Update()
        {
            if (!talker.Enabled && Scene.Entities.FindFirst<NPC05_Oshiro_Lobby>() == null)
                talker.Enabled = true;
            base.Update();
        }
    }
}




