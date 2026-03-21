namespace MaggyHelper
{
    [HotReloadable]
    public class WaveFazeSlammerPlaybackTutorial
    {
        public Action OnRender;
        private bool hasUpdated;
        private float dashTrailTimer;
        private int dashTrailCounter;
        private bool dashing;
        private bool firstDash = true;
        private bool secondDash = false;
        private bool dreamDashing = false;
        private bool groundPounding = false;
        private bool wallJumping = false;
        private float launchedTimer;
        private int tag;
        private Vector2 dashDirection0;
        private Vector2 dashDirection1;
        private Vector2 dreamDashIn;
        private Vector2 dashDirection2;

        public PlayerPlayback Playback { get; private set; }

        public WaveFazeSlammerPlaybackTutorial(
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
            
            // Handle wavedash (first dash)
            if (Playback.Sprite.CurrentAnimationID == "dash" && Playback.Sprite.CurrentAnimationFrame == 0 && firstDash)
            {
                if (!dashing)
                {
                    dashing = true;
                    Celeste.Celeste.Freeze(0.05f);
                    SlashFx.Burst(Playback.Center, dashDirection0.Angle()).Tag = tag;
                    dashTrailTimer = 0.1f;
                    dashTrailCounter = 2;
                    firstDash = false;
                    secondDash = true;
                }
            }
            // Handle dream dash
            else if (Playback.Sprite.CurrentAnimationID == "dreamDashIn" && !dreamDashing)
            {
                dreamDashing = true;
                SpeedRing speedRing = Engine.Pooler.Create<SpeedRing>().Init(Playback.Center, dreamDashIn.Angle(), Color.Cyan);
                speedRing.Tag = tag;
                Engine.Scene.Add(speedRing);
            }
            // Handle wall jump
            else if (Playback.Sprite.CurrentAnimationID == "wallslide" && !wallJumping)
            {
                wallJumping = true;
            }
            // Handle ground pound / second dash
            else if (Playback.Sprite.CurrentAnimationID == "dash" && Playback.Sprite.CurrentAnimationFrame == 0 && secondDash && !groundPounding)
            {
                if (!dashing)
                {
                    dashing = true;
                    groundPounding = true;
                    Celeste.Celeste.Freeze(0.05f);
                    SlashFx.Burst(Playback.Center, dashDirection2.Angle()).Tag = tag;
                    dashTrailTimer = 0.1f;
                    dashTrailCounter = 3;
                }
            }
            else
            {
                dashing = false;
            }
            
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
            
            if (dreamDashing || wallJumping || groundPounding)
            {
                launchedTimer += Engine.DeltaTime;
                if (Calc.OnInterval(launchedTimer, launchedTimer - Engine.DeltaTime, 0.15f))
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
            
            try
            {
                Draw.SpriteBatch.End();
            }
            catch (InvalidOperationException)
            {
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
