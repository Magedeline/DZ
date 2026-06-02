namespace Celeste
{
    [HotReloadable]
    public class WaveFazePlaybackTutorial
    {
        public Action OnRender;
        private bool hasUpdated;
        private float dashTrailTimer;
        private int dashTrailCounter;
        private bool dashing;
        private bool firstDash = true;
        private bool launched;
        private float launchedDelay;
        private float launchedTimer;
        private int tag;
        private Vector2 dashDirection0;
        private Vector2 dashDirection1;
        private Vector2 dreamDashIn;
        private Vector2 dashDirection2;

        public PlayerPlayback Playback { get; private set; }

        public WaveFazePlaybackTutorial(
            string name,
            Vector2 offset,
            Vector2 dashDirection0,
            Vector2 dashDirection1,
            Vector2 dreamDashIn,
            Vector2 dashDirection2)
        {
            List<global::Celeste.Player.ChaserState> tutorial = PlaybackData.Tutorials[name];
            Playback = new PlayerPlayback(offset, (global::Celeste.PlayerSpriteMode)PlayerSpriteMode.MadelineNoBackpack, tutorial);
            tag = Calc.Random.Next();
            this.dashDirection0 = dashDirection0;
            this.dashDirection1 = dashDirection1;
            this.dreamDashIn = dreamDashIn;
            this.dashDirection2 = dashDirection2;
        }

        public void Update()
        {
            Playback.Update();
            Playback.Hair.AfterUpdate();
            if (Playback.Sprite.CurrentAnimationID == "dash" && Playback.Sprite.CurrentAnimationFrame == 0)
            {
                if (!dashing)
                {
                    dashing = true;
                    CelesteGame.Freeze(0.05f);
                    SlashFx.Burst(Playback.Center, (firstDash ? dashDirection0 : (launched ? dreamDashIn : dashDirection1)).Angle()).Tag = tag;
                    dashTrailTimer = 0.1f;
                    dashTrailCounter = 2;
                    if (firstDash)
                        launchedDelay = 0.15f;
                    firstDash = !firstDash;
                }
            }
            else
                dashing = false;
            if (dashTrailTimer > 0.0)
            {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0.0)
                {
                    --dashTrailCounter;
                    if (dashTrailCounter > 0)
                        dashTrailTimer = 0.1f;
                }
            }
            if (launchedDelay > 0.0)
            {
                launchedDelay -= Engine.DeltaTime;
                if (launchedDelay <= 0.0)
                {
                    launched = true;
                    launchedTimer = 0.0f;
                }
            }
            if (launched)
            {
                float launchedTimer = this.launchedTimer;
                this.launchedTimer += Engine.DeltaTime;
                if (this.launchedTimer >= 0.5)
                {
                    launched = false;
                    this.launchedTimer = 0.0f;
                }
                else if (Calc.OnInterval(this.launchedTimer, launchedTimer, 0.15f))
                {
                    SpeedRing speedRing = Engine.Pooler.Create<SpeedRing>().Init(Playback.Center, (Playback.Position - Playback.LastPosition).Angle(), Color.White);
                    speedRing.Tag = tag;
                    Engine.Scene.Add(speedRing);
                }
            }
            hasUpdated = true;
        }

        public void Render(Vector2 position, float scale)
        {
            Matrix transformMatrix = Matrix.CreateScale(4f) * Matrix.CreateTranslation(position.X, position.Y, 0.0f);
            
            // Safely end the current sprite batch if one is active
            try
            {
                Draw.SpriteBatch.End();
            }
            catch (InvalidOperationException)
            {
                // Sprite batch wasn't active, that's okay
            }
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix);
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<TrailManager.Snapshot>())
            {
                if (entity.Tag == tag)
                    entity.Render();
            }
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<SlashFx>())
            {
                if (entity.Tag == tag && entity.Visible)
                    entity.Render();
            }
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<SpeedRing>())
            {
                if (entity.Tag == tag)
                    entity.Render();
            }
            if (Playback.Visible && hasUpdated)
                Playback.Render();
            if (OnRender != null)
                OnRender();
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
        }
    }
}




