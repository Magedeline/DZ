// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_CPIntro
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_CPIntro : CutsceneEntity
{
  private Level level;
  private Player player;
  private SoundSource sfx;

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    ((Entity) this).Add((Component) (this.sfx = new SoundSource()));
  }

  public override void OnBegin(Level level)
  {
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
    this.player.StateMachine.State = 11;
    this.player.StateMachine.Locked = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    this.sfx.Play("event:/ricky06/CP-OSTIntroCutscene", (string) null, 0.0f);
    yield return (object) 1f;
    Rectangle bounds = this.level.Bounds;
    double num = (double) bounds.Right - 320.0;
    bounds = this.level.Bounds;
    double top = (double) bounds.Top;
    yield return (object) CutsceneEntity.CameraTo(new Vector2((float) num, (float) top), 18f, Ease.SineInOut, 0.0f);
    yield return (object) 2f;
    yield return (object) this.level.ZoomBack(2f);
    this.EndCutscene(this.level, true);
  }

  public override void OnEnd(Level level)
  {
    if (this.player != null)
    {
      this.player.StateMachine.Locked = false;
      this.player.StateMachine.State = 0;
      this.player.Speed.Y = 0.0f;
      while (((Entity) this.player).CollideCheck<Solid>())
      {
        Player player = this.player;
        ((Entity) player).Y = ((Entity) player).Y - 1f;
      }
    }
    level.ResetZoom();
    level.CameraOffset = new Vector2(48f, -32f);
    level.Session.SetFlag("cp_intro_pan", true);
  }

  public CS_CPIntro()
    : base(true, false)
  {
  }
}
