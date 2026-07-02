// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.NoResetMoveBlock
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/NoResetMoveBlock"})]
[Tracked(false)]
internal class NoResetMoveBlock : Solid
{
  public static ParticleType P_Activate = new ParticleType(MoveBlock.P_Activate);
  public static ParticleType P_Break = new ParticleType(MoveBlock.P_Break);
  public static ParticleType P_Move = new ParticleType(MoveBlock.P_Move);
  private const float Accel = 300f;
  private const float MoveSpeed = 60f;
  private const float FastMoveSpeed = 75f;
  private const float SteerSpeed = 50.2654839f;
  private const float MaxAngle = 0.7853982f;
  private const float NoSteerTime = 0.2f;
  private const float CrashTime = 0.15f;
  private const float CrashResetTime = 0.1f;
  private const float RegenTime = 3f;
  private bool canSteer;
  private bool fast;
  private NoResetMoveBlock.Directions direction;
  private float homeAngle;
  private int angleSteerSign;
  private Vector2 startPosition;
  private NoResetMoveBlock.MovementState state;
  private bool leftPressed;
  private bool rightPressed;
  private bool topPressed;
  private float speed;
  private float targetSpeed;
  private float angle;
  private float targetAngle;
  private Player noSquish;
  private List<Image> body = new List<Image>();
  private List<Image> topButton = new List<Image>();
  private List<Image> leftButton = new List<Image>();
  private List<Image> rightButton = new List<Image>();
  private List<MTexture> arrows = new List<MTexture>();
  private NoResetMoveBlock.Border border;
  private Color fillColor = NoResetMoveBlock.idleBgFill;
  private float flash;
  private SoundSource moveSfx;
  private bool triggered;
  private static readonly Color idleBgFill = Calc.HexToColor("6eefff");
  private static readonly Color pressedBgFill = Calc.HexToColor("d7ff38");
  private static readonly Color breakingBgFill = Calc.HexToColor("eb540e");
  private float particleRemainder;
  public EntityID eid;
  private bool superSlow;
  private float superSlowSpeed;
  public bool startedMoving;

  public NoResetMoveBlock(
    Vector2 position,
    int width,
    int height,
    NoResetMoveBlock.Directions direction,
    bool canSteer,
    bool fast,
    bool superSlow,
    float superSlowSpeed)
    : base(position, (float) width, (float) height, false)
  {
    ((Entity) this).Depth = -1;
    this.startPosition = position;
    this.canSteer = canSteer;
    this.direction = direction;
    this.fast = fast;
    this.superSlow = superSlow;
    this.superSlowSpeed = superSlowSpeed;
    switch (direction)
    {
      case NoResetMoveBlock.Directions.Left:
        this.homeAngle = this.targetAngle = this.angle = 3.14159274f;
        this.angleSteerSign = -1;
        break;
      case NoResetMoveBlock.Directions.Up:
        this.homeAngle = this.targetAngle = this.angle = -1.57079637f;
        this.angleSteerSign = 1;
        break;
      case NoResetMoveBlock.Directions.Down:
        this.homeAngle = this.targetAngle = this.angle = 1.57079637f;
        this.angleSteerSign = -1;
        break;
      default:
        this.homeAngle = this.targetAngle = this.angle = 0.0f;
        this.angleSteerSign = 1;
        break;
    }
    int num1 = width / 8;
    int num2 = height / 8;
    MTexture mtexture1 = GFX.Game["objects/DZ/DZ/nrMoveblock/base"];
    MTexture mtexture2 = GFX.Game["objects/DZ/DZ/nrMoveblock/button"];
    if (canSteer && (direction == NoResetMoveBlock.Directions.Left || direction == NoResetMoveBlock.Directions.Right))
    {
      for (int index = 0; index < num1; ++index)
      {
        int num3 = index != 0 ? (index < num1 - 1 ? 1 : 2) : 0;
        this.AddImage(mtexture2.GetSubtexture(num3 * 8, 0, 8, 8, (MTexture) null), new Vector2((float) (index * 8), -4f), 0.0f, new Vector2(1f, 1f), this.topButton);
      }
      mtexture1 = GFX.Game["objects/DZ/DZ/nrMoveblock/base_h"];
    }
    else if (canSteer && (direction == NoResetMoveBlock.Directions.Up || direction == NoResetMoveBlock.Directions.Down))
    {
      for (int index = 0; index < num2; ++index)
      {
        int num4 = index != 0 ? (index < num2 - 1 ? 1 : 2) : 0;
        this.AddImage(mtexture2.GetSubtexture(num4 * 8, 0, 8, 8, (MTexture) null), new Vector2(-4f, (float) (index * 8)), 1.57079637f, new Vector2(1f, -1f), this.leftButton);
        this.AddImage(mtexture2.GetSubtexture(num4 * 8, 0, 8, 8, (MTexture) null), new Vector2((float) ((num1 - 1) * 8 + 4), (float) (index * 8)), 1.57079637f, new Vector2(1f, 1f), this.rightButton);
      }
      mtexture1 = GFX.Game["objects/DZ/DZ/nrMoveblock/base_v"];
    }
    for (int index1 = 0; index1 < num1; ++index1)
    {
      for (int index2 = 0; index2 < num2; ++index2)
      {
        int num5 = index1 != 0 ? (index1 < num1 - 1 ? 1 : 2) : 0;
        int num6 = index2 != 0 ? (index2 < num2 - 1 ? 1 : 2) : 0;
        this.AddImage(mtexture1.GetSubtexture(num5 * 8, num6 * 8, 8, 8, (MTexture) null), ((new Vector2((float) index1, (float) index2)) * (8f)), 0.0f, new Vector2(1f, 1f), this.body);
      }
    }
    this.arrows = GFX.Game.GetAtlasSubtextures("objects/DZ/DZ/nrMoveblock/arrow");
    ((Entity) this).Add((Component) (this.moveSfx = new SoundSource()));
    ((Entity) this).Add((Component) new Coroutine(this.Controller(), true));
    this.UpdateColors();
    ((Entity) this).Add((Component) new LightOcclude(0.5f));
  }

