// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_CPEnd
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_CPEnd : CutsceneEntity
{
  private Level level;
  private Player player;
  private BadelineDummy badeline;

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnBegin(Level level)
  {
    level.RegisterAreaComplete();
    ((Entity) this).Add((Component) new Coroutine(this.Cutscene(), true));
  }

  private IEnumerator Cutscene()
  {
    while (this.player == null)
    {
      this.player = ((Entity) this).Scene.Tracker.GetEntity<Player>();
      if (this.player == null)
        yield return (object) null;
      else
        break;
    }
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    yield return (object) 1f;
    Player player = this.player;
    Rectangle bounds1 = this.level.Bounds;
    double num1 = (double) bounds1.Right - 90.0;
    yield return (object) player.DummyWalkTo((float) num1, false, 0.25f, false);
    yield return (object) 0.5f;
    this.player.Dashes = 1;
    this.level.Session.Inventory.Dashes = 1;
    ((Scene) this.level).Add((Entity) (this.badeline = new BadelineDummy(((Entity) this.player).Center)));
    this.player.CreateSplitParticles();
    Input.Rumble((RumbleStrength) 0, (RumbleLength) 1);
    this.Level.Displacement.AddBurst(((Entity) this.player).Center, 0.4f, 8f, 32f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    ((GraphicsComponent) this.badeline.Sprite).Scale.X = -1f;
    global::Celeste.Audio.Play("event:/char/badeline/maddy_split", ((Entity) this.player).Position);
    yield return (object) this.badeline.FloatTo(((((Entity) this.player).Position) + (new Vector2(20f, 0.0f))), new int?(-1), false, false, false);
    yield return (object) 0.5f;
    this.player.DummyAutoAnimate = false;
    ((Sprite) this.player.Sprite).Play("sitDown", false, false);
    yield return (object) 3f;
    yield return (object) Textbox.Say("CP_END_DIALOGUE", new Func<IEnumerator>[2]
    {
      new Func<IEnumerator>(this.pauseDialogue1),
      new Func<IEnumerator>(this.pauseDialogue2)
    });
    Rectangle bounds2 = this.level.Bounds;
    double num2 = (double) bounds2.Right - 620.0;
    bounds2 = this.level.Bounds;
    double top = (double) bounds2.Top;
    yield return (object) CutsceneEntity.CameraTo(new Vector2((float) num2, (float) top), 6f, Ease.SineInOut, 1f);
    yield return (object) 2f;
    this.EndCutscene(this.level, true);
  }

  private IEnumerator pauseDialogue1()
  {
    yield return (object) 2f;
  }

  private IEnumerator pauseDialogue2()
  {
    yield return (object) 2f;
  }

  public override void OnEnd(Level level) => level.CompleteArea(false, false);

  public CS_CPEnd()
    : base(true, false)
  {
  }
}
