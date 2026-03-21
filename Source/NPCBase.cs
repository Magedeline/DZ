using MaggyHelper.Cutscenes;
namespace MaggyHelper.NPCs
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
    }
}




