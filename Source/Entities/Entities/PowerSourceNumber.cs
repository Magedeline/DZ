#nullable disable
namespace Celeste.Entities
{
    /// <summary>
    /// A decorative power-source number display that shows a numbered indicator
    /// and pulses with a glow effect once the "disable_lightning" flag is set.
    /// Place one per power source to show completion status to the player.
    /// </summary>
    [CustomEntity("DZ/PowerSourceNumber")]
    [Tracked]
    public class PowerSourceNumber : Entity
    {
        private readonly Image image;
        private readonly Image glow;
        private float ease;
        private float timer;
        private bool gotCollectable;
        private int index;
        private string flag;

        public PowerSourceNumber(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            index = Calc.Clamp(data.Int("index", 1), 1, 9);
            flag = data.Attr("flag", "disable_lightning");
            gotCollectable = data.Bool("gotCollectable", false);

            Depth = -10010;

            string numberPath = data.Attr("numberSprite", "scenery/powersource_numbers/" + index);
            string glowPath = data.Attr("glowSprite", "scenery/powersource_numbers/" + index + "_glow");

            Add(image = new Image(GFX.Game[numberPath]));
            image.CenterOrigin();

            Add(glow = new Image(GFX.Game[glowPath]));
            glow.CenterOrigin();
            glow.Color = Color.Transparent;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            // If the session already has the flag set, check collectable status
            Level level = scene as Level;
            if (level != null && level.Session.GetFlag(flag) && !gotCollectable)
            {
                // Immediately show glow if condition is already met
                ease = 1f;
                glow.Color = Color.White * 0.7f;
            }
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            if (level == null)
                return;

            if (level.Session.GetFlag(flag) && !gotCollectable)
            {
                timer += Engine.DeltaTime;
                ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 4f);
                glow.Color = Color.White * ease * Calc.SineMap(timer * 2f, 0.5f, 0.9f);
            }
            else if (gotCollectable)
            {
                // Already collected — keep glow off
                ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
                glow.Color = Color.White * ease;
            }
        }

        /// <summary>
        /// Mark this source number as collected (stops the glow).
        /// Can be called from triggers or other entities.
        /// </summary>
        public void MarkCollected()
        {
            gotCollectable = true;
        }
    }
}
