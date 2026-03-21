using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that damages Kirby when entered.
    /// Can be used for hazards, spikes, enemies, etc.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KirbyDamageTrigger")]
    public class KirbyDamageTrigger : Trigger
    {
        #region Fields

        private int damage;
        private float cooldown;
        private bool oncePerPlayer;
        private bool hasTriggered;
        private float cooldownTimer;

        #endregion

        #region Constructor

        public KirbyDamageTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            damage = data.Int("damage", 1);
            cooldown = data.Float("cooldown", 0.5f);
            oncePerPlayer = data.Bool("oncePerPlayer", false);
        }

        #endregion

        #region Lifecycle

        public override void Update()
        {
            base.Update();

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (oncePerPlayer && hasTriggered)
                return;

            if (cooldownTimer > 0f)
                return;

            // Route damage through shared runtime helpers so extension/legacy implementations stay in sync.
            if (player.TryDamageKirby(damage, Center))
            {
                cooldownTimer = cooldown;
                hasTriggered = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Entity that continuously damages Kirby while in contact.
    /// Similar to spikes but specifically for Kirby mode.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KirbyHazard")]
    [Tracked]
    public class KirbyHazard : Entity
    {
        #region Fields

        private int damage;
        private float damageInterval;
        private float damageTimer;
        private Sprite sprite;

        #endregion

        #region Constructor

        public KirbyHazard(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            damage = data.Int("damage", 1);
            damageInterval = data.Float("damageInterval", 0.5f);

            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.FGTerrain;
        }

        #endregion

        #region Lifecycle

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Try to add a sprite
            try
            {
                sprite = GFX.SpriteBank.Create("kirby_hazard");
                sprite.Play("idle");
                Add(sprite);
            }
            catch
            {
                // No sprite available
            }
        }

        public override void Update()
        {
            base.Update();

            if (damageTimer > 0f)
            {
                damageTimer -= Engine.DeltaTime;
                return;
            }

            // Also check regular player in Kirby mode
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player) && player.TryDamageKirby(damage, Center))
            {
                damageTimer = damageInterval;
            }
        }

        public override void Render()
        {
            base.Render();

            // Draw debug visualization if no sprite
            if (sprite == null)
            {
                Draw.Rect(Collider, Color.Red * 0.3f);
                Draw.HollowRect(Collider, Color.Red);
            }
        }

        #endregion
    }
}