  public NoResetMoveBlock(EntityData data, Vector2 offset, EntityID id)
    : this(((data.Position) + (offset)), data.Width, data.Height, data.Enum<NoResetMoveBlock.Directions>(nameof (direction), NoResetMoveBlock.Directions.Left), data.Bool(nameof (canSteer), true), data.Bool(nameof (fast), false), data.Bool(nameof (superSlow), false), data.Float(nameof (superSlowSpeed), 0.0f))
  {
    this.eid = id;
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    scene.Add((Entity) (this.border = new NoResetMoveBlock.Border(this)));
  }

  public void makeGlobal() => ((Entity) this).AddTag(((int) (Tags.Global)));

  public void removeGlobal() => ((Entity) this).RemoveTag(((int) (Tags.Global)));

  private IEnumerator Controller()
  {
    while (true)
    {
      this.triggered = false;
      this.state = NoResetMoveBlock.MovementState.Idling;
      while (!this.triggered && !this.HasPlayerRider())
        yield return (object) null;
      global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_activate", ((Entity) this).Position);
      this.startedMoving = true;
      this.state = NoResetMoveBlock.MovementState.Moving;
      ((Platform) this).StartShaking(0.2f);
      this.ActivateParticles();
      yield return (object) 0.2f;
      this.targetSpeed = this.fast ? 75f : 60f;
      if (this.superSlow)
        this.targetSpeed = this.superSlowSpeed;
      this.moveSfx.Play("event:/game/04_cliffside/arrowblock_move", (string) null, 0.0f);
      this.moveSfx.Param("arrow_stop", 0.0f);
      this.StopPlayerRunIntoAnimation = false;
      float crashTimer = 0.15f;
      float crashResetTimer = 0.1f;
      float noSteerTimer = 0.2f;
      while (true)
      {
        if (this.canSteer)
        {
          this.targetAngle = this.homeAngle;
          bool flag = this.direction == NoResetMoveBlock.Directions.Right || this.direction == NoResetMoveBlock.Directions.Left ? this.HasPlayerOnTop() : this.HasPlayerClimbing();
          if (flag && (double) noSteerTimer > 0.0)
            noSteerTimer -= Engine.DeltaTime;
          if (flag)
          {
            if ((double) noSteerTimer <= 0.0)
              this.targetAngle = this.direction != NoResetMoveBlock.Directions.Right && this.direction != NoResetMoveBlock.Directions.Left ? this.homeAngle + 0.7853982f * (float) this.angleSteerSign * (float) Input.MoveX.Value : this.homeAngle + 0.7853982f * (float) this.angleSteerSign * (float) Input.MoveY.Value;
          }
          else
            noSteerTimer = 0.2f;
        }
        if (((Entity) this).Scene.OnInterval(0.02f))
          this.MoveParticles();
        this.speed = Calc.Approach(this.speed, this.targetSpeed, 300f * Engine.DeltaTime);
        this.angle = Calc.Approach(this.angle, this.targetAngle, 50.2654839f * Engine.DeltaTime);
        Vector2 vector = Calc.AngleToVector(this.angle, this.speed);
        Vector2 vec = ((vector) * (Engine.DeltaTime));
        bool flag2;
        Rectangle bounds;
        if (this.direction == NoResetMoveBlock.Directions.Right || this.direction == NoResetMoveBlock.Directions.Left)
        {
          flag2 = this.MoveCheck(Calc.XComp(vec));
          this.noSquish = ((Entity) this).Scene.Tracker.GetEntity<Player>();
          ((Platform) this).MoveVCollideSolids(vec.Y, false, (Action<Vector2, Vector2, Platform>) null);
          this.noSquish = (Player) null;
          ((Platform) this).LiftSpeed = vector;
          if (((Entity) this).Scene.OnInterval(0.03f))
          {
            if ((double) vec.Y > 0.0)
              this.ScrapeParticles(Vector2.UnitY);
            else if ((double) vec.Y < 0.0)
              this.ScrapeParticles((-(Vector2.UnitY)));
          }
        }
        else
        {
          flag2 = this.MoveCheck(Calc.YComp(vec));
          this.noSquish = ((Entity) this).Scene.Tracker.GetEntity<Player>();
          ((Platform) this).MoveHCollideSolids(vec.X, false, (Action<Vector2, Vector2, Platform>) null);
          this.noSquish = (Player) null;
          ((Platform) this).LiftSpeed = vector;
          if (((Entity) this).Scene.OnInterval(0.03f))
          {
            if ((double) vec.X > 0.0)
              this.ScrapeParticles(Vector2.UnitX);
            else if ((double) vec.X < 0.0)
              this.ScrapeParticles((-(Vector2.UnitX)));
          }
          int num1;
          if (this.direction == NoResetMoveBlock.Directions.Down)
          {
            double top = (double) ((Entity) this).Top;
            bounds = ((Entity) this).SceneAs<Level>().Bounds;
            double num2 = (double) (bounds.Bottom + 32 /*0x20*/);
            num1 = top > num2 ? 1 : 0;
          }
          else
            num1 = 0;
          if (num1 != 0)
            flag2 = true;
        }
        if (flag2)
        {
          this.moveSfx.Param("arrow_stop", 1f);
          crashResetTimer = 0.1f;
          if ((double) crashTimer > 0.0)
            crashTimer -= Engine.DeltaTime;
          else
            break;
        }
        else
        {
          this.moveSfx.Param("arrow_stop", 0.0f);
          if ((double) crashResetTimer > 0.0)
            crashResetTimer -= Engine.DeltaTime;
          else
            crashTimer = 0.15f;
        }
        Level level = ((Entity) this).Scene as Level;
        double left1 = (double) ((Entity) this).Left;
        bounds = level.Bounds;
        double left2 = (double) bounds.Left;
        int num;
        if (left1 >= left2)
        {
          double top1 = (double) ((Entity) this).Top;
          bounds = level.Bounds;
          double top2 = (double) bounds.Top;
          if (top1 >= top2)
          {
            double right1 = (double) ((Entity) this).Right;
            bounds = level.Bounds;
            double right2 = (double) bounds.Right;
            num = right1 > right2 ? 1 : 0;
            goto label_45;
          }
        }
        num = 1;
label_45:
        if (num == 0)
        {
          yield return (object) null;
          vector = new Vector2();
          vec = new Vector2();
          level = (Level) null;
        }
        else
          break;
      }
      global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_break", ((Entity) this).Position);
      this.moveSfx.Stop(true);
      this.state = NoResetMoveBlock.MovementState.Breaking;
      this.speed = this.targetSpeed = 0.0f;
      this.angle = this.targetAngle = this.homeAngle;
      ((Platform) this).StartShaking(0.2f);
      this.StopPlayerRunIntoAnimation = true;
      yield return (object) 0.2f;
      this.BreakParticles();
      List<NoResetMoveBlock.Debris> debris = new List<NoResetMoveBlock.Debris>();
      for (int i = 0; (double) i < (double) ((Entity) this).Width; i += 8)
      {
        for (int j = 0; (double) j < (double) ((Entity) this).Height; j += 8)
        {
          Vector2 vector2 = new Vector2((float) i + 4f, (float) j + 4f);
          NoResetMoveBlock.Debris debris2 = Engine.Pooler.Create<NoResetMoveBlock.Debris>().Init(((((Entity) this).Position) + (vector2)), ((Entity) this).Center, ((this.startPosition) + (vector2)));
          debris.Add(debris2);
          ((Entity) this).Scene.Add((Entity) debris2);
          vector2 = new Vector2();
          debris2 = (NoResetMoveBlock.Debris) null;
        }
      }
      ((Platform) this).MoveStaticMovers(((this.startPosition) - (((Entity) this).Position)));
      ((Platform) this).DisableStaticMovers();
      ((Entity) this).Position = this.startPosition;
      ((Entity) this).Visible = ((Entity) this).Collidable = false;
      yield return (object) 2.2f;
      foreach (NoResetMoveBlock.Debris item in debris)
        item.StopMoving();
      while (((Entity) this).CollideCheck<Actor>() || ((Entity) this).CollideCheck<Solid>())
        yield return (object) null;
      ((Entity) this).Collidable = true;
      EventInstance instance = global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", ((Entity) debris[0]).Position);
      NoResetMoveBlock moveBlock = this;
      Coroutine component;
      Coroutine routine = component = new Coroutine(this.SoundFollowsDebrisCenter(instance, debris), true);
      ((Entity) moveBlock).Add((Component) component);
      foreach (NoResetMoveBlock.Debris item2 in debris)
        item2.StartShaking();
      yield return (object) 0.2f;
      foreach (NoResetMoveBlock.Debris item3 in debris)
        item3.ReturnHome(0.65f);
      yield return (object) 0.6f;
      ((Component) routine).RemoveSelf();
      foreach (NoResetMoveBlock.Debris item4 in debris)
        ((Entity) item4).RemoveSelf();
      global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_reappear", ((Entity) this).Position);
      ((Entity) this).Visible = true;
      ((Platform) this).EnableStaticMovers();
      this.speed = this.targetSpeed = 0.0f;
      this.angle = this.targetAngle = this.homeAngle;
      this.noSquish = (Player) null;
      this.fillColor = NoResetMoveBlock.idleBgFill;
      this.UpdateColors();
      this.flash = 1f;
      debris = (List<NoResetMoveBlock.Debris>) null;
      instance = (EventInstance) null;
      moveBlock = (NoResetMoveBlock) null;
      component = (Coroutine) null;
      routine = (Coroutine) null;
    }
  }

