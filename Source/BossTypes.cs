using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities.Bosses
{
    /// <summary>
    /// Interface for entities that can provide a copy ability when inhaled by Kirby.
    /// </summary>
    public interface IKirbyCopySource
    {
        CopyAbilityType GetCopyAbility();
    }

    /// <summary>
    /// Types of copy abilities that Kirby can obtain from enemies and bosses.
    /// </summary>
    public enum CopyAbilityType
    {
        None,
        Fire,
        Ice,
        Spark,
        Sword,
        Cutter,
        Beam,
        Stone,
        Needle,
        Parasol,
        Wheel,
        Bomb,
        Fighter,
        Suplex,
        Ninja,
        Mirror,
        Hammer,
        Wing,
        UFO,
        Sleep
    }

    /// <summary>
    /// Collectible star that grants a copy ability when touched.
    /// </summary>
    public class AbilityStar : Entity
    {
        private CopyAbilityType ability;
        private Sprite sprite;

        public AbilityStar(Vector2 position, CopyAbilityType ability) : base(position)
        {
            this.ability = ability;
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Depth = -100;

            Add(new PlayerCollider(OnPlayer));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Try to load a sprite; fall back gracefully if sprite bank entry doesn't exist
            try
            {
                sprite = GFX.SpriteBank.Create("MaggyHelper_AbilityStar");
                sprite.Play("idle");
                Add(sprite);
            }
            catch
            {
                // No sprite available – entity will still function
            }
        }

        private void OnPlayer(Player player)
        {
            // Grant the ability via session
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.CurrentCopyAbility = ability;
                session.CurrentKirbyPower = ability.ToString();
            }

            Audio.Play("event:/game/general/diamond_touch", Position);
            RemoveSelf();
        }
    }
}
