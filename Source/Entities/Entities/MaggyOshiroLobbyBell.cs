using Celeste.NPCs;

namespace Celeste.Entities
{
    [CustomEntity("DZ/MaggyOshiroLobbyBell")]
    public class MaggyOshiroLobbyBell : Entity
    {
        private TalkComponent talker;
        private string soundEffect;
        private bool startsActive;

        public MaggyOshiroLobbyBell(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            soundEffect = data.Attr("soundEffect", "event:/game/05_restore/deskbell_again");
            startsActive = data.Bool("isActive", false);
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
            talker.Enabled = startsActive;
        }

        public MaggyOshiroLobbyBell(Vector2 position)
            : base(position)
        {
            soundEffect = "event:/game/05_restore/deskbell_again";
            startsActive = false;
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), OnTalk));
            talker.Enabled = false;
        }

        private void OnTalk(global::Celeste.Player player) =>
            Audio.Play(soundEffect, Position);

        public override void Update()
        {
            if (!talker.Enabled && Scene.Entities.FindFirst<NPC05_Maggy_Oshiro_Lobby>() == null)
                talker.Enabled = true;
            base.Update();
        }
    }
}