  private IEnumerator SoundFollowsDebrisCenter(
    EventInstance instance,
    List<NoResetMoveBlock.Debris> debris)
  {
    while (true)
    {
      PLAYBACK_STATE pLAYBACK_STATE;
      instance.getPlaybackState(out pLAYBACK_STATE);
      if (pLAYBACK_STATE != PLAYBACK_STATE.STOPPED)
      {
        Vector2 zero = Vector2.Zero;
        foreach (NoResetMoveBlock.Debris debri in debris)
          zero = ((zero) + (((Entity) debri).Position));
        zero = ((zero) / ((float) debris.Count));
        global::Celeste.Audio.Position(instance, zero);
        yield return (object) null;
        zero = new Vector2();
      }
      else
        break;
    }
  }

  public override void Update()
  {
    base.Update();
    if (this.canSteer)
    {
      bool flag1 = (this.direction == NoResetMoveBlock.Directions.Up || this.direction == NoResetMoveBlock.Directions.Down) && ((Entity) this).CollideCheck<Player>(((((Entity) this).Position) + (new Vector2(-1f, 0.0f))));
      bool flag2 = (this.direction == NoResetMoveBlock.Directions.Up || this.direction == NoResetMoveBlock.Directions.Down) && ((Entity) this).CollideCheck<Player>(((((Entity) this).Position) + (new Vector2(1f, 0.0f))));
      bool flag3 = (this.direction == NoResetMoveBlock.Directions.Left || this.direction == NoResetMoveBlock.Directions.Right) && ((Entity) this).CollideCheck<Player>(((((Entity) this).Position) + (new Vector2(0.0f, -1f))));
      foreach (GraphicsComponent graphicsComponent in this.topButton)
        graphicsComponent.Y = flag3 ? 2f : 0.0f;
      foreach (GraphicsComponent graphicsComponent in this.leftButton)
        graphicsComponent.X = flag1 ? 2f : 0.0f;
      foreach (GraphicsComponent graphicsComponent in this.rightButton)
        graphicsComponent.X = ((Entity) this).Width + (flag2 ? -2f : 0.0f);
      if (flag1 && !this.leftPressed || flag3 && !this.topPressed || flag2 && !this.rightPressed)
        global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", ((Entity) this).Position);
      if (!flag1 && this.leftPressed || !flag3 && this.topPressed || !flag2 && this.rightPressed)
        global::Celeste.Audio.Play("event:/game/04_cliffside/arrowblock_side_release", ((Entity) this).Position);
      this.leftPressed = flag1;
      this.rightPressed = flag2;
      this.topPressed = flag3;
    }
    if (this.moveSfx != null && this.moveSfx.Playing)
      this.moveSfx.Param("arrow_influence", (float) ((int) Math.Floor((0.0 - (double) Calc.Angle(((Calc.AngleToVector(this.angle, 1f)) * (new Vector2(-1f, 1f)))) + 6.2831854820251465) % 6.2831854820251465 / 6.2831854820251465 * 8.0 + 0.5) + 1));
    this.border.Visible = ((Entity) this).Visible;
    this.flash = Calc.Approach(this.flash, 0.0f, Engine.DeltaTime * 5f);
    this.UpdateColors();
  }

