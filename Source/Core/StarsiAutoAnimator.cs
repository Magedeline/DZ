#nullable enable
namespace DZ {
    [HotReloadable]
    public class StarsiAutoAnimator() : Component(true, false) {
        private const bool enabled = true;
        private string lastAnimation = "fallSlow";
        private bool wasSyncingSprite = false;
        private Wiggler? pop;

        public override void Added(Entity entity) {
            base.Added(entity);
            entity.Add(pop = Wiggler.Create(0.5f, 4f, f => {
                Sprite? sprite = Entity.Get<Sprite>();
                if (sprite == null)
                    return;
                sprite.Scale = new Vector2(Math.Sign(sprite.Scale.X), 1f) * (float)(1.0 + 0.25 * f);
            }));
        }

        public override void Removed(Entity entity) {
            if (pop != null)
                entity.Remove(pop);
            base.Removed(entity);
        }

        public void SetReturnToAnimation(string anim) => lastAnimation = anim;

        public override void Update() {
            Sprite? sprite = Entity.Get<Sprite>();
            if (Scene == null || sprite == null)
                return;
            bool flag = false;
            Textbox? entity = Scene.Tracker.GetEntity<Textbox>();
            if (enabled && entity != null) {
                if (entity.PortraitName.IsIgnoreCase("starsi")) {
                    if (entity.PortraitAnimation.IsIgnoreCase("laugh")) {
                        if (!wasSyncingSprite)
                            lastAnimation = sprite.CurrentAnimationID;
                        sprite.Play("laugh");
                        wasSyncingSprite = flag = true;
                    } else if (entity.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC")) {
                        if (!wasSyncingSprite) {
                            pop?.Start();
                            lastAnimation = sprite.CurrentAnimationID;
                        }
                        sprite.Play("angry");
                        wasSyncingSprite = flag = true;
                    }
                }
            }
            if (!wasSyncingSprite || flag)
                return;
            wasSyncingSprite = false;
            if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
                lastAnimation = "fallSlow";
            if (sprite.CurrentAnimationID == "angry")
                pop?.Start();
            sprite.Play(lastAnimation);
        }
    }
}



