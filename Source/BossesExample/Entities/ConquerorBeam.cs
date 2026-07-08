// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.ConquerorBeam
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

internal class ConquerorBeam : Entity
{
  public static ParticleType P_Dissipate = new ParticleType(FinalBossBeam.P_Dissipate);
  public const float ChargeTime = 1.4f;
  public const float FollowTime = 0.9f;
  public const float ActiveTime = 0.12f;
  private const float AngleStartOffset = 100f;
  private const float RotationSpeed = 200f;
  private const float CollideCheckSep = 2f;
  private const float BeamLength = 2000f;
  private const float BeamStartDist = 12f;
  private const int BeamsDrawn = 15;
  private const float SideDarknessAlpha = 0.35f;
  private ConquerorBoss boss;
  private Player player;
  private Sprite beamSprite;
  private Sprite beamStartSprite;
  private float chargeTimer;
  private float followTimer;
  private float activeTimer;
  private float angle;
  private float beamAlpha;
  private float sideFadeAlpha;
  private Vector2 targetBeam;
  private VertexPositionColor[] fade = new VertexPositionColor[24];

  public ConquerorBeam()
  {
    this.Add((Component) (this.beamSprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create("beam")));
    this.beamSprite.OnLastFrame = (Action<string>) (anim =>
    {
      if (!(anim == "shoot"))
        return;
      this.Destroy();
    });
    this.Add((Component) (this.beamStartSprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create("beam_start")));
    ((Component) this.beamSprite).Visible = false;
    this.Depth = -1000000;
  }

  public ConquerorBeam Init(ConquerorBoss boss, Vector2 target, int pos = 0)
  {
    this.boss = boss;
    this.targetBeam = target;
    this.chargeTimer = 1f;
    this.followTimer = 0.9f;
    this.activeTimer = 0.12f;
    this.beamSprite.Play("charge", false, false);
    this.sideFadeAlpha = 0.0f;
    this.beamAlpha = 0.0f;
    int num = (double) target.Y <= (double) ((Entity) boss).Y + 16.0 ? 1 : -1;
    if ((double) target.X >= (double) ((Entity) boss).X)
      num *= -1;
    this.angle = Calc.Angle(((Entity) boss).Center, target);
    Vector2 vector2 = ((Calc.ClosestPointOnLine(((Entity) boss).Center, ((((Entity) boss).Center) + (Calc.AngleToVector(this.angle, 2000f))), target)) + (((Calc.SafeNormalize(Calc.Perpendicular(((target) - (((Entity) boss).Center))), 100f)) * ((float) num))));
    this.angle = Calc.Angle(((Entity) boss).Center, vector2);
    return this;
  }

  public override void Added(Scene scene) => base.Added(scene);

  public override void Update()
  {
    base.Update();
    this.player = this.Scene.Tracker.GetEntity<Player>();
    this.beamAlpha = Calc.Approach(this.beamAlpha, 1f, 2f * Engine.DeltaTime);
    if ((double) this.chargeTimer > 0.0)
    {
      this.sideFadeAlpha = Calc.Approach(this.sideFadeAlpha, 1f, Engine.DeltaTime);
      if (this.player != null && !this.player.Dead)
      {
        this.followTimer -= Engine.DeltaTime;
        this.chargeTimer -= Engine.DeltaTime;
        if ((double) this.followTimer > 0.0 && ((this.targetBeam) != (((Entity) this.boss).Center)))
          this.angle = Calc.Angle(((Entity) this.boss).Center, Calc.Approach(Calc.ClosestPointOnLine(((Entity) this.boss).Center, ((((Entity) this.boss).Center) + (Calc.AngleToVector(this.angle, 2000f))), this.targetBeam), this.targetBeam, 200f * Engine.DeltaTime));
        else if (this.beamSprite.CurrentAnimationID == "charge")
          this.beamSprite.Play("lock", false, false);
        if ((double) this.chargeTimer <= 0.0)
        {
          this.SceneAs<Level>().DirectionalShake(Calc.AngleToVector(this.angle, 1f), 0.15f);
          Input.Rumble((RumbleStrength) 1, (RumbleLength) 1);
          this.DissipateParticles();
        }
      }
    }
    else if ((double) this.activeTimer > 0.0)
    {
      this.sideFadeAlpha = Calc.Approach(this.sideFadeAlpha, 0.0f, Engine.DeltaTime * 8f);
      if (this.beamSprite.CurrentAnimationID != "shoot")
      {
        this.beamSprite.Play("shoot", false, false);
        this.beamStartSprite.Play("shoot", true, false);
      }
      this.activeTimer -= Engine.DeltaTime;
      if ((double) this.activeTimer > 0.0)
        this.PlayerCollideCheck();
    }
    if (!this.boss.dead)
      return;
    this.Destroy();
  }