  public override void OnStaticMoverTrigger(StaticMover sm) => this.triggered = true;

  public override void MoveHExact(int move)
  {
    if (this.noSquish != null && (move < 0 && (double) ((Entity) this.noSquish).X < (double) ((Entity) this).X || move > 0 && (double) ((Entity) this.noSquish).X > (double) ((Entity) this).X))
    {
      while (move != 0 && ((Entity) this.noSquish).CollideCheck<Solid>(((((Entity) this.noSquish).Position) + (((Vector2.UnitX) * ((float) move))))))
        move -= Math.Sign(move);
    }
    base.MoveHExact(move);
  }

  public override void MoveVExact(int move)
  {
    if (this.noSquish != null && move < 0 && (double) ((Entity) this.noSquish).Y <= (double) ((Entity) this).Y)
    {
      while (move != 0 && ((Entity) this.noSquish).CollideCheck<Solid>(((((Entity) this.noSquish).Position) + (((Vector2.UnitY) * ((float) move))))))
        move -= Math.Sign(move);
    }
    base.MoveVExact(move);
  }

  private bool MoveCheck(Vector2 speed)
  {
    if ((double) speed.X != 0.0)
    {
      if (!((Platform) this).MoveHCollideSolids(speed.X, false, (Action<Vector2, Vector2, Platform>) null))
        return false;
      for (int index1 = 1; index1 <= 3; ++index1)
      {
        for (int index2 = 1; index2 >= -1; index2 -= 2)
        {
          Vector2 vector2 = new Vector2((float) Math.Sign(speed.X), (float) (index1 * index2));
          if (!((Entity) this).CollideCheck<Solid>(((((Entity) this).Position) + (vector2))))
          {
            ((Platform) this).MoveVExact(index1 * index2);
            ((Platform) this).MoveHExact(Math.Sign(speed.X));
            return false;
          }
        }
      }
      return true;
    }
    if ((double) speed.Y == 0.0 || !((Platform) this).MoveVCollideSolids(speed.Y, false, (Action<Vector2, Vector2, Platform>) null))
      return false;
    for (int index3 = 1; index3 <= 3; ++index3)
    {
      for (int index4 = 1; index4 >= -1; index4 -= 2)
      {
        Vector2 vector2 = new Vector2((float) (index3 * index4), (float) Math.Sign(speed.Y));
        if (!((Entity) this).CollideCheck<Solid>(((((Entity) this).Position) + (vector2))))
        {
          ((Platform) this).MoveHExact(index3 * index4);
          ((Platform) this).MoveVExact(Math.Sign(speed.Y));
          return false;
        }
      }
    }
    return true;
  }

