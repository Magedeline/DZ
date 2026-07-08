// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.NPC_Boss
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/NPC_Boss"})]
[Tracked(false)]
internal class NPC_Boss : NPC
{
  private string cutsceneType;
  public bool lightningVisible;
  public Sprite lightning;
  private Level level;
  public static ParticleType P_Explode = new ParticleType()
  {
    Color = Color.White,
    Size = 1f,
    FadeMode = (ParticleType.FadeModes) 2,
    SpeedMin = 300f,
    SpeedMax = 600f,
    DirectionRange = 3.14159274f,
    LifeMin = 0.1f,
    LifeMax = 0.2f
  };

  public NPC_Boss(Vector2 position, string cutscene)
    : base(position)
  {
    ((Entity) this).Add((Component) (this.Sprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create("boss_sprite")));
    ((Entity) this).Add((Component) (this.lightning = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create("boss_lightningDZ_CHarge")));
    ((Component) this.lightning).Visible = false;
    this.lightning.OnFinish = (Action<string>) (_param1 => this.lightningVisible = false);
    ((Entity) this).Visible = false;
    this.cutsceneType = cutscene;
    ((Entity) this).Depth = -12000;
  }

  public NPC_Boss(EntityData data, Vector2 offset)
    : this(((data.Position) + (offset)), data.Attr("cutscene", ""))
  {
  }

  public override void Render()
  {
    ((Entity) this).Render();
    if (!this.lightningVisible)
      return;
    ((GraphicsComponent) this.lightning).RenderPosition = new Vector2(this.level.Camera.Left, ((Entity) this).Top + 8f);
    ((Component) this.lightning).Render();
  }

  public void invokeIntroScreech(bool hardMode)
  {
    if (this.Session.GetFlag("cb_screech_intro"))
      ((Entity) this).RemoveSelf();
    else
      ((Entity) this).Scene.Add((Entity) new CS_BossScreechBegin((NPC) this, hardMode));
  }

  public void EmitParticles()
  {
    ((Entity) this).Add((Component) new Coroutine(this.ParticlesCoroutine(), true));
  }

  private IEnumerator ParticlesCoroutine()
  {
    Vector2 temp = ((Entity) this).Position;
    for (int i = 0; i < 20; ++i)
    {
      float dir = Calc.NextAngle(Calc.Random);
      ((Entity) this).SceneAs<Level>().Particles.Emit(NPC_Boss.P_Explode, 5, ((temp) + (Calc.AngleToVector(dir, 8f))), ((Vector2.One) * (10f)), dir);
      yield return (object) null;
    }
  }

  public void invokeDefeatFinal()
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity == null)
      return;
    ((Entity) this).Scene.Add((Entity) new CS_BossDefeated((NPC) this, entity));
  }

  public void invokeDefeat()
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity == null)
      return;
    ((Entity) this).Scene.Add((Entity) new CS_BossOver((NPC) this, entity));
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    switch (this.cutsceneType)
    {
      case "intro":
        if (this.Session.GetFlag("boss_intro_cutscene"))
        {
          ((Entity) this).RemoveSelf();
          break;
        }
        foreach (Entity entity in ((Entity) this).Scene.Tracker.GetEntities<ConquerorBoss>())
          entity.RemoveSelf();
        ((Entity) this).Scene.Add((Entity) new CS_BossIntro((NPC) this));
        break;
      case "powerup":
        if (this.Session.GetFlag("boss_powerup_cutscene"))
        {
          ((Entity) this).RemoveSelf();
          break;
        }
        ((Entity) this).Scene.Add((Entity) new CS_Powerup(((Entity) this).Scene.Tracker.GetEntity<Player>(), (NPC) this));
        break;
      case "fightScreech":
        break;
      default:
        if (this.Session.GetFlag("boss_intro_cutscene"))
        {
          ((Entity) this).RemoveSelf();
          break;
        }
        foreach (Entity entity in ((Entity) this).Scene.Tracker.GetEntities<ConquerorBoss>())
          entity.RemoveSelf();
        ((Entity) this).Scene.Add((Entity) new CS_BossIntro((NPC) this));
        break;
    }
  }
}
