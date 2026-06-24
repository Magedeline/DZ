namespace Celeste.Entities
{
    /// <summary>
    /// A reinforced block that cannot be broken by normal dashing.
    /// Only powerful entities can break it:
    /// - Kevin (CrushBlock) slamming into it
    /// - Cannon (LaunchCannon) launching the player through it at high speed
    /// - Seekers colliding with it
    /// </summary>
    [CustomEntity("MaggyHelper/UltraDashBlock")]
    [Tracked]
    public class UltraDashBlock : Solid
    {
        private const float LAUNCH_SPEED_THRESHOLD = 300f;

        private const string SFX_BREAK_GENERAL = "event:/game/pusheen/general/break_stone";
        private const string SFX_BREAK_SPACE = "event:/new_content/game/pusheen/19_spaces/space_break_stone";
        private const string SFX_BREAK_UNDERWATER = "event:/game/pusheen/general/underwater_break_stone";

        public enum BreakEnvironment
        {
            General,
            Space,
            Underwater
        }

        private char fillTile;
        private TileGrid tiles;
        private bool broken;
        private new float shakeTimer;
        private Vector2 shakeOffset;
        private bool permanent;
        private EntityID id;
        private EffectCutout cutout;
        private bool canBeSeeker;
        private BreakEnvironment environment;

        public UltraDashBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            this.id = id;
            fillTile = data.Char("tiletype", 'm');
            permanent = data.Bool("permanent", false);
            canBeSeeker = data.Bool("breakableBySeeker", true);
            environment = data.Enum("environment", BreakEnvironment.General);
            Depth = -13000;
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = 18;
            Add(cutout = new EffectCutout());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

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
            Add(tiles);
            Add(new TileInterceptor(tiles, true));
        }

        public override void Update()
        {
            base.Update();

            if (broken) return;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                shakeOffset = Calc.Random.ShakeVector();
            }
            else
            {
                shakeOffset = Vector2.Zero;
            }

            // Check for Kevin (CrushBlock) adjacent collision.
            // CrushBlock is a Solid that moves into other solids; when it stops
            // at this block's edge it means it has impacted us. We expand our
            // hitbox by 1 pixel in each direction to detect adjacency.
            foreach (Entity entity in Scene.Tracker.GetEntities<CrushBlock>())
            {
                CrushBlock crushBlock = (CrushBlock)entity;
                if (CollideCheck(crushBlock, Position + Vector2.UnitX) ||
                    CollideCheck(crushBlock, Position - Vector2.UnitX) ||
                    CollideCheck(crushBlock, Position + Vector2.UnitY) ||
                    CollideCheck(crushBlock, Position - Vector2.UnitY))
                {
                    Break(crushBlock.Center, (Center - crushBlock.Center).SafeNormalize());
                    return;
                }
            }

            // Check for Seeker collision
            if (canBeSeeker)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<Seeker>())
                {
                    Seeker seeker = (Seeker)entity;
                    if (CollideCheck(seeker))
                    {
                        Break(seeker.Center, (Center - seeker.Center).SafeNormalize());
                        return;
                    }
                }
            }

            // Check for player launched at high speed (e.g. from cannon)
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                float speed = player.Speed.Length();
                if (speed >= LAUNCH_SPEED_THRESHOLD)
                {
                    // Check adjacency with the player (touching the block at speed)
                    if (CollideCheck(player, player.Position + player.Speed.SafeNormalize()) ||
                        CollideCheck(player))
                    {
                        Break(player.Center, player.Speed.SafeNormalize());
                        return;
                    }
                }
            }
        }

        private DashCollisionResults OnDashed(global::Celeste.Player player, Vector2 direction)
        {
            // Check if the player is moving fast enough (e.g. launched by cannon)
            float speed = player.Speed.Length();
            if (speed >= LAUNCH_SPEED_THRESHOLD)
            {
                Break(player.Center, direction);
                return DashCollisionResults.Rebound;
            }

            // Normal dashes just shake - the block is too tough
            shakeTimer = 0.3f;
            Audio.Play(GetBreakSfx(), Center);
            return DashCollisionResults.NormalCollision;
        }

        /// <summary>
        /// Public break method so other powerful entities (like CrushBlock hooks) can break this block.
        /// </summary>
        public void Break(Vector2 from, Vector2 direction)
        {
            if (broken) return;
            broken = true;
            Collidable = false;

            Level level = SceneAs<Level>();
            level.Shake(0.3f);
            Audio.Play(GetBreakSfx(), Center);

            // Create debris
            for (int x = 0; x < Width; x += 8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    Vector2 debrisPos = Position + new Vector2(x + 4, y + 4);
                    level.Particles.Emit(Player.P_CassetteFly, 2, debrisPos, Vector2.One * 4f);

                    Debris debris = Engine.Pooler.Create<Debris>()
                        .Init(debrisPos, fillTile, playSound: false)
                        .BlastFrom(from);
                    level.Add(debris);
                }
            }

            if (permanent)
            {
                level.Session.DoNotLoad.Add(id);
            }

            RemoveSelf();
        }

        private string GetBreakSfx()
        {
            return environment switch
            {
                BreakEnvironment.Space => SFX_BREAK_SPACE,
                BreakEnvironment.Underwater => SFX_BREAK_UNDERWATER,
                _ => SFX_BREAK_GENERAL
            };
        }

        public override void Render()
        {
            if (!broken)
            {
                Vector2 origPos = Position;
                Position += shakeOffset;
                base.Render();
                Position = origPos;
            }
        }
    }
}
