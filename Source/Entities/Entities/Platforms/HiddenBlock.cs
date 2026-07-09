namespace Celeste.Entities
{
    /// <summary>
    /// A hidden block or wall that looks like empty space until the player walks into it.
    /// When the player collides with it, it reveals itself with a fade-in effect and
    /// plays the revealed_secrets sound event.
    /// Similar to vanilla FakeWall but with custom reveal behavior.
    /// </summary>
    [CustomEntity("DZ/HiddenBlock")]
    [Tracked]
    public class HiddenBlock : Solid
    {
        private const string SFX_REVEALED = "event:/DZ/game/general/revealed_secrets";

        private char fillTile;
        private TileGrid tiles;
        private EffectCutout cutout;
        private EntityID id;
        private bool permanent;
        private bool revealed;
        private bool fading;
        private float revealAlpha;
        private float transitionStartAlpha;
        private bool transitionFade;

        public HiddenBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            this.id = id;
            fillTile = data.Char("tiletype", '3');
            permanent = data.Bool("permanent", true);
            Depth = -13000;

            // Start invisible and non-collidable - this is a hidden block
            Collidable = false;
            Add(cutout = new EffectCutout());
            cutout.Visible = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // If already revealed in this session, show it directly
            if (permanent && SceneAs<Level>().Session.DoNotLoad.Contains(id))
            {
                RemoveSelf();
                return;
            }

            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)X / 8 - tileBounds.Left;
            int y = (int)Y / 8 - tileBounds.Top;
            tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            tiles.Alpha = 0f;
            Add(tiles);
            Add(new TileInterceptor(tiles, false));

            Add(new TransitionListener()
            {
                OnOut = OnTransitionOut,
                OnOutBegin = OnTransitionOutBegin,
                OnIn = OnTransitionIn,
                OnInBegin = OnTransitionInBegin
            });
        }

        public override void Update()
        {
            base.Update();

            if (revealed && !fading)
                return;

            if (!revealed)
            {
                // Temporarily enable collision to check if the player overlaps this hidden area
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    Collidable = true;
                    bool isOverlapping = player.CollideCheck(this);
                    Collidable = false;
                    if (isOverlapping)
                    {
                        Reveal();
                    }
                }
            }

            if (fading)
            {
                revealAlpha = Calc.Approach(revealAlpha, 1f, 2f * Engine.DeltaTime);
                tiles.Alpha = revealAlpha;
                cutout.Alpha = revealAlpha;

                if (revealAlpha >= 1f)
                {
                    fading = false;
                }
            }
        }

        private void Reveal()
        {
            if (revealed) return;
            revealed = true;
            fading = true;
            revealAlpha = 0f;

            // Make the block solid now that it's revealed
            Collidable = true;
            cutout.Visible = true;

            Audio.Play(SFX_REVEALED, Center);

            if (permanent)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
        }

        private void OnTransitionOutBegin()
        {
            if (revealed && Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (!transitionFade) return;
            tiles.Alpha = transitionStartAlpha * (1f - percent);
        }

        private void OnTransitionInBegin()
        {
            Level level = SceneAs<Level>();
            if (revealed && level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
            {
                transitionFade = true;
                tiles.Alpha = 0f;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent)
        {
            if (!transitionFade) return;
            tiles.Alpha = percent;
        }

        public override void Render()
        {
            if (revealed)
            {
                Level scene = Scene as Level;
                if (scene.ShakeVector.X < 0.0 && scene.Camera.X <= scene.Bounds.Left && X <= scene.Bounds.Left)
                    tiles.RenderAt(Position + new Vector2(-3f, 0f));
                if (scene.ShakeVector.X > 0.0 && scene.Camera.X + 320.0 >= scene.Bounds.Right && X + Width >= scene.Bounds.Right)
                    tiles.RenderAt(Position + new Vector2(3f, 0f));
                if (scene.ShakeVector.Y < 0.0 && scene.Camera.Y <= scene.Bounds.Top && Y <= scene.Bounds.Top)
                    tiles.RenderAt(Position + new Vector2(0f, -3f));
                if (scene.ShakeVector.Y > 0.0 && scene.Camera.Y + 180.0 >= scene.Bounds.Bottom && Y + Height >= scene.Bounds.Bottom)
                    tiles.RenderAt(Position + new Vector2(0f, 3f));
            }
            base.Render();
        }
    }
}
