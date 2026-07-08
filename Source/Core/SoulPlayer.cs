using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A lightweight top-down soul/vessel entity used for 8-directional path exploration.
    /// Created alongside or instead of K_Player for soul-path sequences.
    /// </summary>
    [CustomEntity("DZ/SoulPlayer")]
    [Tracked(true)]
    [HotReloadable]
    public class SoulPlayer : Actor
    {
        public SoulPlayerController Controller { get; private set; }
        public BattlePlayerController BattleController { get; private set; }

        public int Health { get; private set; } = 6;
        public int MaxHealth { get; set; } = 6;
        public bool IsDead { get; internal set; }
        public int RefuseCount { get; private set; }
        public bool HasRefused => RefuseCount > 0;

        private Sprite sprite;
        private VertexLight glow;
        private float spawnTimer;
        private float invincibilityTimer;
        private const float InvincibilityDuration = 1.5f;

        public SoulPlayer(Vector2 position)
            : base(position)
        {
            Depth = -100;
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Controller = new SoulPlayerController());
            Add(BattleController = new BattlePlayerController());
            SetupVisuals();
        }

        public SoulPlayer(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        private void SetupVisuals()
        {
            try
            {
                sprite = GFX.SpriteBank.Create("DZ_Soul");
                sprite.Play("idle");
                sprite.CenterOrigin();
                Add(sprite);
            }
            catch
            {
                // Fallback if sprite bank entry is missing
            }

            Add(glow = new VertexLight(Color.LightCyan, 0.6f, 32, 48));
            glow.Position = Vector2.Zero;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            spawnTimer = 0.5f;
        }

        public override void Update()
        {
            base.Update();
            if (spawnTimer > 0f)
            {
                spawnTimer -= Engine.DeltaTime;
            }
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Engine.DeltaTime;
            }
        }

        public override void Render()
        {
            if (sprite != null)
            {
                sprite.Color = Color.White * (1f - spawnTimer / 0.5f);
            }
            base.Render();
        }

        public void SetActive(bool active)
        {
            Controller.IsActive = active;
            if (!active)
            {
                Controller.Reset();
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || invincibilityTimer > 0f) return;

            Health -= damage;
            invincibilityTimer = InvincibilityDuration;

            if (Health <= 0)
            {
                Health = 0;
                OnDeath();
            }
        }

        public void HealToFull()
        {
            Health = MaxHealth;
            IsDead = false;
        }

        public void OnRefused()
        {
            RefuseCount++;
            HealToFull();
        }

        private void OnDeath()
        {
            IsDead = true;
            Controller.SetMovementEnabled(false);
            Controller.Reset();

            if (Scene == null) return;

            var asrielGod = Scene.Tracker.GetEntity<AsrielGodBoss>();
            if (asrielGod != null && !asrielGod.IsBossDefeated)
            {
                Scene.Add(new global::Celeste.Cutscenes.CS20_AsrielGodRefusedToDie(this));
                return;
            }

            Scene.Add(new global::Celeste.Cutscenes.CS20_SoulRefusedToDie(this));
        }
    }
}