  private void UpdateColors()
  {
    Color color = NoResetMoveBlock.idleBgFill;
    if (this.state == NoResetMoveBlock.MovementState.Moving)
      color = NoResetMoveBlock.pressedBgFill;
    else if (this.state == NoResetMoveBlock.MovementState.Breaking)
      color = NoResetMoveBlock.breakingBgFill;
    this.fillColor = Color.Lerp(this.fillColor, color, 10f * Engine.DeltaTime);
    foreach (GraphicsComponent graphicsComponent in this.topButton)
      graphicsComponent.Color = this.fillColor;
    foreach (GraphicsComponent graphicsComponent in this.leftButton)
      graphicsComponent.Color = this.fillColor;
    foreach (GraphicsComponent graphicsComponent in this.rightButton)
      graphicsComponent.Color = this.fillColor;
  }

  private void AddImage(
    MTexture tex,
    Vector2 position,
    float rotation,
    Vector2 scale,
    List<Image> addTo)
  {
    Image image = new Image(tex);
    ((GraphicsComponent) image).Position = ((position) + (new Vector2(4f, 4f)));
    image.CenterOrigin();
    ((GraphicsComponent) image).Rotation = rotation;
    ((GraphicsComponent) image).Scale = scale;
    ((Entity) this).Add((Component) image);
    addTo?.Add(image);
  }

  private void SetVisible(List<Image> images, bool visible)
  {
    foreach (Component image in images)
      image.Visible = visible;
  }

