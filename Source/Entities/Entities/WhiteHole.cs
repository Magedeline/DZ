namespace Celeste.Entities
{
	[CustomEntity(new string[] { "DZ/WhiteHole" })]
	public class WhiteHole : Entity
	{
		private global::Celeste.Player player;
		private Hole hole;
		private float speedModifier;
		private float forceModifier;

		public static void Load()
		{
			// Fix recursive call
			// WhiteHole.Load();
		}

		public WhiteHole(EntityData data, Vector2 offset)
: base(data.Position + offset)
		{
			this.speedModifier = float.Parse(data.Attr("SpeedModifier", "1.02"));
			this.forceModifier = float.Parse(data.Attr("ForceModifier", "0.8"));
			Sprite sprite = new Sprite(GFX.Game, "Whitehole/");
			sprite.AddLoop("whitehole", "Whitehole", 0.1f);
			sprite.Play("whitehole");
			this.Add((Component)sprite);
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height / 2f;
			this.Collider = (Collider)new Monocle.Circle(48f);
			this.Add((Component)(this.hole = new Hole(true, true)));
			this.hole.HoleCollider = new Monocle.Circle(8f);
		}

		public override void Update()
		{
			base.Update();
			if (!this.CollideCheck<global::Celeste.Player>())
				return;
			this.player = this.CollideFirst<global::Celeste.Player>();
			if (this.player != null)
			{
				this.drag(this.player);
				this.holeTransport(this.player);
			}
		}

		private void holeTransport(global::Celeste.Player player)
		{
			if (!this.hole.Check(player))
				return;
			Vector2 exitPosition = this.Position + new Vector2(100, 0); // Adjust the offset as needed
			player.Position = exitPosition;
			player.Speed = new Vector2(200, 0); // Adjust the speed as needed
		}

		private void drag(global::Celeste.Player player)
		{
			Vector2 vector2 = (this.Position - player.Position) * this.forceModifier;
			player.Speed *= this.speedModifier;
			player.Speed += vector2;
		}
	}
}




