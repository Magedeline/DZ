using Microsoft.Xna.Framework;
using Monocle;
using DZ;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/DZJumpThru")]
    [Monocle.Tracked]
    [HotReloadable]
    public class JumpThru : JumpthruPlatform
    {
        private string texture;
        
        public JumpThru(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, "wood") // Use string parameter
        {
            texture = data.Attr(nameof(texture), "wood");
            Depth = -9000;
            
            // Set surface sound based on texture - simplified
            SurfaceSoundIndex = SurfaceIndex.Wood;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            // Create the visual representation
            string texturePath = "objects/jumpthru/" + texture;
            
            int tilesWidth = (int)(Width / 8f);
            
            for (int i = 0; i < tilesWidth; i++)
            {
                int frame;
                if (i == 0)
                {
                    frame = 0; // Left edge
                }
                else if (i == tilesWidth - 1)
                {
                    frame = 2; // Right edge
                }
                else
                {
                    frame = 1; // Middle
                }
                
                MTexture texture = AtlasPathHelper.TryGetTexture(texturePath + frame.ToString("00"))
                    ?? AtlasPathHelper.TryGetTexture("objects/jumpthru/wood" + frame.ToString("00"));
                
                if (texture != null)
                {
                    Image image = new Image(texture);
                    image.X = i * 8;
                    Add(image);
                }
            }
        }
    }
}



