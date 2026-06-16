using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/MrBonesDoor")]
    public class MrBonesDoor : Solid
    {
        private MTexture texture;
        private bool opened;
        private float openAmount;
        private int requiredKeys;
        private string flagToSet;

        public MrBonesDoor(Vector2 position, int height, int requiredKeys, string flagToSet)
            : base(position, 8f, height, true)
        {
            this.requiredKeys = requiredKeys;
            this.flagToSet = flagToSet;
            
            texture = AtlasPathHelper.GetTexture("objects/mrbonesdoor");
            
            Add(new PlayerCollider(player => OnPlayer((global::Celeste.Player)player)));
            base.Depth = -9000;
        }

        public MrBonesDoor(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, 
                   data.Int("requiredKeys", 0), data.Attr("flagToSet", ""))
        {
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (!opened && CanOpen())
            {
                Open();
            }
        }

        private bool CanOpen()
        {
            if (requiredKeys <= 0) return true;
            
            // Check if player has enough keys (using session flags)
            Level level = SceneAs<Level>();
            int keysCollected = 0;
            for (int i = 0; i < 10; i++)
            {
                if (level.Session.GetFlag($"key_{i}"))
                {
                    keysCollected++;
                }
            }
            return keysCollected >= requiredKeys;
        }

        private void Open()
        {
            opened = true;
            Collidable = false;
            
            Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
            
            Level level = SceneAs<Level>();
            if (!string.IsNullOrEmpty(flagToSet))
            {
                level.Session.SetFlag(flagToSet);
            }
            
            Add(new Coroutine(OpenRoutine()));
        }

        private IEnumerator OpenRoutine()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                openAmount = Ease.CubeOut(elapsed / duration);
                yield return null;
            }
            
            openAmount = 1f;
        }

        public override void Render()
        {
            if (!opened || openAmount < 1f)
            {
                float visibleHeight = Height * (1f - openAmount);
                
                for (int y = 0; y < visibleHeight; y += texture.Height)
                {
                    float h = Math.Min(texture.Height, visibleHeight - y);
                    texture.Draw(Position + new Vector2(0, y), Vector2.Zero, Color.White, 
                        new Vector2(1f, h / texture.Height));
                }
            }
        }
    }
}

