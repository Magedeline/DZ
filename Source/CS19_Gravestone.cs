// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_Gravestone
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
using MaggyHelper.Entities;
using BirdNPC = MaggyHelper.Entities.BirdNPC;
using NPC19_Gravestone = MaggyHelper.NPCs.NPC19_Gravestone;

namespace MaggyHelper.Cutscenes
{
  public class CS19_Gravestone : CutsceneEntity
  {
    private const string Flag = "maddy_gravestone";
    
    // Bird goner sprite paths
    private const string BirdGonerPath = "characters/birdgoner/";
    private const string BirdGonerClover = BirdGonerPath + "cloverflyup";
    private const string BirdGonerCody = BirdGonerPath + "codyflyup";
    private const string BirdGonerEmily = BirdGonerPath + "emilyflyup";
    private const string BirdGonerOdin = BirdGonerPath + "odinflyup";
    private const string BirdGonerRobin = BirdGonerPath + "robinflyup";
    private const string BirdGonerSabel = BirdGonerPath + "sabelflyup";

    private global::Celeste.Player player;
    private NPC19_Gravestone gravestone;
    private CharaDummy chara;
    private BirdNPC bird;
    private BirdNPC birdClover;
    private BirdNPC birdCody;
    private BirdNPC birdEmily;
    private BirdNPC birdOdin;
    private BirdNPC birdRobin;
    private BirdNPC birdSabel;
    private Vector2 boostTarget;
    private bool addedBooster;
    private CharaDummy undyne;
    private CharaDummy toriel;
    private CharaDummy theo;
    private CharaDummy asgore;
    private CharaDummy starsi;
    private CharaDummy ralsei;
    private CharaDummy sans;
    private CharaDummy papyrus;
    private CharaDummy magolor;
    private CharaDummy alphy;

    public CS19_Gravestone(global::Celeste.Player player, NPC19_Gravestone gravestone, Vector2 boostTarget)
        {
      this.player = player;
      this.gravestone = gravestone;
      this.boostTarget = boostTarget;
        }

        public override void OnBegin(Level level)
    {
      this.Add((Component)new Coroutine(this.cutscene()));
    }

