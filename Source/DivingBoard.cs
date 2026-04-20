namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/DivingBoard")]
    [HotReloadable]
    public class DivingBoard : JumpthruPlatform
    {
        private MTexture boardTexture;
        private MTexture baseTexture;
        private float rotation;
        private float springTimer;
        private Wiggler wiggler;
        private float launchSpeed;
        private bool playerOnBoard;

        public DivingBoard(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.launchSpeed = data.Float("launchSpeed", -300f);

            boardTexture = AtlasPathHelper.TryGetTexture("objects/divingBoard");
            baseTexture = AtlasPathHelper.TryGetTexture("objects/divingBoardBase");
            
            // Validate that required textures exist before loading them to avoid runtime exceptions.
            if (boardTexture == null || baseTexture == null)
            {
                throw new InvalidOperationException("Required diving board textures are missing for 'objects/MaggyHelper/divingBoard' or 'objects/divingBoard'.");
            }
            
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                rotation = v * 0.2f;
            }));
            
            Depth = -1;
        }

        public override void Update()
        {
            base.Update();
            
            global::Celeste.Player player = GetPlayerRider();
            bool wasOnBoard = playerOnBoard;
            playerOnBoard = player != null;
            
            if (playerOnBoard && !wasOnBoard)
            {
                // Player just landed on board
                wiggler.Start();
            }
            
            if (springTimer > 0f)
            {
                springTimer -= Engine.DeltaTime;
            }
            
            // Launch player when they jump
            if (player != null && Input.Jump.Pressed && springTimer <= 0f)
            {
                LaunchPlayer(player);
            }
        }

        private void LaunchPlayer(global::Celeste.Player player)
        {
            springTimer = 0.5f;
            wiggler.Start();
            
            // Apply vertical launch speed using SuperJump
            player.Speed.Y = launchSpeed;
            player.Jump(false, true);
            
            Audio.Play("event:/game/general/spring", Position);
            
            Level level = SceneAs<Level>();
            level.DirectionalShake(new Vector2(0, -1), 0.1f);
        }

        public override void Render()
        {
            // Draw base first
            baseTexture.DrawCentered(Position + new Vector2(12, 8));
            
            // Draw board with rotation applied
            Vector2 boardPos = Position + new Vector2(12, 0);
            boardTexture.DrawCentered(boardPos, Color.White, 1f, rotation);
        }
    }
}