  public override void Render()
  {
    Vector2 position = ((Entity) this).Position;
    ((Entity) this).Position = ((((Entity) this).Position) + (((Platform) this).Shake));
    foreach (Component component in this.leftButton)
      component.Render();
    foreach (Component component in this.rightButton)
      component.Render();
    foreach (Component component in this.topButton)
      component.Render();
    Draw.Rect(((Entity) this).X + 3f, ((Entity) this).Y + 3f, ((Entity) this).Width - 6f, ((Entity) this).Height - 6f, this.fillColor);
    foreach (Component component in this.body)
      component.Render();
    Draw.Rect(((Entity) this).Center.X - 4f, ((Entity) this).Center.Y - 4f, 8f, 8f, this.fillColor);
    if (this.state != NoResetMoveBlock.MovementState.Breaking)
      this.arrows[Calc.Clamp((int) Math.Floor((0.0 - (double) this.angle + 6.2831854820251465) % 6.2831854820251465 / 6.2831854820251465 * 8.0 + 0.5), 0, 7)].DrawCentered(((Entity) this).Center);
    else
      GFX.Game["objects/DZ/DZ/nrMoveblock/x"].DrawCentered(((Entity) this).Center);
    float num = this.flash * 4f;
    Draw.Rect(((Entity) this).X - num, ((Entity) this).Y - num, ((Entity) this).Width + num * 2f, ((Entity) this).Height + num * 2f, ((Color.White) * (this.flash)));
    ((Entity) this).Position = position;
  }

  private void ActivateParticles()
  {
    bool flag1 = this.direction == NoResetMoveBlock.Directions.Down || this.direction == NoResetMoveBlock.Directions.Up;
    bool flag2 = (!this.canSteer || !flag1) && !((Entity) this).CollideCheck<Player>(((((Entity) this).Position) - (Vector2.UnitX)));
    bool flag3 = (!this.canSteer || !flag1) && !((Entity) this).CollideCheck<Player>(((((Entity) this).Position) + (Vector2.UnitX)));
    bool flag4 = !this.canSteer | flag1 && !((Entity) this).CollideCheck<Player>(((((Entity) this).Position) - (Vector2.UnitY)));
    if (flag2)
      ((Entity) this).SceneAs<Level>().ParticlesBG.Emit(NoResetMoveBlock.P_Activate, (int) ((double) ((Entity) this).Height / 2.0), ((Entity) this).CenterLeft, ((((Vector2.UnitY) * (((Entity) this).Height - 4f))) * (0.5f)), 3.14159274f);
    if (flag3)
      ((Entity) this).SceneAs<Level>().ParticlesBG.Emit(NoResetMoveBlock.P_Activate, (int) ((double) ((Entity) this).Height / 2.0), ((Entity) this).CenterRight, ((((Vector2.UnitY) * (((Entity) this).Height - 4f))) * (0.5f)), 0.0f);
    if (flag4)
      ((Entity) this).SceneAs<Level>().ParticlesBG.Emit(NoResetMoveBlock.P_Activate, (int) ((double) ((Entity) this).Width / 2.0), ((Entity) this).TopCenter, ((((Vector2.UnitX) * (((Entity) this).Width - 4f))) * (0.5f)), -1.57079637f);
    ((Entity) this).SceneAs<Level>().ParticlesBG.Emit(NoResetMoveBlock.P_Activate, (int) ((double) ((Entity) this).Width / 2.0), ((Entity) this).BottomCenter, ((((Vector2.UnitX) * (((Entity) this).Width - 4f))) * (0.5f)), 1.57079637f);
  }

  private void BreakParticles()
  {
    Vector2 center = ((Entity) this).Center;
    for (int index1 = 0; (double) index1 < (double) ((Entity) this).Width; index1 += 4)
    {
      for (int index2 = 0; (double) index2 < (double) ((Entity) this).Height; index2 += 4)
      {
        Vector2 vector2 = ((((Entity) this).Position) + (new Vector2((float) (2 + index1), (float) (2 + index2))));
        ((Entity) this).SceneAs<Level>().Particles.Emit(NoResetMoveBlock.P_Break, 1, vector2, ((Vector2.One) * (2f)), Calc.Angle(((vector2) - (center))));
      }
    }
  }

