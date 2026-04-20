using System;
using Microsoft.Xna.Framework;

namespace Celeste.Entities
{
    public enum ElsKnightCloneKind
    {
        Galacta,
        Morpho
    }

    public enum ElsKnightCloneState
    {
        Dormant,
        Intro,
        Hovering,
        Telegraphing,
        ExecutingAttack,
        Recovering,
        Defeated
    }

    public enum ElsKnightCloneAttack
    {
        DashSlash,
        CrescentVolley,
        RadialBurst,
        WarpStrike
    }

    public readonly struct ElsKnightCloneCombatProfile
    {
        public ElsKnightCloneCombatProfile(int maxHealth, float moveSpeed, float attackCooldown, float orbitRadius, float dashSpeed)
        {
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackCooldown = attackCooldown;
            OrbitRadius = orbitRadius;
            DashSpeed = dashSpeed;
        }

        public int MaxHealth { get; }

        public float MoveSpeed { get; }

        public float AttackCooldown { get; }

        public float OrbitRadius { get; }

        public float DashSpeed { get; }
    }

    public readonly struct ElsKnightCloneVisualProfile
    {
        public ElsKnightCloneVisualProfile(
            Color primaryColor,
            Color secondaryColor,
            Color accentColor,
            string spritePath,
            string idleAnimationPath,
            string moveAnimationPath,
            string chargeAnimationPath,
            string slashAnimationPath,
            string warpAnimationPath)
        {
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            AccentColor = accentColor;
            SpritePath = spritePath;
            IdleAnimationPath = idleAnimationPath;
            MoveAnimationPath = moveAnimationPath;
            ChargeAnimationPath = chargeAnimationPath;
            SlashAnimationPath = slashAnimationPath;
            WarpAnimationPath = warpAnimationPath;
        }

        public Color PrimaryColor { get; }

        public Color SecondaryColor { get; }

        public Color AccentColor { get; }

        public string SpritePath { get; }

        public string IdleAnimationPath { get; }

        public string MoveAnimationPath { get; }

        public string ChargeAnimationPath { get; }

        public string SlashAnimationPath { get; }

        public string WarpAnimationPath { get; }
    }

    public readonly struct ElsKnightCloneAttackProfile
    {
        public ElsKnightCloneAttackProfile(ElsKnightCloneAttack attack, float windup, float activeTime, float cooldown, int projectileCount = 0, float projectileSpeed = 0f)
        {
            Attack = attack;
            Windup = windup;
            ActiveTime = activeTime;
            Cooldown = cooldown;
            ProjectileCount = projectileCount;
            ProjectileSpeed = projectileSpeed;
        }

        public ElsKnightCloneAttack Attack { get; }

        public float Windup { get; }

        public float ActiveTime { get; }

        public float Cooldown { get; }

        public int ProjectileCount { get; }

        public float ProjectileSpeed { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ElsKnightCloneVariantAttribute : Attribute
    {
        public ElsKnightCloneVariantAttribute(ElsKnightCloneKind kind, string displayName, string defaultMusicEvent)
        {
            Kind = kind;
            DisplayName = displayName;
            DefaultMusicEvent = defaultMusicEvent;
        }

        public ElsKnightCloneKind Kind { get; }

        public string DisplayName { get; }

        public string DefaultMusicEvent { get; }
    }
}