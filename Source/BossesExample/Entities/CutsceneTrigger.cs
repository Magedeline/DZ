// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CutsceneTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/CutsceneTrigger"})]
[Tracked(false)]
internal class CutsceneTrigger : Trigger
{
  private Level level;
  private string cutscene;

  public CutsceneTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
  {
    this.cutscene = data.Attr(nameof (cutscene), "");
  }

  public override void Added(Scene scene)
  {
    ((Entity) this).Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnEnter(Player player)
  {
    base.OnEnter(player);
    if (this.cutscene.Length == 0 || this.cutscene == "intro")
    {
      if (!this.level.Session.GetFlag("cp_intro_pan"))
        ((Entity) this).Scene.Add((Entity) new CS_CPIntro());
      else
        this.level.CameraOffset = new Vector2(48f, -32f);
    }
    else if (this.cutscene == "end")
    {
      ((Entity) this).Scene.Add((Entity) new CS_CPEnd());
    }
    else
    {
      if (!(this.cutscene == "endmusic"))
        return;
      this.level.Session.Audio.Music.Event = "event:/ricky06/CP-OSTEndCutscene";
      this.level.Session.Audio.Apply(false);
    }
  }
}
