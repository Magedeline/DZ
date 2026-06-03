using System;
using Celeste.Entities.Bosses;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Drives the player's <see cref="PlayerSprite"/> when Kirby mode is active,
    /// routing vanilla animation IDs to Kirby-specific variants (float, inhale,
    /// copy-ability overrides, etc.).
    /// </summary>
    public class KirbyPlayerSpriteController : Component
    {
        private global::Celeste.Player player;
        private KirbyPlayerController kirbyCtrl;

        // Ability-specific idle/walk suffixes that the sprite bank defines
        private static readonly string[] AbilityPrefixes = new[]
        {
            "fire", "ice", "spark", "stone", "sword", "beam",
            "cutter", "hammer", "wing", "needle", "parasol", "wheel",
            "bomb", "fighter", "suplex", "ninja", "mirror", "ufo", "sleep"
        };

        // Current visual state
        private string currentAnimId;
        private bool forceOnce;

        public KirbyPlayerSpriteController() : base(true, false) { }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as global::Celeste.Player;
            kirbyCtrl = player?.Get<KirbyPlayerController>();
        }

        public override void Update()
        {
            if (player == null || !player.IsKirbyMode())
                return;

            string desired = ResolveDesiredAnimation();

            if (!string.IsNullOrEmpty(desired))
            {
                if (desired != currentAnimId || forceOnce)
                {
                    forceOnce = false;
                    PlayIfExists(desired);
                    currentAnimId = desired;
                }
            }
            else
            {
                currentAnimId = null;
            }
        }

        private string ResolveDesiredAnimation()
        {
            if (kirbyCtrl == null)
                return null;

            var sprite = player.Sprite;
            int state = player.StateMachine.State;
            bool onGround = player.OnGround();

            // ── Inhale takes highest priority ──
            if (kirbyCtrl.IsInhaling)
            {
                // If we just started inhaling, use begin anim; otherwise loop
                if (sprite.CurrentAnimationID != "inhalebegin" &&
                    sprite.CurrentAnimationID != "inhaleloop")
                {
                    forceOnce = true;
                    return "inhalebegin";
                }
                if (!sprite.Animating || sprite.CurrentAnimationID == "inhalebegin")
                {
                    return "inhaleloop";
                }
                return "inhaleloop";
            }

            // ── Float / Hover while airborne ──
            if (!onGround && state == global::Celeste.Player.StNormal)
            {
                if (Input.Jump.Check && player.Speed.Y > 0f)
                {
                    return "float";
                }
                if (Input.Jump.Check && Math.Abs(player.Speed.Y) <= 10f)
                {
                    return "hover";
                }
            }

            // ── Crouch / Duck override ──
            if (player.Ducking && onGround)
            {
                return "crouch";
            }

            // ── Copy-ability overrides for idle/walk ──
            var session = KIRBY_CELESTEModule.Session;
            CopyAbilityType ability = session?.CurrentCopyAbility ?? CopyAbilityType.None;
            string abilityPrefix = AbilityToPrefix(ability);

            if (!string.IsNullOrEmpty(abilityPrefix) && state == global::Celeste.Player.StNormal)
            {
                // If the sprite bank has ability-specific versions, prefer them
                if (onGround && player.Speed.X != 0f && sprite.Has(abilityPrefix + "_walk"))
                    return abilityPrefix + "_walk";
                if (onGround && player.Speed.X == 0f && sprite.Has(abilityPrefix + "_idle"))
                    return abilityPrefix + "_idle";
            }

            // ── Hurt / damage flash ──
            if (state == global::Celeste.Player.StHitSquash && sprite.Has("hurt"))
            {
                return "hurt";
            }

            return null; // let vanilla handle everything else
        }

        private bool PlayIfExists(string animId)
        {
            if (player?.Sprite == null)
                return false;
            var sprite = player.Sprite;
            if (!sprite.Has(animId))
                return false;

            // Only restart if we're not already playing this animation
            if (sprite.CurrentAnimationID == animId && sprite.Animating)
                return true;

            sprite.Play(animId);
            return true;
        }

        private static string AbilityToPrefix(CopyAbilityType ability)
        {
            return ability switch
            {
                CopyAbilityType.Fire    => "fire",
                CopyAbilityType.Ice     => "ice",
                CopyAbilityType.Spark   => "spark",
                CopyAbilityType.Stone   => "stone",
                CopyAbilityType.Sword   => "sword",
                CopyAbilityType.Beam    => "beam",
                CopyAbilityType.Cutter  => "cutter",
                CopyAbilityType.Hammer  => "hammer",
                CopyAbilityType.Wing    => "wing",
                CopyAbilityType.Needle  => "needle",
                CopyAbilityType.Parasol => "parasol",
                CopyAbilityType.Wheel   => "wheel",
                CopyAbilityType.Bomb    => "bomb",
                CopyAbilityType.Fighter => "fighter",
                CopyAbilityType.Suplex  => "suplex",
                CopyAbilityType.Ninja   => "ninja",
                CopyAbilityType.Mirror  => "mirror",
                CopyAbilityType.UFO     => "ufo",
                CopyAbilityType.Sleep   => "sleep",
                _ => null
            };
        }
    }
}
