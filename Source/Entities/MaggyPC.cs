using System.Runtime.CompilerServices;

namespace Celeste.Entities
{
    /// <summary>
    /// Maggy's PC - A sci-fi themed computer terminal inspired by the Star Cutter Lor from Kirby.
    /// Features a holographic display with animated visuals and ambient sounds.
    /// Based on KevinsPC structure but with Maggy-specific theming.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MaggyPC")]
    public class MaggyPC : Actor
    {
        private Image image;
        private MTexture spectogram;
        private MTexture subtex;
        private SoundSource sfx;
        private float timer;

        // Spectogram animation settings
        private const int SPECTOGRAM_WIDTH = 32;
        private const int SPECTOGRAM_HEIGHT = 18;
        private const float ANIMATION_CYCLE_DURATION = 22f; // Duration of one full spectogram cycle in seconds

        [MethodImpl(MethodImplOptions.NoInlining)]
        public MaggyPC(Vector2 position)
            : base(position)
        {
            // Main PC image - Star Cutter themed computer terminal
            Add(image = new Image(AtlasPathHelper.GetTexture("objects/maggypc/pc0")));
            image.JustifyOrigin(0.5f, 1f);
            
            // Depth between foreground and background elements
            base.Depth = 8999;
            
            // Holographic display spectogram
            spectogram = AtlasPathHelper.GetTexture("objects/maggypc/spectogram");
            subtex = spectogram.GetSubtexture(0, 0, SPECTOGRAM_WIDTH, SPECTOGRAM_HEIGHT, subtex);
            
            // Ambient sci-fi computer sound
            Add(sfx = new SoundSource("event:/pusheen/final_content/env/19_maggypc"));
            sfx.Position = new Vector2(0f, -16f);
            
            timer = 0f;
        }

        public MaggyPC(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override bool IsRiding(Solid solid)
        {
            // Check if the PC is resting on a solid platform
            return base.Scene.CollideCheck(new Rectangle((int)base.X - 4, (int)base.Y, 8, 2), solid);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            
            // Animate the spectogram display by scrolling through the texture
            timer += Engine.DeltaTime;
            int scrollWidth = spectogram.Width - SPECTOGRAM_WIDTH;
            int x = (int)(timer * ((float)scrollWidth / ANIMATION_CYCLE_DURATION) % (float)scrollWidth);
            subtex = spectogram.GetSubtexture(x, 0, SPECTOGRAM_WIDTH, SPECTOGRAM_HEIGHT, subtex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            base.Render();
            
            // Draw the animated holographic display on top of the PC
            if (subtex != null)
            {
                // Draw the spectogram/hologram display
                subtex.Draw(Position + new Vector2(-16f, -39f));
                
                // Subtle dark overlay for depth effect (sci-fi hologram look)
                Draw.Rect(base.X - 16f, base.Y - 39f, SPECTOGRAM_WIDTH, SPECTOGRAM_HEIGHT, Color.Black * 0.25f);
            }
        }
    }
}
