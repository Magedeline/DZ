using Celeste.Entities.Bosses;
using Microsoft.Xna.Framework;

namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/MorphoKnightClone")]
    [Tracked]
    [HotReloadable]
    [ElsKnightCloneVariant(ElsKnightCloneKind.Morpho, "Morpho Knight Clone", "event:/desolozantas/music/lvl15/morpho_knight")]
    public sealed class MorphoKnightClone : ElsKnightCloneBoss
    {
        private static readonly ElsKnightCloneCombatProfile DefaultCombat = new ElsKnightCloneCombatProfile(
            maxHealth: 10,
            moveSpeed: 142f,
            attackCooldown: 1.05f,
            orbitRadius: 66f,
            dashSpeed: 320f);

        private static readonly ElsKnightCloneVisualProfile DefaultVisuals = new ElsKnightCloneVisualProfile(
            new Color(180, 60, 220),
            new Color(255, 50, 150),
            new Color(255, 208, 144),
            "characters/els_true_final_boss/siamo_zero_morpho_knight_fake/",
            "swords",
            "swords",
            "vortex_summon",
            "double_side_slash",
            "vortex_strike");

        private static readonly ElsKnightCloneAttackProfile[] DefaultAttackPattern =
        {
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.WarpStrike, 0.35f, 0.45f, 1.1f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.CrescentVolley, 0.3f, 0.1f, 0.95f, projectileCount: 5, projectileSpeed: 220f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.DashSlash, 0.22f, 0.4f, 1f),
            new ElsKnightCloneAttackProfile(ElsKnightCloneAttack.RadialBurst, 0.45f, 0.1f, 1.25f, projectileCount: 10, projectileSpeed: 210f)
        };

        public MorphoKnightClone(EntityData data, Vector2 offset)
            : base(data, offset, DefaultCombat, DefaultVisuals, DefaultAttackPattern)
        {
        }

        public MorphoKnightClone(Vector2 position)
            : base(position, DefaultCombat, DefaultVisuals, DefaultAttackPattern)
        {
        }

        protected override CopyAbilityType CloneAbility => CopyAbilityType.Wing;

        protected override string SpriteBankEntryName => "morpho_knight_clone";
    }
}
