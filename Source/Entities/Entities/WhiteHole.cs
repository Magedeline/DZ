namespace Celeste.Entities
{
	[CustomEntity(new string[] { "DZ/WhiteHole" })]
	public class WhiteHole : Entity
	{
		private global::Celeste.Player player;
		private Hole hole;
		private float speedModifier;
		private float forceModifier;
		// Exit world-position defined by the first node in Loenn.
		// Falls back to a same-screen offset when no node is placed.
		private Vector2 exitPosition;
		// Direction the player is launched on transport (normalised from entry→exit).
		private Vector2 exitLaunchDir;
		private const float LaunchSpeed = 240f;
		// Cooldown so the hole doesn't teleport the player repeatedly.
		private float transportCooldown;

		public static void Load() { }
		public static void Unload() { }

		public WhiteHole(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			speedModifier = float.Parse(data.Attr("SpeedModifier", "1.02"));
			forceModifier = float.Parse(data.Attr("ForceModifier", "0.8"));

			// First node = exit position; fall back to 200 px to the right.
			exitPosition = data.Nodes.Length > 0
				? data.Nodes[0] + offset
				: Position + new Vector2(200f, 0f);

			var dir = exitPosition - Position;
			exitLaunchDir = dir == Vector2.Zero ? Vector2.UnitX : Vector2.Normalize(dir);

			Sprite sprite = new Sprite(GFX.Game, "Whitehole/");
			sprite.AddLoop("whitehole", "Whitehole", 0.1f);
			sprite.Play("whitehole");
			Add((Component)sprite);
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height / 2f;
			Collider = (Collider)new Monocle.Circle(48f);
			Add((Component)(hole = new Hole(true, true)));
			hole.HoleCollider = new Monocle.Circle(8f);
		}

		public override void Update()
		{
			base.Update();
			if (transportCooldown > 0f)
			{
				transportCooldown -= Engine.DeltaTime;
				return;
			}
			if (!CollideCheck<global::Celeste.Player>())
				return;
			player = CollideFirst<global::Celeste.Player>();
			if (player != null)
			{
				drag(player);
				holeTransport(player);
			}
		}

		private void holeTransport(global::Celeste.Player player)
		{
			if (!hole.Check(player))
				return;
			// Warp to exit, launch in direction of entry→exit.
			player.Position = exitPosition;
			player.Speed    = exitLaunchDir * LaunchSpeed;
			// Play a warp sound if available; fall back to silence gracefully.
			Audio.Play("event:/game/09_core/gate_advance_side", player.Position);
			// Short cooldown prevents immediate re-entry if exit overlaps an entry.
			transportCooldown = 0.5f;
			player.StateMachine.State = global::Celeste.Player.StNormal;
		}

		private void drag(global::Celeste.Player player)
		{
			Vector2 vector2 = (Position - player.Position) * forceModifier;
			player.Speed *= speedModifier;
			player.Speed += vector2;
		}
	}
}




