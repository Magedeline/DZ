using Celeste.Cutscenes;
using Microsoft.Xna.Framework;

namespace Celeste.NPCs
{
    public abstract class NpcBase : Entity
    {
        public string CutsceneId { get; protected set; }
        public bool CanInteract { get; protected set; } = true;
        public float InteractRadius { get; protected set; } = 32f;
        public bool TriggerOnTouch { get; protected set; } = false;

        protected Sprite Sprite;
        protected TalkComponent Talker;
        protected bool Interacting = false;
        protected Color FallbackColor = Color.Magenta;
        protected bool UseFallback = false;

        public NpcBase(Vector2 position, string cutsceneId) : base(position)
        {
            CutsceneId = cutsceneId;
            Depth = 100;
            Add(Talker = new TalkComponent(
                new Rectangle(-16, -8, 32, 8),
                Vector2.Zero, // drawAt parameter  
                Interact,
                null // hoverDisplay parameter fixed to null  
            ));
            Talker.Enabled = CanInteract;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (TriggerOnTouch)
            {
                Add(new PlayerCollider(OnPlayerTouch));
            }
        }

        protected virtual void OnPlayerTouch(global::Celeste.Player player)
        {
            if (CanInteract && !Interacting)
            {
                Interact(player);
            }
        }

        protected virtual void Interact(global::Celeste.Player player)
        {
            if (!string.IsNullOrEmpty(CutsceneId) && !Interacting)
            {
                Interacting = true;
                Scene.Add(new CutsceneTrigger(CutsceneId, player, () => Interacting = false));
            }
        }

        public override void Update()
        {
            base.Update();
            Talker.Enabled = CanInteract && !Interacting;
            // Simple ground snapping: if just above solid ground within small tolerance, snap down.
            // Prevents floating NPC placement due to integer rounding in map data.
            const int max_snap_pixels = 6;
            if (Scene != null && CollideCheck<Solid>(Position + Vector2.UnitY))
            {
                // Already on ground.
            }
            else
            {
                for (int i = 1; i <= max_snap_pixels; i++)
                {
                    Vector2 test = Position + Vector2.UnitY * i;
                    if (CollideCheck<Solid>(test + Vector2.UnitY))
                    {
                        // Found ground just below; snap flush to surface
                        Position = test;
                        break;
                    }
                }
            }
        }

        protected void AddSprite(string spriteId, Color? fallbackColor = null)
        {
            if (fallbackColor.HasValue)
                FallbackColor = fallbackColor.Value;

            var sprite = GFX.SpriteBank.Create(spriteId);
            if (sprite != null)
            {
                Add(sprite);
                sprite.Play("idle");
                Sprite = sprite;
                UseFallback = false;
            }
            else
            {
                UseFallback = true;
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"Sprite '{spriteId}' not found, using fallback square for {GetType().Name}");
            }
        }

        public override void Render()
        {
            if (UseFallback)
            {
                Draw.Rect(X - 12, Y - 24, 24, 32, FallbackColor);
                Draw.Rect(X - 10, Y - 22, 20, 28, FallbackColor * 0.7f);
            }
            base.Render();
        }
    }
}