  private void MoveParticles()
  {
    Vector2 vector2_1;
    Vector2 vector2_2;
    float num1;
    float num2;
    if (this.direction == NoResetMoveBlock.Directions.Right)
    {
      vector2_1 = ((((Entity) this).CenterLeft) + (Vector2.UnitX));
      vector2_2 = ((Vector2.UnitY) * (((Entity) this).Height - 4f));
      num1 = 3.14159274f;
      num2 = ((Entity) this).Height / 32f;
    }
    else if (this.direction == NoResetMoveBlock.Directions.Left)
    {
      vector2_1 = ((Entity) this).CenterRight;
      vector2_2 = ((Vector2.UnitY) * (((Entity) this).Height - 4f));
      num1 = 0.0f;
      num2 = ((Entity) this).Height / 32f;
    }
    else if (this.direction == NoResetMoveBlock.Directions.Down)
    {
      vector2_1 = ((((Entity) this).TopCenter) + (Vector2.UnitY));
      vector2_2 = ((Vector2.UnitX) * (((Entity) this).Width - 4f));
      num1 = -1.57079637f;
      num2 = ((Entity) this).Width / 32f;
    }
    else
    {
      vector2_1 = ((Entity) this).BottomCenter;
      vector2_2 = ((Vector2.UnitX) * (((Entity) this).Width - 4f));
      num1 = 1.57079637f;
      num2 = ((Entity) this).Width / 32f;
    }
    this.particleRemainder += num2;
    int particleRemainder = (int) this.particleRemainder;
    this.particleRemainder -= (float) particleRemainder;
    Vector2 vector2_3 = ((vector2_2) * (0.5f));
    if (particleRemainder <= 0)
      return;
    ((Entity) this).SceneAs<Level>().ParticlesBG.Emit(NoResetMoveBlock.P_Move, particleRemainder, vector2_1, vector2_3, num1);
  }

  private void ScrapeParticles(Vector2 dir)
  {
    int num1 = ((Entity) this).Collidable ? 1 : 0;
    ((Entity) this).Collidable = false;
    if ((double) dir.X != 0.0)
    {
      float num2 = (double) dir.X <= 0.0 ? ((Entity) this).Left - 1f : ((Entity) this).Right;
      for (int index = 0; (double) index < (double) ((Entity) this).Height; index += 8)
      {
        Vector2 vector2 = new Vector2(num2, ((Entity) this).Top + 4f + (float) index);
        if (((Entity) this).Scene.CollideCheck<Solid>(vector2))
          ((Entity) this).SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
      }
    }
    else
    {
      float num3 = (double) dir.Y <= 0.0 ? ((Entity) this).Top - 1f : ((Entity) this).Bottom;
      for (int index = 0; (double) index < (double) ((Entity) this).Width; index += 8)
      {
        Vector2 vector2 = new Vector2(((Entity) this).Left + 4f + (float) index, num3);
        if (((Entity) this).Scene.CollideCheck<Solid>(vector2))
          ((Entity) this).SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
      }
    }
    ((Entity) this).Collidable = true;
  }

  public enum Directions
  {
    Left,
    Right,
    Up,
    Down,
  }

  private enum MovementState
  {
    Idling,
    Moving,
    Breaking,
  }

  private class Border : Entity
  {
    public NoResetMoveBlock Parent;

    public Border(NoResetMoveBlock parent)
    {
      this.Parent = parent;
      this.Depth = 1;
    }

    public override void Update()
    {
      if (((Entity) this.Parent).Scene != this.Scene)
        this.RemoveSelf();
      base.Update();
    }

    public override void Render()
    {
      Draw.Rect((float) ((double) ((Entity) this.Parent).X + (double) ((Platform) this.Parent).Shake.X - 1.0), (float) ((double) ((Entity) this.Parent).Y + (double) ((Platform) this.Parent).Shake.Y - 1.0), ((Entity) this.Parent).Width + 2f, ((Entity) this.Parent).Height + 2f, Color.Black);
    }
  }

  [Pooled]
  private class Debris : Actor
  {
    private Image sprite;
    private Vector2 home;
    private Vector2 speed;
    private bool shaking;
    private bool returning;
    private float returnEase;
    private float returnDuration;
    private SimpleCurve returnCurve;
    private bool firstHit;
    private float alpha;
    private Collision onCollideH;
    private Collision onCollideV;
    private float spin;

    public Debris()
      : base(Vector2.Zero)
    {
      ((Entity) this).Tag = ((int) (Tags.TransitionUpdate));
      ((Entity) this).Collider = (Collider) new Hitbox(4f, 4f, -2f, -2f);
      ((Entity) this).Add((Component) (this.sprite = new Image(Calc.Choose<MTexture>(Calc.Random, GFX.Game.GetAtlasSubtextures("objects/DZ/DZ/nrMoveblock/debris")))));
      this.sprite.CenterOrigin();
      ((GraphicsComponent) this.sprite).FlipX = Calc.Chance(Calc.Random, 0.5f);
      // ISSUE: method pointer
      this.onCollideH = new Collision(this.OnCollideH);
      // ISSUE: method pointer
      this.onCollideV = new Collision(this.OnCollideV);
    }

    public override void OnSquish(CollisionData data)
    {
    }

    private void OnCollideH(CollisionData data)
    {
      this.firstHit = true;
      this.speed.X *= -0.6f;
      this.spin *= -1f;
    }

