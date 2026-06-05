namespace Celeste.Entities
{
    /// <summary>
    /// A gate for Chapter 7 (Infernal Reflections) that opens when the player
    /// approaches and closes behind them, blocking backtracking.
    /// Based on vanilla TempleGate with CloseBehindPlayer behavior.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InfernoMaddyGate")]
    [Tracked]
    public class InfernoMaddyGate : Solid
    {
        public enum Types
        {
            NearestSwitch,
            CloseBehindPlayer,
            CloseBehindPlayerAlways,
            HoldingTheo,
            TouchSwitches
        }

        public Types Type;
        public string LevelID;
        public bool ClaimedByASwitch;

        private int closedHeight;
        private Sprite sprite;
        private Shaker shaker;
        private float drawHeight;
        private float drawHeightMoveSpeed;
        private bool open;
        private float holdingWaitTimer = 0.2f;
        private Vector2 holdingCheckFrom;
        private bool lockState;
        private string spriteName;

        public InfernoMaddyGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8f, data.Height, true)
        {
            closedHeight = data.Height;
            spriteName = data.Attr("sprite", "default");
            Type = data.Enum("type", Types.CloseBehindPlayer);
            LevelID = data.Level.Name;

            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_templegate_" + spriteName));
            sprite.X = 4f;
            sprite.Play("idle");

            Add(shaker = new Shaker(on: false));

            Depth = -9000;
            drawHeight = closedHeight;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (Type == Types.CloseBehindPlayer || Type == Types.CloseBehindPlayerAlways)
            {
                CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
                if (player != null && player.X < X)
                {
                    // Player is already past this gate â€” start open
                    StartOpen();
                }
            }
            else if (Type == Types.HoldingTheo)
            {
                holdingCheckFrom = new Vector2(X - 16f, CenterY);
            }
        }

        public override void Update()
        {
            base.Update();

            if (lockState)
                return;

            switch (Type)
            {
                case Types.CloseBehindPlayer:
                case Types.CloseBehindPlayerAlways:
                    UpdateCloseBehindPlayer();
                    break;
                case Types.HoldingTheo:
                    UpdateHoldingTheo();
                    break;
                case Types.NearestSwitch:
                    // Controlled externally by switches
                    break;
                case Types.TouchSwitches:
                    // Controlled by touch switches
                    break;
            }

            float target = open ? 0f : closedHeight;
            if (drawHeight != target)
            {
                drawHeightMoveSpeed += 800f * Engine.DeltaTime;
                drawHeight = Calc.Approach(drawHeight, target, drawHeightMoveSpeed * Engine.DeltaTime);
            }
            else
            {
                drawHeightMoveSpeed = 0f;
            }

            Collider.Height = Math.Max(2f, drawHeight);
        }

        private void UpdateCloseBehindPlayer()
        {
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
            if (player == null) return;

            if (!open)
            {
                // Open when player is close and approaching from the right side
                if (player.X > X + 4f && Math.Abs(player.Y - CenterY) < Height / 2f + 16f)
                {
                    Open();
                }
            }
            else if (Type == Types.CloseBehindPlayerAlways || 
                     SceneAs<Level>().Session.Level == LevelID)
            {
                // Close behind player once they pass through
                if (player.X < X - 8f)
                {
                    Close();
                }
            }
        }

        private void UpdateHoldingTheo()
        {
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
            if (player == null) return;

            if (!open)
            {
                if (player.Holding != null && player.X > X - 24f)
                {
                    holdingWaitTimer -= Engine.DeltaTime;
                    if (holdingWaitTimer <= 0f)
                        Open();
                }
                else
                {
                    holdingWaitTimer = 0.2f;
                }
            }
            else
            {
                if (player.Holding == null && player.X < X + 4f)
                {
                    Close();
                }
            }
        }

        public void Open()
        {
            if (open) return;
            open = true;
            Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
            sprite.Play("open");
            drawHeightMoveSpeed = 0f;
        }

        public void Close()
        {
            if (!open) return;
            open = false;
            Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
            sprite.Play("hit");
            drawHeightMoveSpeed = 200f;
        }

        public void StartOpen()
        {
            open = true;
            drawHeight = 0f;
            drawHeightMoveSpeed = 0f;
            Collider.Height = 2f;
        }

        public void LockState(bool lockOpen)
        {
            lockState = true;
            if (lockOpen)
                StartOpen();
        }

        public override void Render()
        {
            Vector2 shakeOffset = shaker.Value;

            // Draw the gate sprite clipped to the current height
            float drawY = Y + closedHeight - drawHeight;
            if (drawHeight > 0f)
            {
                int tileHeight = (int)(drawHeight / 8f);
                for (int i = 0; i <= tileHeight; i++)
                {
                    float segmentY = drawY + i * 8f;
                    float segmentHeight = Math.Min(8f, Y + closedHeight - segmentY);
                    if (segmentHeight > 0f)
                    {
                        sprite.Position = new Vector2(4f + shakeOffset.X, segmentY - Y + shakeOffset.Y);
                        // Use a simple filled rectangle for non-sprite segments
                        Draw.Rect(X + shakeOffset.X, segmentY + shakeOffset.Y, 8f, segmentHeight, Color.DarkRed);
                    }
                }

                // Draw the sprite at the top of the gate
                sprite.Position = new Vector2(4f + shakeOffset.X, drawY - Y + shakeOffset.Y);
                sprite.Render();
            }
        }
    }
}

