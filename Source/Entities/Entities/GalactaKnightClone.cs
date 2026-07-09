using Celeste.Entities.Bosses;
using Microsoft.Xna.Framework;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/GalactaKnightClone,DZ/GalacticKnightClone")]
    [Tracked]
    [HotReloadable]
    [ElsKnightCloneVariant(ElsKnightCloneKind.Galacta, "Galacta Knight Clone", "event:/DZ/music/lvl18/galacta_knight")]
    public class GalactaKnightClone : ElsKnightCloneBoss
    {
        private static readonly ElsKnightCloneCombatProfile DefaultCombat = new ElsKnightCloneCombatProfile(
            maxHealth: 12,
            moveSpeed: 150f,
            attackCooldown: 1.15f,
            orbitRadius: 74f,
            dashSpeed: 340f);

        private static readonly ElsKnightCloneVisualProfile DefaultVisuals = new ElsKnightCloneVisualProfile(
            new Color(255, 220, 128),
            new Color(100, 255, 255),
            new Color(255, 248, 220),
            "characters/DZ/els_true_final_boss/siamo_zero_aeon_hero_fake/",
            "idle",
            "move",
            "awaken",
            "rapid_slash",
            "fly_start");

        private static readonly ElsKnightCloneAttackProfile[] DefaultAttackPattern =
        {
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.CrescentVolley, 0.35f, 0.1f, 1f, projectileCount: 3, projectileSpeed: 260f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.DashSlash, 0.25f, 0.45f, 1.1f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.WarpStrike, 0.4f, 0.4f, 1.25f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.RadialBurst, 0.5f, 0.1f, 1.35f, projectileCount: 8, projectileSpeed: 240f)
        };

        public GalactaKnightClone(EntityData data, Vector2 offset)
            : base(data, offset, DefaultCombat, DefaultVisuals, DefaultAttackPattern)
        {
        }

        public GalactaKnightClone(Vector2 position)
            : base(position, DefaultCombat, DefaultVisuals, DefaultAttackPattern)
        {
        }

        protected override CopyAbilityType CloneAbility => CopyAbilityType.Sword;

        protected override string SpriteBankEntryName => "galacta_knight_clone";
    }
}
