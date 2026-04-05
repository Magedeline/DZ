// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.ConquerorHealth
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/ConquerorHealth"})]
[Tracked(false)]
internal class ConquerorHealth : Entity
{
  private Sprite baseBar;
  private List<ConquerorHealth.HealthSlice> healthSliceList;
  private bool moreHealth;
  private Level level;
  private int healthAmount;
  private float pixelSize;

  public ConquerorHealth(bool smallHealth)
  {
    this.Add((Component) (this.baseBar = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.GuiBank.Create("bossBaseBar")));
    this.Tag = ((int) (Tags.HUD));
    this.moreHealth = smallHealth;
    this.healthSliceList = new List<ConquerorHealth.HealthSlice>();
    this.healthAmount = this.moreHealth ? 75 : 40;
    this.pixelSize = this.moreHealth ? 8f : 15f;
    string tag = this.moreHealth ? "healthSliceSmall" : "healthSliceBig";
    for (int index = 0; index < this.healthAmount; ++index)
      this.healthSliceList.Add(new ConquerorHealth.HealthSlice(tag));
  }

  public virtual void Awake(Scene scene)
  {
    base.Awake(scene);
    this.level = this.SceneAs<Level>();
    this.Position = new Vector2(1200f, 950f);
    this.Add((Component) new Coroutine(this.CreateBar(), true));
  }

  private IEnumerator CreateBar()
  {
    this.baseBar.Play("appear", false, false);
    yield return (object) 0.5f;
    this.baseBar.Play("idle", false, false);
    float offset = this.moreHealth ? 656f : 649f;
    for (int i = 0; i < this.healthAmount; ++i)
    {
      if (i < this.healthSliceList.Count)
      {
        ConquerorHealth.HealthSlice hs = this.healthSliceList[i];
        ((Scene) this.level).Add((Entity) hs);
        hs.drawSlice(new Vector2(this.X + (offset - (float) i * this.pixelSize), this.Y + 34f));
        yield return (object) null;
        hs = (ConquerorHealth.HealthSlice) null;
      }
    }
  }

  public void removeHealth()
  {
    this.healthSliceList[this.healthSliceList.Count - 1].removeSlice();
    this.healthSliceList.RemoveAt(this.healthSliceList.Count - 1);
  }

  public virtual void Render()
  {
    base.Render();
    if (this.Scene.Paused)
      ((Component) this.baseBar).Visible = false;
    else
      ((Component) this.baseBar).Visible = true;
  }

  private class HealthSlice : Entity
  {
    private Sprite slice;

    public HealthSlice(string tag)
    {
      this.Add((Component) (this.slice = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.GuiBank.Create(tag)));
      this.Tag = ((int) (Tags.HUD));
    }

    public void drawSlice(Vector2 position)
    {
      this.Position = position;
      this.slice.Play("idle", false, false);
    }

    public void removeSlice() => this.Add((Component) new Coroutine(this.removeRoutine(), true));

    private IEnumerator removeRoutine()
    {
      this.slice.Play("flash", false, false);
      yield return (object) 0.22f;
      this.RemoveSelf();
    }

    public virtual void Render()
    {
      base.Render();
      if (this.Scene.Paused)
        ((Component) this.slice).Visible = false;
      else
        ((Component) this.slice).Visible = true;
    }
  }
}
