#nullable enable
using global::Celeste.Mod.Meta;

namespace Celeste.Entities
{
    /// <summary>
    /// Maggy3D - Custom mountain marker character similar to Maddy3D
    /// Used to display a custom character marker on the mountain overworld map
    /// </summary>
    public class Maggy3D : Entity
    {
        public MountainRenderer Renderer;

        public Billboard Image;

        public Wiggler Wiggler;

        public Vector2 Scale = Vector2.One;

        public new Vector3 Position;

        public bool Show = true;

        public bool Disabled;

        private List<MTexture> frames = new List<MTexture>();

        private float frame;

        private float frameSpeed;

        private float alpha = 1f;

        private int hideDown;

        private bool running;

        /// <summary>
        /// Custom marker texture path - can be set to change the character appearance
        /// </summary>
        public string CustomMarkerPath { get; set; } = "marker/maggy";

        public Maggy3D(MountainRenderer renderer)
        {
            Renderer = renderer;
            Add(Image = new Billboard(null, Vector3.Zero));
            Image.BeforeRender = () =>
            {
                if (Disabled)
                {
                    Image.Color = Color.Transparent;
                }
                else
                {
                    Image.Position = Position + hideDown * Vector3.Up * (1f - Ease.CubeOut(alpha)) * 0.25f;
                    if (Wiggler != null)
                    {
                        Image.Scale = Scale + Vector2.One * Wiggler.Value * Scale * 0.2f;
                    }
                    else
                    {
                        Image.Scale = Scale;
                    }
                    Image.Scale *= (Renderer.Model.Camera.Position - Position).Length() / 20f;
                    Image.Color = Color.White * alpha;
                }
            };
            Add(Wiggler = Wiggler.Create(0.5f, 3f));
            Running(renderer.Area < 7);
        }

        public void Running(bool backpack = true)
        {
            running = true;
            Show = true;
            hideDown = -1;
            SetRunAnim();
            frameSpeed = 8f;
            frame = 0f;
            if (frames.Count > 0)
            {
                Image.Size = new Vector2(frames[0].ClipRect.Width, frames[0].ClipRect.Height) / frames[0].ClipRect.Width;
            }
        }

        public void Falling()
        {
            running = false;
            Show = true;
            hideDown = -1;
            
            // Try custom fall animation first
            if (MTN.Mountain.Has($"{CustomMarkerPath}/Fall"))
            {
                frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/Fall");
            }
            else
            {
                frames = MTN.Mountain.GetAtlasSubtextures("marker/Fall");
            }
            
            frameSpeed = 2f;
            frame = 0f;
            if (frames.Count > 0)
            {
                Image.Size = new Vector2(frames[0].ClipRect.Width, frames[0].ClipRect.Height) / frames[0].ClipRect.Width;
            }
        }

        public void Hide(bool down = true)
        {
            running = false;
            Show = false;
            hideDown = ((!down) ? 1 : (-1));
        }

        private void SetRunAnim()
        {
            if (Renderer.Area < 0 || AreaData.Get(Renderer.Area).IsOfficialLevelSet())
            {
                SetDefaultRunAnim();
                return;
            }
            
            // Try to get custom marker texture from map metadata
            string? markerTexture = GetMarkerTexture();
            if (markerTexture != null && MTN.Mountain.Has(markerTexture))
            {
                frames = MTN.Mountain.GetAtlasSubtextures(markerTexture);
            }
            // Try custom Maggy markers
            else if (MTN.Mountain.Has($"{CustomMarkerPath}/runNoBackpack") && 
                     AreaData.Get(Renderer.Area).Mode[0].Inventory.Dashes > 1)
            {
                frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/runNoBackpack");
            }
            else if (MTN.Mountain.Has($"{CustomMarkerPath}/runBackpack"))
            {
                frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/runBackpack");
            }
            else if (MTN.Mountain.Has($"{CustomMarkerPath}/run"))
            {
                frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/run");
            }
            // Fallback to vanilla Madeline
            else if (AreaData.Get(Renderer.Area).Mode[0].Inventory.Dashes > 1)
            {
                frames = MTN.Mountain.GetAtlasSubtextures("marker/runNoBackpack");
            }
            else
            {
                frames = MTN.Mountain.GetAtlasSubtextures("marker/runBackpack");
            }
        }

        public override void Update()
        {
            base.Update();
            if (running)
            {
                SetRunAnim();
            }
            if (frames != null && frames.Count > 0)
            {
                frame += Engine.DeltaTime * frameSpeed;
                if (frame >= (float)frames.Count)
                {
                    frame -= frames.Count;
                }
                Image.Texture = frames[(int)frame];
            }
            alpha = Calc.Approach(alpha, Show ? 1 : 0, Engine.DeltaTime * 4f);
        }

        private void SetDefaultRunAnim()
        {
            // Try custom Maggy markers first
            if (Renderer.Area < 7)
            {
                if (MTN.Mountain.Has($"{CustomMarkerPath}/runBackpack"))
                {
                    frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/runBackpack");
                }
                else
                {
                    frames = MTN.Mountain.GetAtlasSubtextures("marker/runBackpack");
                }
            }
            else
            {
                if (MTN.Mountain.Has($"{CustomMarkerPath}/runNoBackpack"))
                {
                    frames = MTN.Mountain.GetAtlasSubtextures($"{CustomMarkerPath}/runNoBackpack");
                }
                else
                {
                    frames = MTN.Mountain.GetAtlasSubtextures("marker/runNoBackpack");
                }
            }
        }

        private string? GetMarkerTexture()
        {
            try
            {
                var areaData = AreaData.Get(Renderer.Area);
                if (areaData == null) return null;
                
                if (!Everest.Content.TryGet(Path.Combine("Maps", areaData.SID ?? ""), out var metadata))
                {
                    return null;
                }
                return metadata.GetMeta<MapMeta>()?.Mountain?.MarkerTexture;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Trigger a wiggle animation (called when arriving at a chapter)
        /// </summary>
        public void TriggerWiggle()
        {
            Wiggler.Start();
        }

        /// <summary>
        /// Set custom marker texture path (e.g., "marker/magolor" or "DesoloZantas/marker/custom")
        /// </summary>
        public void SetCustomMarker(string path)
        {
            CustomMarkerPath = path;
            if (running)
            {
                SetRunAnim();
            }
        }
    }
}
