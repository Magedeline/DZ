using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/FarewellGate")]
    [HotReloadable]
    public class FarewellGate : Solid
    {
        private MTexture texture;
        private bool opened;
        private float openProgress;
        private string flag;
        private bool inverted;
        private Vector2 closedPosition;
        private float moveDistance;
        private int heartGems;

        public FarewellGate(Vector2 position, int width, int height, string flag, bool inverted, int heartGems)
            : base(position, width, height, true)
        {
            this.flag = flag;
            this.inverted = inverted;
            this.heartGems = heartGems;
            closedPosition = position;
            moveDistance = height + 8;
            
            texture = AtlasPathHelper.GetTexture("objects/DZ/DZ/farewellGate");
            base.Depth = -9000;
        }

        public FarewellGate(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height,
                   data.Attr("flag", ""), data.Bool("inverted", false), data.Int("heartGems", 0))
        {
        }

        public override void Update()
        {
            base.Update();
            
            Level level = SceneAs<Level>();
            
            bool shouldBeOpen = false;
            
            // Check flag condition
            if (!string.IsNullOrEmpty(flag))
            {
                shouldBeOpen = level.Session.GetFlag(flag);
            }
            
            // Check heart gem requirement
            if (heartGems > 0)
            {
                int collected = level.Session.HeartGem ? 1 : 0;
                // Could also check SaveData for total hearts
                shouldBeOpen = collected >= heartGems || shouldBeOpen;
            }
            
            if (inverted) shouldBeOpen = !shouldBeOpen;
            
            if (shouldBeOpen && !opened)
            {
                opened = true;
                Add(new Coroutine(OpenRoutine()));
            }
            else if (!shouldBeOpen && opened)
            {
                opened = false;
                Add(new Coroutine(CloseRoutine()));
            }
        }

        private IEnumerator OpenRoutine()
        {
            Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
            
            float duration = 0.8f;
            float elapsed = 0f;
            float startProgress = openProgress;
            
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                openProgress = Calc.LerpClamp(startProgress, 1f, Ease.CubeOut(elapsed / duration));
                MoveTo(closedPosition + new Vector2(0, -moveDistance * openProgress));
                yield return null;
            }
            
            openProgress = 1f;
            Collidable = false;
        }

        private IEnumerator CloseRoutine()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
            Collidable = true;
            
            float duration = 0.5f;
            float elapsed = 0f;
            float startProgress = openProgress;
            
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                openProgress = Calc.LerpClamp(startProgress, 0f, Ease.CubeIn(elapsed / duration));
                MoveTo(closedPosition + new Vector2(0, -moveDistance * openProgress));
                yield return null;
            }
            
            openProgress = 0f;
        }

        public override void Render()
        {
            for (int x = 0; x < Width; x += texture.Width)
            {
                for (int y = 0; y < Height; y += texture.Height)
                {
                    texture.Draw(Position + new Vector2(x, y));
                }
            }
        }
    }
}

