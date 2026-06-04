namespace Celeste.Entities
{
    /// <summary>
    /// A pooled shockwave projectile for Chapter 7 (Infernal Reflections).
    /// Expands from the big eyeball and pushes the player back.
    /// Uses displacement rendering for a visual distortion effect.
    /// </summary>
    [Pooled]
    [CustomEntity(ids: "MaggyHelper/InfernoBigEyeballShockwave")]
    [Tracked]
    public class InfernoBigEyeballShockwave : Entity
    {
        private MTexture distortionTexture;
        private float distortionAlpha;
        private bool hasHitPlayer;

        public InfernoBigEyeballShockwave()
        {
            Depth = -1000000;
            Collider = new Hitbox(48f, 200f, -30f, -100f);
            Add(new PlayerCollider(OnPlayer));

            MTexture m = GFX.Game["util/displacementcirclehollow"];
            distortionTexture = m.GetSubtexture(0, 0, m.Width / 2, m.Height);
            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        public InfernoBigEyeballShockwave Init(Vector2 position)
        {
            Position = position;
            Collidable = true;
            distortionAlpha = 0f;
            hasHitPlayer = false;
            return this;
        }

        public override void Update()
        {
            base.Update();

            X -= 300f * Engine.DeltaTime;
            distortionAlpha = Calc.Approach(distortionAlpha, 1f, Engine.DeltaTime * 4f);

            Level level = SceneAs<Level>();
            if (X < level.Bounds.Left - 20)
                RemoveSelf();
        }

        private void RenderDisplacement()
        {
            distortionTexture.DrawCentered(Position,
                Color.White * 0.8f * distortionAlpha,
                new Vector2(0.9f, 1.5f));
        }

        private void OnPlayer(CelestePlayer player)
        {
            // Dashing through the shockwave avoids the push
            if (player.StateMachine.State == Player.StDash)
                return;

            player.Speed.X = -100f;
            if (player.Speed.Y > 30f)
                player.Speed.Y = 30f;

            if (!hasHitPlayer)
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("guid://{f64177a2-2611-49cc-8c63-d74c1e29c8a3}", player.Position);
                hasHitPlayer = true;
            }
        }
    }
}

