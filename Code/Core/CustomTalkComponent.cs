namespace Celeste
{
    [HotReloadable]
    public class CustomTalkComponent(Rectangle hitbox, Vector2 talkerOffset, Action<global::Celeste.Player> onTalk)
        : TalkComponent(hitbox, talkerOffset, onTalk)
    {
        public Rectangle Rectangle1 { get; } = hitbox;
        public Vector2 Vector5 { get; } = talkerOffset;
        public Action<global::Celeste.Player> Action = onTalk;
    }

    [HotReloadable]
    public partial class CustomTalkCompodent
    {
        public object StateMachine { get; set; }
        public Facings Facing { get; set; }
        public int Dashes { get; set; }
        public bool Dead { get; set; }
        public Vector2 Position { get; set; }
        public bool DummyAutoAnimate { get; set; }
        public object Sprite { get; set; }
        public static Vector2 Speed { get; set; }

        public bool OnGround()
        {
            // Assuming the StateMachine's state 21 indicates the player is on the ground
            return StateMachine != null && (int)StateMachine.GetType().GetProperty("State")?.GetValue(StateMachine) == 21;
        }
    }
}



