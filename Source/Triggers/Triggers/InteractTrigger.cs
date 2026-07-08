using Celeste.Cutscenes;

namespace Celeste.Triggers;

// Note: Abstract class - cannot have [CustomEntity] attribute as Everest cannot instantiate abstract classes
[Tracked]
public abstract class IngesteInteractTrigger : Entity
{
  public const string FLAG_PREFIX = "it_";
  public TalkComponent Talker;
  public List<string> Events;
  private int eventIndex;
  private float timeout;
  private bool used;

  protected IngesteInteractTrigger() : base(Vector2.Zero)
  {
    Events = new List<string>();
    Collider = new Hitbox(0f, 0f);
    Talker = new TalkComponent(new Rectangle(0, 0, 0, 0), Vector2.Zero,
      new Action<global::Celeste.Player>(OnTalk));
    Talker.PlayerMustBeFacing = false;
  }

  private void OnTalk(global::Celeste.Player player)
  {
    if (used) return;

    var proceedToNextEvent = false;

    switch (Events[eventIndex])
    {
      case "ch2_poem":
        Scene.Add(new Cs02JournalMod(player));
        proceedToNextEvent = false;
        break;
      case "ch3_diary":
        Scene.Add(new Cs05Diary(player));
        proceedToNextEvent = false;
        break;
      case "ch3_guestbook":
        Scene.Add(new Cs05Guestbook(player));
        proceedToNextEvent = false;
        break;
      case "ch3_memo":
        Scene.Add(new Cs05Memo(player));
        proceedToNextEvent = false;
        break;
      case "ch5_mirror_reflection":
        Scene.Add(new CS05_Reflection1(player));
        proceedToNextEvent = true;
        break;
      case "ch5_see_maddy":
        Scene.Add(new Cs05SeeMaddy(player, 0));
        proceedToNextEvent = true;
        break;
      case "ch5_see_maddy_b":
        Scene.Add(new Cs05SeeMaddy(player, 1));
        proceedToNextEvent = true;
        break;
      case "ch5_maddy_phone":
        Scene.Add(new Cs05MaddyPhone(player, Center.X));
        proceedToNextEvent = true;
        break;
    }

    if (!proceedToNextEvent) return;

    (Scene as Level).Session.SetFlag(FLAG_PREFIX + Events[eventIndex]);
    eventIndex++;

    if (eventIndex >= Events.Count)
    {
      used = true;
      timeout = 0.25f;
    }
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    Session session = (scene as Level).Session;
    for (int index = 0; index < this.Events.Count; ++index)
    {
      if (session.GetFlag("it_" + this.Events[index]))
        ++this.eventIndex;
    }

    if (this.eventIndex >= this.Events.Count)
    {
      this.RemoveSelf();
    }
    else
    {
      if (this.Events[this.eventIndex] != "ch5_maddy_phone")
        return;
      scene.Add((Entity)new MaddyPhone(this.Position + new Vector2((float)((double)this.Width / 2.0 - 8.0), this.Height - 1f)));
    }
  }

  public override void Update()
  {
    if (this.used)
    {
      this.timeout -= Engine.DeltaTime;
      if ((double)this.timeout <= 0.0)
        this.RemoveSelf();
    }
    else
    {
      while (this.eventIndex < this.Events.Count &&
             (this.Scene as Level).Session.GetFlag("it_" + this.Events[this.eventIndex]))
        ++this.eventIndex;
      if (this.eventIndex >= this.Events.Count)
        this.RemoveSelf();
    }

    base.Update();
  }

  public override void Removed(Scene scene)
  {
    base.Removed(scene);
    // Custom logic to handle when the entity is removed from the scene  
    if (used)
    {
      // Reset or clean up any resources or states if necessary  
      Events.Clear();
      eventIndex = 0;
      timeout = 0;
    }
  }

  // NOTE: Load() is currently NOT called from DZModule.Load(), so this
  // hook is inert as of this commit. The static handler + Unload pair below is
  // here so that if/when we wire this into the Module's lifecycle, it can be
  // unregistered cleanly on mod reload. See REFACTOR_PLAN.md PR 1.
  //
  // FOOTGUN: the hook body iterates self.Entities right after Level's ctor
  // returns, which is BEFORE LevelLoader populates them. As written, the
  // foreach loop is a no-op every time. If we revive this, the right target is
  // probably On.Celeste.Level.LoadLevel or Everest.Events.Level.OnLoadLevel,
  // not Level.ctor.
  public static void Load()
  {
    On.Celeste.Level.ctor += OnLevelCtor;
  }

  public static void Unload()
  {
    On.Celeste.Level.ctor -= OnLevelCtor;
  }

  private static void OnLevelCtor(On.Celeste.Level.orig_ctor orig, Level self)
  {
    orig(self);

    foreach (var entity in self.Entities)
    {
      if (entity is IngesteInteractTrigger trigger)
      {
        trigger.Added(self);
      }
    }
  }
}



