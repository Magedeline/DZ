namespace Celeste.Entities
{
  [CustomEntity(ids: "MaggyHelper/AmbienceTrigger")]
  [Tracked(true)]
  [HotReloadable]
  internal class AmbienceTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
  {
    private readonly string ambience = data.Attr(nameof(ambience), "event:/new_content/env/10_rain");
    // Retrieve the ambience attribute from the entity data

    public override void OnEnter(global::Celeste.Player player)
    {
      base.OnEnter(player);
      // Get the current session and apply the ambience event
      Level level = SceneAs<Level>();
      if (level?.Session?.Audio != null)
      {
        level.Session.Audio.Ambience.Event = SFX.EventnameByHandle(ambience);
        level.Session.Audio.Apply();
      }
    }
  }
}




