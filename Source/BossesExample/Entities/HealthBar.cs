// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.HealthBar
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/Healthbar"})]
[Tracked(false)]
internal class HealthBar : Entity
{
  private Sprite barSprite;
  private Level level;
  private List<HealthBar.StrawberryIcon> berryList;
  private int health;

  public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData data)
  {
    int amountHealth = data.Int("health", 5);
    return new HealthBar(amountHealth);
  }

  public HealthBar(int amountHealth)
  {
    this.Add((Component) (this.barSprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.GuiBank.Create("healthBar")));
    this.berryList = new List<HealthBar.StrawberryIcon>();
    for (int index = 1; index <= amountHealth; ++index)
      this.berryList.Add(new HealthBar.StrawberryIcon());
    this.health = amountHealth;
    this.Tag = ((int) (Tags.HUD));
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    this.level = this.SceneAs<Level>();
    this.Position = new Vector2(320f, 1020f);
    this.Add((Component) new Coroutine(this.drawBar(), true));
  }

  private IEnumerator drawBar()
  {
    this.barSprite.Play("createBar", false, false);
    yield return (object) 0.2f;
    for (int i = 0; i < this.health; ++i)
    {
      ((Scene) this.level).Add((Entity) this.berryList[i]);
      this.berryList[i].drawIcon(new Vector2((float) (190.0 + 130.0 * (double) i), 940f));
    }
  }

  public void DecreaseHealth()
  {
    if (this.berryList.Count > 0)
    {
      this.berryList[this.berryList.Count - 1].removeIcon();
      this.berryList.RemoveAt(this.berryList.Count - 1);
    }
    else
      Logger.Log("Health Render", "No StrawberryIcon to remove");
  }

  public override void Removed(Scene scene)
  {
    base.Removed(scene);
    foreach (Entity berry in this.berryList)
      berry.RemoveSelf();
    this.berryList.Clear();
  }

  public override void Render()
  {
    base.Render();
    if (this.Scene.Paused)
      ((Component) this.barSprite).Visible = false;
    else
      ((Component) this.barSprite).Visible = true;
  }

  private class StrawberryIcon : Entity
  {
    private Sprite berrySprite;

    public StrawberryIcon()
    {
      this.Add((Component) (this.berrySprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.GuiBank.Create("strawberryIcon")));
      this.Tag = ((int) (Tags.HUD));
    }

    public void drawIcon(Vector2 position)
    {
      this.Position = position;
      this.Add((Component) new Coroutine(this.drawRoutine(), true));
    }

    private IEnumerator drawRoutine()
    {
      this.berrySprite.Play("createStrawberry", false, false);
      yield return (object) 0.32f;
    }

    public void removeIcon() => this.Add((Component) new Coroutine(this.removeRoutine(), true));

    private IEnumerator removeRoutine()
    {
      this.berrySprite.Play("destroyStrawberry", false, false);
      yield return (object) 0.88f;
      this.RemoveSelf();
    }

    public override void Render()
    {
      base.Render();
      if (this.Scene.Paused)
        ((Component) this.berrySprite).Visible = false;
      else
        ((Component) this.berrySprite).Visible = true;
    }
  }
}
