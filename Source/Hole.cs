namespace MaggyHelper.Entities
{
	[Tracked(false)]
	public class Hole(bool active, bool visible) : Component(active, visible)
	{
		public Monocle.Circle HoleCollider;

		public bool Check(global::Celeste.Player player)
		{
			Collider collider = this.Entity.Collider;
			if (this.HoleCollider != null)
				this.Entity.Collider = (Collider)this.HoleCollider;
			bool flag = player.CollideCheck(this.Entity);
			this.Entity.Collider = collider;
			return flag;
		}
	}
}



