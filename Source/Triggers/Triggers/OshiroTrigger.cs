namespace Celeste.Triggers
{
    [CustomEntity(ids: "DZ/OshiroTrigger")]
    public class OshiroTrigger : Trigger
    {
        public bool State;
        private bool pendingRemove;

        public OshiroTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            State = data.Bool("state", true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            var level = Scene as Level;
            if (level == null)
                return;

            if (State)
            {
                var pos = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
                Scene.Add(new AngryOshiro(pos, false));
            }
            else
            {
                Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
            }

            pendingRemove = true;
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemove) RemoveSelf();
        }
    }
}




