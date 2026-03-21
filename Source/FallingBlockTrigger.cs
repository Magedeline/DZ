namespace MaggyHelper.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/FallingBlockTrigger" })]
  [Tracked(false)]
  internal class FallingBlockTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
  {
    public override void OnEnter(global::Celeste.Player player)
    {
      Level level = SceneAs<Level>();
      if (level.Session.GetFlag("c20-08-fallen"))
        return;
      level.Session.SetFlag("ch20-08-fallen");
    }
  }
}