  private void DissipateParticles()
  {
    Level level = this.SceneAs<Level>();
    Vector2 vector2_1 = ((level.Camera.Position) + (new Vector2(160f, 90f)));
    Vector2 vector2_2 = ((((Entity) this.boss).Center) + (Calc.AngleToVector(this.angle, 12f)));
    Vector2 vector2_3 = ((((Entity) this.boss).Center) + (Calc.AngleToVector(this.angle, 2000f)));
    Vector2 vector2_4 = Calc.SafeNormalize(Calc.Perpendicular(((vector2_3) - (vector2_2))));
    Vector2 vector2_5 = Calc.SafeNormalize(((vector2_3) - (vector2_2)));
    Vector2 vector2_6 = (((-(vector2_4))) * (1f));
    Vector2 vector2_7 = ((vector2_4) * (1f));
    float num1 = Calc.Angle(vector2_4);
    float num2 = Calc.Angle((-(vector2_4)));
    float num3 = Vector2.Distance(vector2_1, vector2_2) - 12f;
    Vector2 vector2_8 = Calc.ClosestPointOnLine(vector2_2, vector2_3, vector2_1);
    for (int index1 = 0; index1 < 200; index1 += 12)
    {
      for (int index2 = -1; index2 <= 1; index2 += 2)
      {
        level.ParticlesFG.Emit(ConquerorBeam.P_Dissipate, ((((((vector2_8) + (((vector2_5) * ((float) index1))))) + (((((vector2_4) * (2f))) * ((float) index2))))) + (Calc.Range(Calc.Random, vector2_6, vector2_7))), num1);
        level.ParticlesFG.Emit(ConquerorBeam.P_Dissipate, ((((((vector2_8) + (((vector2_5) * ((float) index1))))) - (((((vector2_4) * (2f))) * ((float) index2))))) + (Calc.Range(Calc.Random, vector2_6, vector2_7))), num2);
        if (index1 != 0 && (double) index1 < (double) num3)
        {
          level.ParticlesFG.Emit(ConquerorBeam.P_Dissipate, ((((((vector2_8) - (((vector2_5) * ((float) index1))))) + (((((vector2_4) * (2f))) * ((float) index2))))) + (Calc.Range(Calc.Random, vector2_6, vector2_7))), num1);
          level.ParticlesFG.Emit(ConquerorBeam.P_Dissipate, ((((((vector2_8) - (((vector2_5) * ((float) index1))))) - (((((vector2_4) * (2f))) * ((float) index2))))) + (Calc.Range(Calc.Random, vector2_6, vector2_7))), num2);
        }
      }
    }
  }

  private void PlayerCollideCheck()
  {
    Vector2 vector2_1 = ((((Entity) this.boss).Center) + (Calc.AngleToVector(this.angle, 12f)));
    Vector2 vector2_2 = ((((Entity) this.boss).Center) + (Calc.AngleToVector(this.angle, 2000f)));
    Vector2 vector2_3 = Calc.SafeNormalize(Calc.Perpendicular(((vector2_2) - (vector2_1))), 2f);
    Player player = (this.Scene.CollideFirst<Player>(((vector2_1) + (vector2_3)), ((vector2_2) + (vector2_3))) ?? this.Scene.CollideFirst<Player>(((vector2_1) - (vector2_3)), ((vector2_2) - (vector2_3)))) ?? this.Scene.CollideFirst<Player>(vector2_1, vector2_2);
    if (player == null)
      return;
    this.boss.PlayerTakeDamage(player);
  }

  public override void Render()
  {
    Vector2 vector2 = ((Entity) this.boss).Center;
    Vector2 vector = Calc.AngleToVector(this.angle, ((Image) this.beamSprite).Width);
    ((GraphicsComponent) this.beamSprite).Rotation = this.angle;
    ((GraphicsComponent) this.beamSprite).Color = ((Color.White) * (this.beamAlpha));
    ((GraphicsComponent) this.beamStartSprite).Rotation = this.angle;
    ((GraphicsComponent) this.beamStartSprite).Color = ((Color.White) * (this.beamAlpha));
    if (this.beamSprite.CurrentAnimationID == "shoot")
      vector2 = ((vector2) + (Calc.AngleToVector(this.angle, 8f)));
    for (int index = 0; index < 15; ++index)
    {
      ((GraphicsComponent) this.beamSprite).RenderPosition = vector2;
      ((Component) this.beamSprite).Render();
      vector2 = ((vector2) + (vector));
    }
    if (this.beamSprite.CurrentAnimationID == "shoot")
    {
      ((GraphicsComponent) this.beamStartSprite).RenderPosition = ((Entity) this.boss).Center;
      ((Component) this.beamStartSprite).Render();
    }
    GameplayRenderer.End();
    GameplayRenderer.Begin();
  }

  private void Quad(
    ref int v,
    Vector2 offset,
    Vector2 a,
    Vector2 b,
    Vector2 c,
    Vector2 d,
    Color ab,
    Color cd)
  {
    this.fade[v].Position.X = offset.X + a.X;
    this.fade[v].Position.Y = offset.Y + a.Y;
    this.fade[v++].Color = ab;
    this.fade[v].Position.X = offset.X + b.X;
    this.fade[v].Position.Y = offset.Y + b.Y;
    this.fade[v++].Color = ab;
    this.fade[v].Position.X = offset.X + c.X;
    this.fade[v].Position.Y = offset.Y + c.Y;
    this.fade[v++].Color = cd;
    this.fade[v].Position.X = offset.X + a.X;
    this.fade[v].Position.Y = offset.Y + a.Y;
    this.fade[v++].Color = ab;
    this.fade[v].Position.X = offset.X + c.X;
    this.fade[v].Position.Y = offset.Y + c.Y;
    this.fade[v++].Color = cd;
    this.fade[v].Position.X = offset.X + d.X;
    this.fade[v].Position.Y = offset.Y + d.Y;
    this.fade[v++].Color = cd;
  }

  public void Destroy() => this.RemoveSelf();
}
