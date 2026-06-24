using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    // =============================================
    // AbilitySwapTrigger - Grants/removes Kirby abilities
    // =============================================
    [CustomEntity("MaggyHelper/AbilitySwapTrigger")]
    public class AbilitySwapTrigger : Trigger
    {
        private string ability;
        private string action; // "give", "remove", "swap"
        private bool onlyOnce;
        private bool triggered = false;

        public AbilitySwapTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            ability = data.Attr("ability", "Fire");
            action = data.Attr("action", "give");
            onlyOnce = data.Bool("onlyOnce", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered && onlyOnce) return;
            triggered = true;

            Level level = SceneAs<Level>();
            switch (action.ToLower())
            {
                case "give":
                    level.Session.SetFlag("kirby_ability_" + ability.ToLower(), true);
                    break;
                case "remove":
                    level.Session.SetFlag("kirby_ability_" + ability.ToLower(), false);
                    break;
                case "swap":
                    bool has = level.Session.GetFlag("kirby_ability_" + ability.ToLower());
                    level.Session.SetFlag("kirby_ability_" + ability.ToLower(), !has);
                    break;
            }
            Audio.Play("event:/game/general/seed_touch", Position);
        }
    }

    // =============================================
    // GravityZoneTrigger - Changes gravity direction
    // =============================================
    [CustomEntity("MaggyHelper/GravityZoneTrigger")]
    public class GravityZoneTrigger : Trigger
    {
        private string gravityDirection;
        private float gravityStrength;

        public GravityZoneTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            gravityDirection = data.Attr("direction", "up");
            gravityStrength = data.Float("strength", 1f);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            Level level = SceneAs<Level>();

            level.Session.SetFlag("gravity_normal", false);
            level.Session.SetFlag("gravity_up", false);
            level.Session.SetFlag("gravity_left", false);
            level.Session.SetFlag("gravity_right", false);
            level.Session.SetFlag("gravity_" + gravityDirection, true);

            switch (gravityDirection.ToLower())
            {
                case "up":
                    player.Speed.Y -= 900f * gravityStrength * Engine.DeltaTime;
                    break;
                case "left":
                    player.Speed.X -= 400f * gravityStrength * Engine.DeltaTime;
                    break;
                case "right":
                    player.Speed.X += 400f * gravityStrength * Engine.DeltaTime;
                    break;
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            level.Session.SetFlag("gravity_up", false);
            level.Session.SetFlag("gravity_left", false);
            level.Session.SetFlag("gravity_right", false);
            level.Session.SetFlag("gravity_normal", true);
        }
    }

    // =============================================
    // SpeedModifierTrigger - Changes player speed
    // =============================================
    [CustomEntity("MaggyHelper/SpeedModifierTrigger")]
    public class SpeedModifierTrigger : Trigger
    {
        private float speedMultiplier;

        public SpeedModifierTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            speedMultiplier = data.Float("speedMultiplier", 0.5f);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            player.Speed *= speedMultiplier;
        }
    }

    // =============================================
    // InvincibilityTrigger - Grants temporary invincibility
    // =============================================
    [CustomEntity("MaggyHelper/InvincibilityTrigger")]
    public class InvincibilityTrigger : Trigger
    {
        private float duration;
        private bool onlyOnce;
        private bool triggered = false;

        public InvincibilityTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            duration = data.Float("duration", 5f);
            onlyOnce = data.Bool("onlyOnce", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered && onlyOnce) return;
            triggered = true;

            Level level = SceneAs<Level>();
            level.Session.SetFlag("star_power_active", true);
            Audio.Play("event:/game/general/seed_touch", Position);
            Add(new Coroutine(InvincibilityRoutine()));
        }

        private IEnumerator InvincibilityRoutine()
        {
            yield return duration;
            SceneAs<Level>().Session.SetFlag("star_power_active", false);
        }
    }

    // =============================================
    // SizeChangeTrigger - Shrinks or grows player
    // =============================================
    [CustomEntity("MaggyHelper/SizeChangeTrigger")]
    public class SizeChangeTrigger : Trigger
    {
        private string sizeMode; // "small", "large", "normal"
        private bool persistOnExit;

        public SizeChangeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            sizeMode = data.Attr("sizeMode", "small");
            persistOnExit = data.Bool("persistOnExit", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            level.Session.SetFlag("size_small", sizeMode == "small");
            level.Session.SetFlag("size_large", sizeMode == "large");
            Audio.Play("event:/game/general/spring", Position);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (!persistOnExit)
            {
                Level level = SceneAs<Level>();
                level.Session.SetFlag("size_small", false);
                level.Session.SetFlag("size_large", false);
            }
        }
    }

    // =============================================
    // DisableAbilityTrigger - Disables specific abilities
    // =============================================
    [CustomEntity("MaggyHelper/DisableAbilityTrigger")]
    public class DisableAbilityTrigger : Trigger
    {
        private bool disableDash;
        private bool disableClimb;
        private bool disableFloat;
        private bool disableInhale;
        private bool disableGrab;

        public DisableAbilityTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            disableDash = data.Bool("disableDash", false);
            disableClimb = data.Bool("disableClimb", false);
            disableFloat = data.Bool("disableFloat", false);
            disableInhale = data.Bool("disableInhale", false);
            disableGrab = data.Bool("disableGrab", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            if (disableDash) level.Session.SetFlag("disable_dash", true);
            if (disableClimb) level.Session.SetFlag("disable_climb", true);
            if (disableFloat) level.Session.SetFlag("disable_float", true);
            if (disableInhale) level.Session.SetFlag("disable_inhale", true);
            if (disableGrab) level.Session.SetFlag("disable_grab", true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            level.Session.SetFlag("disable_dash", false);
            level.Session.SetFlag("disable_climb", false);
            level.Session.SetFlag("disable_float", false);
            level.Session.SetFlag("disable_inhale", false);
            level.Session.SetFlag("disable_grab", false);
        }
    }

    // =============================================
    // StaminaModTrigger - Modifies climb stamina
    // =============================================
    [CustomEntity("MaggyHelper/StaminaModTrigger")]
    public class StaminaModTrigger : Trigger
    {
        private string mode; // "infinite", "reduced", "boosted"
        private float multiplier;

        public StaminaModTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            mode = data.Attr("mode", "infinite");
            multiplier = data.Float("multiplier", 2f);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            switch (mode.ToLower())
            {
                case "infinite":
                    player.Stamina = Player.ClimbMaxStamina;
                    break;
                case "boosted":
                    player.Stamina = Math.Min(player.Stamina * multiplier, Player.ClimbMaxStamina * multiplier);
                    break;
                case "reduced":
                    player.Stamina = Math.Min(player.Stamina, Player.ClimbMaxStamina * multiplier);
                    break;
            }
        }
    }

    // =============================================
    // MirrorModeTrigger - Flips controls
    // =============================================
    [CustomEntity("MaggyHelper/MirrorModeTrigger")]
    public class MirrorModeTrigger : Trigger
    {
        private bool flipHorizontal;
        private bool flipVertical;

        public MirrorModeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flipHorizontal = data.Bool("flipHorizontal", true);
            flipVertical = data.Bool("flipVertical", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            level.Session.SetFlag("mirror_horizontal", flipHorizontal);
            level.Session.SetFlag("mirror_vertical", flipVertical);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            level.Session.SetFlag("mirror_horizontal", false);
            level.Session.SetFlag("mirror_vertical", false);
        }
    }

    // =============================================
    // OneHitTrigger - One hit kills while inside
    // =============================================
    [CustomEntity("MaggyHelper/OneHitTrigger")]
    public class OneHitTrigger : Trigger
    {
        public OneHitTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Session.SetFlag("one_hit_mode", true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SceneAs<Level>().Session.SetFlag("one_hit_mode", false);
        }
    }

    // =============================================
    // DashRefreshTrigger - Infinite dashes in zone
    // =============================================
    [CustomEntity("MaggyHelper/DashRefreshTrigger")]
    public class DashRefreshTrigger : Trigger
    {
        public DashRefreshTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            player.RefillDash();
        }
    }
}