    private IEnumerator cutscene()
    {
      CS19_Gravestone cs10Gravestone = this;
      cs10Gravestone.player.StateMachine.State = 11;
      cs10Gravestone.player.ForceCameraUpdate = true;
      cs10Gravestone.player.DummyGravity = false;
      cs10Gravestone.player.Speed.Y = 0.0f;
      yield return (object)0.1f;
      yield return (object)cs10Gravestone.player.DummyWalkToExact((int)cs10Gravestone.gravestone.X - 30);
      yield return (object)0.1f;
      cs10Gravestone.player.Facing = Facings.Right;
      yield return (object)0.2f;
      yield return (object)cs10Gravestone.Level.ZoomTo(new Vector2(160f, 90f), 2f, 3f);
      cs10Gravestone.player.ForceCameraUpdate = false;
      
      // Initialize the dummy characters
      cs10Gravestone.undyne = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-50f, 0f));
      cs10Gravestone.toriel = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-45f, 0f));
      cs10Gravestone.theo = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-40f, 0f));
      cs10Gravestone.asgore = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-35f, 0f));
      cs10Gravestone.starsi = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-30f, 0f));
      cs10Gravestone.ralsei = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-25f, 0f));
      cs10Gravestone.sans = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-20f, 0f));
      cs10Gravestone.papyrus = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-15f, 0f));
      cs10Gravestone.magolor = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-10f, 0f));
      cs10Gravestone.alphy = new CharaDummy(cs10Gravestone.player.Position + new Vector2(-5f, 0f));

      // Add them to the level
      cs10Gravestone.Level.Add(cs10Gravestone.undyne);
      cs10Gravestone.Level.Add(cs10Gravestone.toriel);
      cs10Gravestone.Level.Add(cs10Gravestone.theo);
      cs10Gravestone.Level.Add(cs10Gravestone.asgore);
      cs10Gravestone.Level.Add(cs10Gravestone.starsi);
      cs10Gravestone.Level.Add(cs10Gravestone.ralsei);
      cs10Gravestone.Level.Add(cs10Gravestone.sans);
      cs10Gravestone.Level.Add(cs10Gravestone.papyrus);
      cs10Gravestone.Level.Add(cs10Gravestone.magolor);
      cs10Gravestone.Level.Add(cs10Gravestone.alphy);

      yield return (object)0.5f;
      yield return (object)Textbox.Say("CH19_GRAVESTONE", new Func<IEnumerator>(cs10Gravestone.stepForward), new Func<IEnumerator>(cs10Gravestone.charaAppears), new Func<IEnumerator>(cs10Gravestone.turnleft), new Func<IEnumerator>(cs10Gravestone.EveryoneWalkin));
      yield return (object)1f;
      yield return (object)cs10Gravestone.birdStuff();
      yield return (object)cs10Gravestone.charaRejoin();
      yield return (object)0.1f;
      yield return (object)cs10Gravestone.Level.ZoomBack(0.5f);
      yield return (object)0.3f;
      cs10Gravestone.addedBooster = true;
      cs10Gravestone.Level.Displacement.AddBurst(cs10Gravestone.boostTarget, 0.5f, 8f, 32f, 0.5f);
      Audio.Play("event:/new_content/char/badeline/booster_first_appear", cs10Gravestone.boostTarget);
      cs10Gravestone.Level.Add((Entity)new CustomCharaBoost(new Vector2[1]
      {
        cs10Gravestone.boostTarget
      }, false));
      yield return (object)0.2f;
      cs10Gravestone.EndCutscene(cs10Gravestone.Level);
    }

    private IEnumerator stepForward()
    {
      yield return (object)this.player.DummyWalkTo(this.player.X + 8f);
    }

    private IEnumerator charaAppears()
    {
      CS19_Gravestone cs10Gravestone = this;
      cs10Gravestone.Level.Session.Inventory.Dashes = 1;
      cs10Gravestone.player.Dashes = 1;
      Vector2 position = cs10Gravestone.player.Position + new Vector2(-12f, -10f);
      cs10Gravestone.Level.Displacement.AddBurst(position, 0.5f, 8f, 32f, 0.5f);
      cs10Gravestone.Level.Add((Entity)(cs10Gravestone.chara = new CharaDummy(position)));
      Audio.Play("event:/char/badeline/maddy_split", position);
      cs10Gravestone.chara.Sprite.Scale.X = 1f;
      yield return null;
    }

    private IEnumerator turnleft()
    {
      yield return (object)0.2f;
      this.player.DummyAutoAnimate = false;
      this.player.Facing = Facings.Left;
      this.player.Sprite.Play("idle");
      yield return (object)0.3f;
    }

    private IEnumerator EveryoneWalkin()
    {
      CS19_Gravestone cs10Gravestone = this;
      yield return cs10Gravestone.undyne.WalkTo(cs10Gravestone.player.X - 10f);
      yield return cs10Gravestone.toriel.WalkTo(cs10Gravestone.player.X + 16f);
      yield return cs10Gravestone.theo.WalkTo(cs10Gravestone.player.X + 18f);
      yield return cs10Gravestone.asgore.WalkTo(cs10Gravestone.player.X);
      yield return cs10Gravestone.starsi.WalkTo(cs10Gravestone.player.X - 8f);
      yield return cs10Gravestone.ralsei.WalkTo(cs10Gravestone.player.X - 8f);
      yield return cs10Gravestone.sans.WalkTo(cs10Gravestone.player.X - 8f);
      yield return cs10Gravestone.papyrus.WalkTo(cs10Gravestone.player.X - 8f);
      yield return cs10Gravestone.magolor.WalkTo(cs10Gravestone.player.X - 8f);
      yield return cs10Gravestone.alphy.WalkTo(cs10Gravestone.player.X - 8f);
    }

    private IEnumerator birdStuff()
    {
      CS19_Gravestone cs10Gravestone = this;
      
      // Main bird
      cs10Gravestone.bird = new BirdNPC(cs10Gravestone.player.Position + new Vector2(88f, -200f), BirdNPC.Modes.None);
      cs10Gravestone.bird.DisableFlapSfx = true;
      cs10Gravestone.bird.Facing = Facings.Left;
      cs10Gravestone.bird.Sprite.Play("fall");
      
      // Colored bird goners - Clover (green)
      cs10Gravestone.birdClover = new BirdNPC(cs10Gravestone.player.Position + new Vector2(108f, -220f), BirdNPC.Modes.None);
      cs10Gravestone.birdClover.DisableFlapSfx = true;
      cs10Gravestone.birdClover.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdClover);
      
      // Colored bird goners - Cody (blue)
      cs10Gravestone.birdCody = new BirdNPC(cs10Gravestone.player.Position + new Vector2(68f, -210f), BirdNPC.Modes.None);
      cs10Gravestone.birdCody.DisableFlapSfx = true;
      cs10Gravestone.birdCody.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdCody);
      
      // Colored bird goners - Emily (pink)
      cs10Gravestone.birdEmily = new BirdNPC(cs10Gravestone.player.Position + new Vector2(128f, -190f), BirdNPC.Modes.None);
      cs10Gravestone.birdEmily.DisableFlapSfx = true;
      cs10Gravestone.birdEmily.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdEmily);
      
      // Colored bird goners - Odin (purple)
      cs10Gravestone.birdOdin = new BirdNPC(cs10Gravestone.player.Position + new Vector2(48f, -230f), BirdNPC.Modes.None);
      cs10Gravestone.birdOdin.DisableFlapSfx = true;
      cs10Gravestone.birdOdin.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdOdin);
      
      // Colored bird goners - Robin (orange)
      cs10Gravestone.birdRobin = new BirdNPC(cs10Gravestone.player.Position + new Vector2(148f, -215f), BirdNPC.Modes.None);
      cs10Gravestone.birdRobin.DisableFlapSfx = true;
      cs10Gravestone.birdRobin.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdRobin);
      
      // Colored bird goners - Sabel (yellow)
      cs10Gravestone.birdSabel = new BirdNPC(cs10Gravestone.player.Position + new Vector2(28f, -205f), BirdNPC.Modes.None);
      cs10Gravestone.birdSabel.DisableFlapSfx = true;
      cs10Gravestone.birdSabel.Facing = Facings.Left;
      cs10Gravestone.Level.Add((Entity)cs10Gravestone.birdSabel);
      
      Vector2 from = cs10Gravestone.bird.Position;
      Vector2 to = cs10Gravestone.gravestone.Position + new Vector2(1f, -16f);
      float percent = 0.0f;
      while ((double)percent < 1.0)
      {
        cs10Gravestone.bird.Position = from + (to - from) * Ease.QuadOut(percent);
        if ((double)percent > 0.5)
          cs10Gravestone.bird.Sprite.Play("fly");
        percent += Engine.DeltaTime * 0.5f;
        yield return (object)null;
      }
      cs10Gravestone.bird.Position = to;
      cs10Gravestone.bird.Sprite.Play("idle");
      yield return (object)0.5f;
      cs10Gravestone.bird.Sprite.Play("croak");
      yield return (object)0.6f;
      Audio.Play("event:/game/general/bird_squawk", cs10Gravestone.bird.Position);
      yield return (object)0.9f;
      Audio.Play("event:/char/madeline/stand", cs10Gravestone.player.Position);
      cs10Gravestone.player.Sprite.Play("idle");
      yield return (object)1f;
      
      // All birds fly away together
      yield return (object)cs10Gravestone.bird.StartleAndFlyAway();
      if (cs10Gravestone.birdClover != null) cs10Gravestone.birdClover.StartleAndFlyAway();
      if (cs10Gravestone.birdCody != null) cs10Gravestone.birdCody.StartleAndFlyAway();
      if (cs10Gravestone.birdEmily != null) cs10Gravestone.birdEmily.StartleAndFlyAway();
      if (cs10Gravestone.birdOdin != null) cs10Gravestone.birdOdin.StartleAndFlyAway();
      if (cs10Gravestone.birdRobin != null) cs10Gravestone.birdRobin.StartleAndFlyAway();
      if (cs10Gravestone.birdSabel != null) cs10Gravestone.birdSabel.StartleAndFlyAway();
    }

    private IEnumerator charaRejoin()
    {
      CS19_Gravestone cs10Gravestone = this;
      Audio.Play("event:/new_content/char/badeline/maddy_join_quick", cs10Gravestone.chara.Position);
      Vector2 from = cs10Gravestone.chara.Position;
      for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.25f)
      {
        cs10Gravestone.chara.Position = Vector2.Lerp(from, cs10Gravestone.player.Position, Ease.CubeIn(p));
        yield return (object)null;
      }
      cs10Gravestone.Level.Displacement.AddBurst(cs10Gravestone.player.Center, 0.5f, 8f, 32f, 0.5f);
      cs10Gravestone.Level.Session.Inventory.Dashes = 2;
      cs10Gravestone.player.Dashes = 2;
      cs10Gravestone.chara.RemoveSelf();
    }

    public override void OnEnd(Level level)
    {
      this.player.Facing = Facings.Right;
      this.player.DummyAutoAnimate = true;
      this.player.DummyGravity = true;
      this.player.StateMachine.State = 0;
      this.Level.Session.Inventory.Dashes = 5;
      this.player.Dashes = 5;
      if (this.chara != null)
        this.chara.RemoveSelf();
      if (this.bird != null)
        this.bird.RemoveSelf();
      if (this.birdClover != null)
        this.birdClover.RemoveSelf();
      if (this.birdCody != null)
        this.birdCody.RemoveSelf();
      if (this.birdEmily != null)
        this.birdEmily.RemoveSelf();
      if (this.birdOdin != null)
        this.birdOdin.RemoveSelf();
      if (this.birdRobin != null)
        this.birdRobin.RemoveSelf();
      if (this.birdSabel != null)
        this.birdSabel.RemoveSelf();
      if (this.undyne != null)
        this.undyne.RemoveSelf();
      if (this.toriel != null)
        this.toriel.RemoveSelf();
      if (this.theo != null)
        this.theo.RemoveSelf();
      if (this.asgore != null)
        this.asgore.RemoveSelf();
      if (this.starsi != null)
        this.starsi.RemoveSelf();
      if (this.ralsei != null)
        this.ralsei.RemoveSelf();
      if (this.sans != null)
        this.sans.RemoveSelf();
      if (this.papyrus != null)
        this.papyrus.RemoveSelf();
      if (this.magolor != null)
        this.magolor.RemoveSelf();
      if (this.alphy != null)
        this.alphy.RemoveSelf();

      if (!this.addedBooster)
        level.Add((Entity)new CustomCharaBoost
        (new Vector2[1]
        {
          this.boostTarget
        }, false));
      level.ResetZoom();
    }
  }
}



