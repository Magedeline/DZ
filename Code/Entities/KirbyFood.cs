using System;
using Celeste.Entities;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Items
{
    /// <summary>
    /// Base class for Kirby-style food items that heal the player
    /// </summary>
    public abstract class KirbyFood : Entity
    {
        #region Fields

        protected Sprite sprite;
        protected float bounceOffset;
        protected float bounceTimer;
        protected float healAmount;
        protected bool isCollected;
        protected ParticleType sparkleParticle;

        // Sound effect paths
        protected const string SFX_COLLECT = "event:/game/general/thing_booped";
        protected const string SFX_HEAL = "event:/char/pusheen/kirby/heal";

        #endregion

        #region Properties

        public abstract int HealAmount { get; }
        public abstract string SpritePath { get; }

        #endregion

        #region Constructor

        public KirbyFood(Vector2 position) : base(position)
        {
            healAmount = HealAmount;
            Collider = new Hitbox(12f, 12f, -6f, -12f);
            Depth = -50;
            isCollected = false;

            // Setup sprite
            Add(sprite = new Sprite(GFX.Game, SpritePath));
            SetupAnimations();
            sprite.Play("idle");

            // Setup sparkle particles
            sparkleParticle = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = Color.Yellow,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Size = 0.8f,
                SpeedMin = 10f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f
            };

            // Add player collider
            Add(new PlayerCollider(OnPlayerCollect));
        }

        protected virtual void SetupAnimations()
        {
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Add("collect", "collect", 0.05f);
        }

        #endregion

        #region Lifecycle

        public override void Update()
        {
            base.Update();

            if (isCollected) return;

            // Bounce animation
            bounceTimer += Engine.DeltaTime * 3f;
            bounceOffset = (float)Math.Sin(bounceTimer) * 2f;
            sprite.Position = new Vector2(0, bounceOffset);

            // Emit sparkles occasionally
            if (Scene.OnInterval(0.5f) && Calc.Random.Chance(0.3f))
            {
                var level = Scene as Level;
                level?.ParticlesFG.Emit(sparkleParticle, Position + new Vector2(Calc.Random.Range(-8f, 8f), Calc.Random.Range(-8f, 8f)));
            }
        }

        #endregion

        #region Collection

        private void OnPlayerCollect(global::Celeste.Player player)
        {
            if (isCollected) return;

            isCollected = true;
            Collect(player);
        }

        protected virtual void Collect(global::Celeste.Player player)
        {
            // Play sounds
            Audio.Play(SFX_COLLECT, Position);
            Audio.Play(SFX_HEAL, Position);

            // Heal player
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.Heal(HealAmount);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(-HealAmount, Position); // Negative damage = heal
                }
            }

            // Visual effects
            var level = Scene as Level;
            if (level != null)
            {
                // Sparkle burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = (i / 12f) * (float)Math.PI * 2f;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level.ParticlesFG.Emit(sparkleParticle, Position + dir * 8f);
                }

                // Heal number popup
                level.Add(new HealNumberPopup(Position, HealAmount));
            }

            // Animation and removal
            sprite.Play("collect");
            Add(new Coroutine(RemoveAfterAnimation()));
        }

        protected IEnumerator RemoveAfterAnimation()
        {
            yield return 0.3f;
            RemoveSelf();
        }

        #endregion

        #region Render

        public override void Render()
        {
            // Draw outline/shadow
            sprite.DrawOutline();
            base.Render();
        }

        #endregion
    }

    /// <summary>
    /// Heal number popup that shows how much health was restored
    /// </summary>
    public class HealNumberPopup : Entity
    {
        private int amount;
        private float timer;
        private float startY;

        public HealNumberPopup(Vector2 position, int healAmount) : base(position)
        {
            amount = healAmount;
            startY = position.Y;
            timer = 0f;
            Depth = -1000;
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            Position.Y = startY - timer * 20f;

            if (timer > 1f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            string text = $"+{amount}";
            Color color = Color.Lerp(Color.Green, Color.White, timer);
            float alpha = 1f - (timer * timer);

            ActiveFont.DrawOutline(text, Position, new Vector2(0.5f, 0.5f), Vector2.One * 0.6f, color * alpha, 2f, Color.Black * alpha);
        }
    }

    #region Specific Food Items

    /// <summary>
    /// Cherry - Heals 1 HP
    /// </summary>
    [CustomEntity("MaggyHelper/Cherry")]
    [Tracked]
    public class Cherry : KirbyFood
    {
        public override int HealAmount => 1;
        public override string SpritePath => "items/food/cherry/";

        public Cherry(EntityData data, Vector2 offset) : base(data.Position + offset) { }
    }

    /// <summary>
    /// Energy Drink - Heals 2 HP
    /// </summary>
    [CustomEntity("MaggyHelper/EnergyDrink")]
    [Tracked]
    public class EnergyDrink : KirbyFood
    {
        public override int HealAmount => 2;
        public override string SpritePath => "items/food/energydrink/";

        public EnergyDrink(EntityData data, Vector2 offset) : base(data.Position + offset) { }
    }

    /// <summary>
    /// Maxim Tomato - Heals all HP (full heal)
    /// </summary>
    [CustomEntity("MaggyHelper/MaximTomato")]
    [Tracked]
    public class MaximTomato : KirbyFood
    {
        public override int HealAmount => 999; // Will be clamped to max health
        public override string SpritePath => "items/food/maximtomato/";

        private float rotateTimer;

        public MaximTomato(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            // Maxim Tomatoes are more valuable - make them sparkle more
            sparkleParticle.Color = Color.Red;
            sparkleParticle.Color2 = Color.Yellow;
        }

        protected override void SetupAnimations()
        {
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Add("collect", "collect", 0.04f);
        }

        public override void Update()
        {
            base.Update();

            if (isCollected) return;

            // Rotate slowly
            rotateTimer += Engine.DeltaTime;
            sprite.Rotation = (float)Math.Sin(rotateTimer) * 0.1f;

            // More intense sparkles
            if (Scene.OnInterval(0.1f))
            {
                var level = Scene as Level;
                level?.ParticlesFG.Emit(sparkleParticle, Position);
            }
        }

        protected override void Collect(global::Celeste.Player player)
        {
            // Override to ensure full heal
            Audio.Play(SFX_COLLECT, Position);
            Audio.Play(SFX_HEAL, Position);

            // Full heal
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.FullHeal();
                }
                else
                {
                    var healthManager = PlayerHealthManager.Instance;
                    healthManager?.FullHeal();
                }
            }

            // Visual effects
            var level = Scene as Level;
            if (level != null)
            {
                // Large sparkle burst
                for (int i = 0; i < 20; i++)
                {
                    float angle = (i / 20f) * (float)Math.PI * 2f;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level.ParticlesFG.Emit(sparkleParticle, Position + dir * 12f);
                }

                // Screen flash
                level.Flash(Color.Red * 0.3f);

                // Heal popup
                level.Add(new HealNumberPopup(Position, 999));
            }

            sprite.Play("collect");
            Add(new Coroutine(RemoveAfterAnimation()));
        }
    }

    /// <summary>
    /// Invincibility Candy - Makes player invincible temporarily
    /// </summary>
    [CustomEntity("MaggyHelper/InvincibilityCandy")]
    [Tracked]
    public class InvincibilityCandy : KirbyFood
    {
        public override int HealAmount => 1; // Also heals 1 HP
        public override string SpritePath => "items/food/invincibilitycandy/";

        public InvincibilityCandy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            sparkleParticle.Color = Color.Gold; // Rainbow sparkles
        }

        protected override void Collect(global::Celeste.Player player)
        {
            base.Collect(player);

            // Grant invincibility
            if (player.IsKirbyMode())
            {
                // Add invincibility effect to player
                Scene.Add(new InvincibilityEffect(player, 10f)); // 10 seconds of invincibility
            }
        }
    }

    /// <summary>
    /// Effect that grants temporary invincibility
    /// </summary>
    public class InvincibilityEffect : Entity
    {
        private global::Celeste.Player player;
        private float duration;
        private float timer;
        private ParticleType rainbowParticle;

        public InvincibilityEffect(global::Celeste.Player target, float durationSeconds) : base(Vector2.Zero)
        {
            player = target;
            duration = durationSeconds;
            timer = 0f;
            Tag = Tags.Persistent;

            rainbowParticle = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                ColorMode = ParticleType.ColorModes.Choose,
                LifeMin = 0.3f,
                LifeMax = 0.5f,
                Size = 1f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f
            };
        }

        public override void Update()
        {
            base.Update();

            if (player == null || player.Scene == null)
            {
                RemoveSelf();
                return;
            }

            timer += Engine.DeltaTime;

            // Emit rainbow particles around player
            if (Scene.OnInterval(0.1f))
            {
                var level = Scene as Level;
                level?.ParticlesFG.Emit(rainbowParticle, player.Center + Calc.Random.Range(Vector2.One * -10f, Vector2.One * 10f));
            }

            // Flash player rainbow colors
            if (player.Sprite != null)
            {
                float hue = (timer * 2f) % 1f;
                player.Sprite.Color = Calc.HsvToColor(hue, 0.8f, 1f);
            }

            // Make player flash on HUD if health controller exists
            var controller = KirbyHealthController.Instance;
            if (controller != null)
            {
                // Extend invincibility on the controller
                controller.GetType().GetField("invincibilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(controller, Math.Max(duration - timer, 0f));
            }

            if (timer >= duration)
            {
                // Reset player color
                if (player.Sprite != null)
                {
                    player.Sprite.Color = Color.White;
                }
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            // Ensure player color is reset
            if (player?.Sprite != null)
            {
                player.Sprite.Color = Color.White;
            }
            base.Removed(scene);
        }
    }

    #endregion
}