    private void OnCollideV(CollisionData data)
    {
      if (!this.firstHit)
      {
        this.firstHit = true;
        this.StartShaking();
      }

      this.speed.Y *= -0.25f;
      this.speed.X *= 0.85f;
      if (Math.Abs(this.speed.Y) < 8f)
        this.speed.Y = 0.0f;
    }

    public NoResetMoveBlock.Debris Init(Vector2 position, Vector2 center, Vector2 returnTo)
    {
      ((Entity) this).Collidable = true;
      ((Entity) this).Position = position;
      this.speed = Calc.SafeNormalize(((position) - (center)), 60f + Calc.NextFloat(Calc.Random, 60f));
      this.home = returnTo;
      ((GraphicsComponent) this.sprite).Position = Vector2.Zero;
      ((GraphicsComponent) this.sprite).Rotation = Calc.NextAngle(Calc.Random);
      this.returning = false;
      this.shaking = false;
      ((GraphicsComponent) this.sprite).Scale.X = 1f;
      ((GraphicsComponent) this.sprite).Scale.Y = 1f;
      ((GraphicsComponent) this.sprite).Color = Color.White;
      this.alpha = 1f;
      this.firstHit = false;
      this.spin = Calc.Range(Calc.Random, 3.49065852f, 10.4719753f) * (float) Calc.Choose<int>(Calc.Random, 1, -1);
      return this;
    }

    public override void Update()
    {
      base.Update();
      if (!this.returning)
      {
        if (((Entity) this).Collidable)
        {
          this.speed.X = Calc.Approach(this.speed.X, 0.0f, Engine.DeltaTime * 100f);
          if (!this.OnGround(1))
            this.speed.Y += 400f * Engine.DeltaTime;
          this.MoveH(this.speed.X * Engine.DeltaTime, this.onCollideH, (Solid) null);
          this.MoveV(this.speed.Y * Engine.DeltaTime, this.onCollideV, (Solid) null);
        }
        if (this.shaking && ((Entity) this).Scene.OnInterval(0.05f))
        {
          ((GraphicsComponent) this.sprite).X = (float) (Calc.Random.Next(3) - 1);
          ((GraphicsComponent) this.sprite).Y = (float) (Calc.Random.Next(3) - 1);
        }
      }
      else
      {
        ((Entity) this).Position = this.returnCurve.GetPoint(Ease.CubeOut.Invoke(this.returnEase));
        this.returnEase = Calc.Approach(this.returnEase, 1f, Engine.DeltaTime / this.returnDuration);
        ((GraphicsComponent) this.sprite).Scale = ((Vector2.One) * ((float) (1.0 + (double) this.returnEase * 0.5)));
      }
      if ((((Entity) this).Scene as Level).Transitioning)
      {
        this.alpha = Calc.Approach(this.alpha, 0.0f, Engine.DeltaTime * 4f);
        ((GraphicsComponent) this.sprite).Color = ((Color.White) * (this.alpha));
      }
      Image sprite = this.sprite;
      ((GraphicsComponent) sprite).Rotation = ((GraphicsComponent) sprite).Rotation + this.spin * Calc.ClampedMap(Math.Abs(this.speed.Y), 50f, 150f, 0.0f, 1f) * Engine.DeltaTime;
    }

    public void StopMoving() => ((Entity) this).Collidable = false;

    public void StartShaking() => this.shaking = true;

    public void ReturnHome(float duration)
    {
      if (((Entity) this).Scene != null)
      {
        Camera camera = (((Entity) this).Scene as Level).Camera;
        if ((double) ((Entity) this).X < (double) camera.X)
          ((Entity) this).X = camera.X - 8f;
        if ((double) ((Entity) this).Y < (double) camera.Y)
          ((Entity) this).Y = camera.Y - 8f;
        if ((double) ((Entity) this).X > (double) camera.X + 320.0)
          ((Entity) this).X = (float) ((double) camera.X + 320.0 + 8.0);
        if ((double) ((Entity) this).Y > (double) camera.Y + 180.0)
          ((Entity) this).Y = (float) ((double) camera.Y + 180.0 + 8.0);
      }
      this.returning = true;
      this.returnEase = 0.0f;
      this.returnDuration = duration;
      Vector2 vector2 = Calc.SafeNormalize(((this.home) - (((Entity) this).Position)));
      this.returnCurve = new SimpleCurve(((Entity) this).Position, this.home, ((((((((Entity) this).Position) + (this.home))) / (2f))) + (((((new Vector2(vector2.Y, 0.0f - vector2.X)) * (Calc.NextFloat(Calc.Random, 16f) + 16f))) * ((float) Calc.Facing(Calc.Random))))));
    }
  }
}

