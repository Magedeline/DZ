using System;
using Monocle;
using Microsoft.Xna.Framework;

/// <summary>
/// Vanilla Madeline player character - standard Celeste mechanics without Kirby abilities.
/// This is the "normal" player for levels that don't use special Kirby mechanics.
/// 
/// For now, this is a minimal placeholder that shares core mechanics with the Kirby player
/// but disables all Kirby-specific features (abilities, combat, inhale, etc.).
/// </summary>
namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/MadelinePlayer")]
    [Tracked(true)]
    [HotReloadable]
    public class MadelinePlayer : KirbyPlayerExtension
    {
        private int maxHealth = 1;
        private int currentHealth = 1;
        private Level level;
        public bool Dead { get; private set; } = false;

        /// <summary>
        /// EntityData constructor - called by Celeste when loading from a map file.
        /// </summary>
        public MadelinePlayer(EntityData data, Vector2 offset)
            : this(data.Position + offset, (PlayerSpriteMode)data.Enum("spriteMode", PlayerSpriteMode.Madeline))
        {
        }

        public MadelinePlayer(Vector2 position, PlayerSpriteMode spriteMode)
            : base(new Vector2((int)position.X, (int)position.Y))
        {
            Tag = Tags.Persistent;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            PlayerSelectionManager.GetOrCreate(level);
            PlayerHealthManager.GetOrCreate(level, maxHealth);
        }

        public override void Update()
        {
            base.Update();
            if (Dead) return;
        }

        public override bool IsDead => Dead;
        public override bool IsHovering => false;
        public override float CurrentStamina { get => 110f; set { } }
        public override float MaxStamina => 110f;
        public override int CurrentHealth => currentHealth;
        public override int MaxHealth => maxHealth;

        public override void Heal(int amount)
        {
            currentHealth = Math.Min(currentHealth + amount, maxHealth);
        }

        public override void SetPowerState(KirbyMode.KirbyPowerState powerState)
        {
        }
    }
}
