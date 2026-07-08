using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Auto-animator for Chara boss entities.
    /// Handles sprite animation timing and transitions.
    /// </summary>
    public class CharaAutoAnimator : Component
    {
        private Sprite sprite;
        private float idleTimer;
        private const float IDLE_INTERVAL = 2f;

        public CharaAutoAnimator()
            : base(false, false)
        {
        }

        public void Initialize(Sprite sprite)
        {
            this.sprite = sprite;
            idleTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (sprite == null)
                return;

            idleTimer += Engine.DeltaTime;

            if (idleTimer >= IDLE_INTERVAL)
            {
                idleTimer = 0f;
                if (sprite.Has("idle"))
                {
                    sprite.Play("idle");
                }
            }
        }
    }
}
