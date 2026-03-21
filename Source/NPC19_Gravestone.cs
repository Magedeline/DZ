using System.Runtime.CompilerServices;
using MaggyHelper.Cutscenes;
using MaggyHelper.Entities;

namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC19_Gravestone")]
    [Tracked(true)]
    public class NPC19_Gravestone : Entity
    {
        private const string Flag = "maddy_gravestone";

        private Vector2 boostTarget;

        private TalkComponent talk;

        private Sprite sprite;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NPC19_Gravestone(EntityData data, Vector2 offset)
            : base()
        {
            // set the entity position from the EntityData and provided offset
            Position = data.Position + offset;
            boostTarget = data.FirstNodeNullable(offset) ?? Vector2.Zero;
            
            // Add sprite component
            Add(sprite = GFX.SpriteBank.Create("gravestone"));
            sprite.Play("maddygrave00");
            
            Add(talk = new TalkComponent(new Rectangle(-24, -8, 32, 8), new Vector2(-0.5f, -20f), Interact));
            talk.PlayerMustBeFacing = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);
            var level = scene as global::Celeste.Level;
            if (level != null && level.Session.GetFlag(Flag))
            {
                level.Add(new CustomCharaBoost(new Vector2[1] { boostTarget }, lockCamera: false));
                talk.RemoveSelf();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Interact(global::Celeste.Player player)
        {
            var level = Scene as global::Celeste.Level;
            if (level != null)
                level.Session.SetFlag(Flag);
            base.Scene.Add(new CS19_GravestoneSevenBirds(player, this, boostTarget));
            talk.Enabled = false;
        }
    }
}




