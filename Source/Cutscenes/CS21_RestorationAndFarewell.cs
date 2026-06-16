using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;


public class CS21_RestorationAndFarewell : CutsceneEntity
{
    private Player player;
    
    // Main characters
    private Npc20_Madeline madeline;
    private NPC badeline;
    private Npc20_Asriel asriel;
    private Npc20_Granny granny;
    private NPC titanKing;
    private NPC kirby;
    
    // Undertale/Deltarune characters
    private NPC toriel;
    private NPC asgore;
    private NPC theo;
    private NPC sans;
    private NPC undyne;
    private NPC papyrus;
    private NPC alphys;
    private NPC suzy;
    private NPC berdly;
    private NPC noelle;
    private NPC chara;
    private NPC flowey;
    
    // Kirby's parents
    private NPC stellar;
    private NPC voidDreamer;
    
    private EventInstance snapshot;
    private float fade;
    private ParticleType soulParticle;
    private BarrierBreakController barrierBreakController;
    private EventInstance entrySfx;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS21_RestorationAndFarewell(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        base.Depth = -1000000;
        
        // Soul particle effect
        soulParticle = new ParticleType
        {
            Color = Color.White,
            Color2 = Color.Gold,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 1f,
            LifeMax = 2f,
            Size = 1f,
            SpeedMin = 20f,
            SpeedMax = 40f,
            Direction = -MathHelper.PiOver2,
            DirectionRange = MathHelper.PiOver4
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Level level = scene as Level;
        level.TimerStopped = true;
        level.TimerHidden = true;
        level.SaveQuitDisabled = true;
        snapshot = Audio.CreateSnapshot("snapshot:/game_10_granny_clouds_dialogue");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        // Setup
        player.StateMachine.State = Player.StDummy; // StDummy
        player.Sprite.Play("idle");
        
        // Set the music for this emotional finale
        Audio.SetMusic("event:/new_content/music/pusheen/lvl21/saved");
        
        // Fade in from white (after defeating Els)
        FadeWipe fadeWipe = new FadeWipe(Level, wipeIn: true);
        fadeWipe.Duration = 2f;
        ScreenWipe.WipeColor = Color.White;
        yield return fadeWipe.Duration;
        yield return 1f;
        
        // Setup all NPCs at starting positions
        yield return SetupNPCs(level);
        
        // Main dialog with all triggers
        yield return Textbox.Say(
            "DZ_CH20_RESTORATION_AND_FAREWELL",
            EveryoneWalksIn,
            GrannyAndTitanKingWalkIn,
            PanToAsgoreAndToriel,
            FamilyHug,
            PanToBadelineAndMadeline,
            MadelineAndBadelineLookLeft,
            PanToKirbyParents,
            KirbyHugsParents,
            KirbyTurnsRight,
            SansWalksIn,
            MadelineTurnsRight,
            PanToKirby,
            AsrielWalksRight,
            ReleaseSouls
        );
        
        // Giygas destruction message
        yield return 0.5f;
        Level.Flash(Color.White, true);
        Level.Shake(2f);
        yield return Textbox.Say("DZ_CH20_GIYGAS_IS_DESTROYED");
        yield return 1f;
        
        // Goodbye sequence
        yield return Textbox.Say(
            "DZ_CH20_GOODBYE",
            AsrielPowerDown,
            AsrielFadesAway,
            CharaWalksIn,
            SpiritsFadeAway,
            FadeToWhite
        );
        
        // Decades later title card
        yield return 2f;
        yield return Textbox.Say("DZ_CH20_DECADES_LATER");
        
        yield return 1f;
        EndCutscene(level);
    }

    private IEnumerator SetupNPCs(Level level)
    {
        Vector2 spawn = level.GetSpawnPoint(player.Position);
            
        // Set up player state for cutscene
        player.StateMachine.State = Player.StDummy; // Dummy state
        player.DummyGravity = false;
            
        // Play entry sound and perform moon landing (from CS20_saved)
        entrySfx = Audio.Play("event:/new_content/char/madeline/screenentry_gran_landing", player.Position);
        yield return player.MoonLanding(spawn);
            
        // Zoom to focus on the scene
        yield return level.ZoomTo(new Vector2(spawn.X - level.Camera.X, 134f), 2f, 0.5f);
        yield return 0.5f;

        // Yield first to ensure we're not in the middle of entity enumeration
        yield return null;
        
        // Create Kirby (player representation in this scene)
        kirby = CreateNPC("kirby", player.Position, "kirby");
        yield return null;
        
        // Create main characters off-screen to the right
        float rightOffset = 200f;
        madeline = new Npc20_Madeline(new EntityData(), player.Position + new Vector2(rightOffset, 0f));
        SetupNpcSprite(madeline, "madeline");
        Scene.Add(madeline);
        yield return null;
        
        badeline = CreateNPC("badeline", player.Position + new Vector2(rightOffset + 20f, 0f), "badeline");
        asriel = new Npc20_Asriel(new EntityData(), player.Position + new Vector2(rightOffset + 40f, 0f));
        SetupNpcSprite(asriel, "asriel");
        Scene.Add(asriel);
        yield return null;
        
        // Create other characters further off-screen
        float farRight = 300f;
        granny = new Npc20_Granny(new EntityData(), player.Position + new Vector2(farRight, 0f));
        SetupNpcSprite(granny, "granny");
        Scene.Add(granny);
        yield return null;
        
        titanKing = CreateNPC("titan_king", player.Position + new Vector2(farRight + 30f, 0f), "titan_king");
        theo = CreateNPC("theo", player.Position + new Vector2(rightOffset + 60f, 0f), "theo");
        yield return null;
        
        toriel = CreateNPC("toriel", player.Position + new Vector2(rightOffset + 80f, 0f), "toriel");
        asgore = CreateNPC("asgore", player.Position + new Vector2(rightOffset + 100f, 0f), "asgore");
        yield return null;
        
        sans = CreateNPC("sans", player.Position + new Vector2(rightOffset + 120f, 0f), "sans");
        undyne = CreateNPC("undyne", player.Position + new Vector2(rightOffset + 140f, 0f), "undyne");
        yield return null;
        
        papyrus = CreateNPC("papyrus", player.Position + new Vector2(rightOffset + 160f, 0f), "papyrus");
        alphys = CreateNPC("alphys", player.Position + new Vector2(rightOffset + 180f, 0f), "alphys");
        yield return null;
        
        suzy = CreateNPC("suzy", player.Position + new Vector2(rightOffset + 200f, 0f), "suzy");
        berdly = CreateNPC("berdly", player.Position + new Vector2(rightOffset + 220f, 0f), "berdly");
        yield return null;
        
        noelle = CreateNPC("noelle", player.Position + new Vector2(rightOffset + 240f, 0f), "noelle");
        chara = CreateNPC("chara", player.Position + new Vector2(farRight + 60f, 0f), "maggy_chara");
        yield return null;
        
        flowey = CreateNPC("flowey", player.Position + new Vector2(farRight + 90f, 0f), "maggy_flowey");
        yield return null;
        
        // Kirby's parents (spirits)
        stellar = CreateNPC("stellar", player.Position + new Vector2(-200f, 0f), "stellar");
        voidDreamer = CreateNPC("void_dreamer", player.Position + new Vector2(-230f, 0f), "void_dreamer");
        yield return null;
        
        // Make spirit characters semi-transparent
        if (stellar?.Sprite != null) stellar.Sprite.Color = Color.White * 0.8f;
        if (voidDreamer?.Sprite != null) voidDreamer.Sprite.Color = Color.White * 0.8f;
        if (granny?.Sprite != null) granny.Sprite.Color = Color.White * 0.8f;
        if (titanKing?.Sprite != null) titanKing.Sprite.Color = Color.White * 0.8f;
        
        yield return null;
    }

    private NPC CreateNPC(string name, Vector2 position, string spriteBank)
    {
        var npc = new NPC(position);
        npc.Add(npc.Sprite = GFX.SpriteBank.Create(spriteBank));
        npc.Sprite.Play("idle");
        Scene.Add(npc);
        return npc;
    }

    private void SetupNpcSprite(NPC npc, string spriteBank)
    {
        npc.Add(npc.Sprite = GFX.SpriteBank.Create(spriteBank));
        npc.Sprite.Play("idle");
        npc.IdleAnim = "idle";
        npc.MoveAnim = "walk";
        npc.Maxspeed = 40f;
    }

    private void SetupNpcSprite(Npc20_Madeline npc, string spriteBank)
    {
        npc.Add(npc.Sprite = GFX.SpriteBank.Create(spriteBank));
        npc.Sprite.Play("idle");
    }

    private void SetupNpcSprite(Npc20_Asriel npc, string spriteBank)
    {
        npc.Add(npc.Sprite = GFX.SpriteBank.Create(spriteBank));
        npc.Sprite.Play("idle");
    }

    private void SetupNpcSprite(Npc20_Granny npc, string spriteBank)
    {
        npc.Add(npc.Sprite = GFX.SpriteBank.Create(spriteBank));
        npc.Sprite.Play("idle");
    }

    #region Trigger 0-13: CH20_RESTORATION_AND_FAREWELL

    // {trigger 0 maddy baddy and everyone else walk in left}
    private IEnumerator EveryoneWalksIn()
    {
        // Everyone walks in from the right
        float targetX = player.X + 50f;
        
        Add(new Coroutine(WalkNpcTo(madeline, targetX)));
        Add(new Coroutine(WalkNpcTo(badeline, targetX + 20f)));
        Add(new Coroutine(WalkNpcTo(theo, targetX + 40f)));
        Add(new Coroutine(WalkNpcTo(toriel, targetX + 60f)));
        Add(new Coroutine(WalkNpcTo(asgore, targetX + 80f)));
        Add(new Coroutine(WalkNpcTo(asriel, targetX + 100f)));
        
        yield return 2f;
    }

    // {trigger 1 granny and titan king walk in right}
    private IEnumerator GrannyAndTitanKingWalkIn()
    {
        float targetX = player.X + 120f;
        
        Add(new Coroutine(WalkNpcTo(granny, targetX)));
        Add(new Coroutine(WalkNpcTo(titanKing, targetX + 30f)));
        
        yield return 1.5f;
    }

    // {trigger 2 pan to the right to see asgore and toriel}
    private IEnumerator PanToAsgoreAndToriel()
    {
        Vector2 targetCamera = new Vector2(asgore.X - 160f, Level.Camera.Y);
        yield return CameraTo(targetCamera, 1f, Ease.SineInOut);
    }

    // {trigger 3 tori and asgore run and hug asriel}
    private IEnumerator FamilyHug()
    {
        // Toriel and Asgore run to Asriel
        float asrielX = asriel.X;
        
        Add(new Coroutine(RunNpcTo(toriel, asrielX - 20f)));
        Add(new Coroutine(RunNpcTo(asgore, asrielX + 20f)));
        
        yield return 1f;
        
        // Emotional moment - screen shake
        Level.Shake(0.2f);
        Audio.Play("event:/new_content/char/pusheen/asriel/emotional_reunion");
        
        yield return 0.5f;
    }

    // {trigger 4 pan to badeline and madeline look at right}
    private IEnumerator PanToBadelineAndMadeline()
    {
        Vector2 targetCamera = new Vector2(madeline.X - 80f, Level.Camera.Y);
        yield return CameraTo(targetCamera, 0.8f, Ease.SineInOut);
        
        // Make them face right
        if (madeline?.Sprite != null) madeline.Sprite.Scale.X = 1f;
        if (badeline?.Sprite != null) badeline.Sprite.Scale.X = 1f;
        
        yield return null;
    }

    // {trigger 5 madeline and badeline look at left}
    private IEnumerator MadelineAndBadelineLookLeft()
    {
        if (madeline?.Sprite != null) madeline.Sprite.Scale.X = -1f;
        if (badeline?.Sprite != null) badeline.Sprite.Scale.X = -1f;
        yield return 0.3f;
    }

    // {trigger 6 pan to the kirby parents to the left and kirby turn left}
    private IEnumerator PanToKirbyParents()
    {
        // Move Kirby's parents closer
        Add(new Coroutine(WalkNpcTo(stellar, player.X - 60f)));
        Add(new Coroutine(WalkNpcTo(voidDreamer, player.X - 90f)));
        
        Vector2 targetCamera = new Vector2(stellar.X - 80f, Level.Camera.Y);
        yield return CameraTo(targetCamera, 1f, Ease.SineInOut);
        
        // Kirby turns left
        player.Facing = Facings.Left;
        yield return null;
    }

    // {trigger 7 kirby walk to the parents and hug them}
    private IEnumerator KirbyHugsParents()
    {
        yield return player.DummyWalkToExact((int)(stellar.X + 20f), walkBackwards: false, 1f);
        
        // Emotional hug moment
        Level.Shake(0.1f);
        Audio.Play("event:/new_content/char/pusheen/kirby/emotional_reunion");
        
        // Particle effects for the reunion
        for (int i = 0; i < 20; i++)
        {
            Level.ParticlesFG.Emit(soulParticle, player.Center, Color.Pink);
        }
        
        yield return 1f;
    }

    // {trigger 8 kirby turn right and look at madeline and badeline}
    private IEnumerator KirbyTurnsRight()
    {
        player.Facing = Facings.Right;
        
        Vector2 targetCamera = new Vector2(madeline.X - 100f, Level.Camera.Y);
        yield return CameraTo(targetCamera, 0.6f, Ease.SineInOut);
    }

    // {trigger 9 sans walk left and look at madeline and badeline}
    private IEnumerator SansWalksIn()
    {
        Add(new Coroutine(WalkNpcTo(sans, madeline.X + 40f)));
        yield return 1f;
        
        // Sans faces left toward Madeline
        if (sans?.Sprite != null) sans.Sprite.Scale.X = -1f;
        yield return null;
    }

    // {trigger 10 madeline turn right}
    private IEnumerator MadelineTurnsRight()
    {
        if (madeline?.Sprite != null) madeline.Sprite.Scale.X = 1f;
        yield return 0.2f;
    }

    // {trigger 11 madeline turn left and pan ominously to kirby}
    private IEnumerator PanToKirby()
    {
        if (madeline?.Sprite != null) madeline.Sprite.Scale.X = -1f;
        
        // Ominous pan to Kirby
        Vector2 targetCamera = new Vector2(player.X - 80f, Level.Camera.Y);
        yield return CameraTo(targetCamera, 1.2f, Ease.SineIn);
        
        // Slight screen effect for emotional weight
        Level.ZoomSnap(new Vector2(160f, 90f), 1.1f);
        yield return 0.3f;
    }

    // {trigger 12 move offset to madeline and badeline and asriel walk right}
    private IEnumerator AsrielWalksRight()
    {
        Vector2 targetCamera = new Vector2(madeline.X - 60f, Level.Camera.Y);
        Add(new Coroutine(CameraTo(targetCamera, 0.8f, Ease.SineInOut)));
        
        // Asriel walks to the right toward Madeline
        Add(new Coroutine(WalkNpcTo(asriel, madeline.X + 50f)));
        
        yield return 1.5f;
    }

    // {trigger 13 asriel released the souls and the 4th wall opens up destroying giygas and the barrier of the void}
    private IEnumerator ReleaseSouls()
    {
        // Dramatic buildup
        Audio.SetMusic("event:/new_content/music/pusheen/lvl20/back");

        yield return 0.5f;

        // Asriel raises his hands (play animation if available)
        if (asriel?.Sprite != null)
        {
            asriel.Sprite.Play("release_souls");
        }

        // Screen effects for soul release
        for (int i = 0; i < 5; i++)
        {
            Level.Shake(0.5f + i * 0.2f);
            Level.Flash(Color.Gold * (0.2f + i * 0.15f));

            // Soul particles emanating from Asriel
            for (int j = 0; j < 30; j++)
            {
                Level.ParticlesFG.Emit(soulParticle, asriel.Center + new Vector2(Calc.Random.Range(-20f, 20f), Calc.Random.Range(-30f, 10f)), Color.Lerp(Color.Gold, Color.White, Calc.Random.NextFloat()));
            }

            yield return 0.4f;
        }

        // Create and position the barrier break effect
        barrierBreakController = new BarrierBreakController(asriel.Center + new Vector2(0f, -50f));
        Scene.Add(barrierBreakController);
        
        yield return 0.2f;

        // Execute the full barrier break sequence (Part 1, 2, and 3)
        yield return barrierBreakController.ExecuteBarrierBreakSequence();

        yield return 0.5f;

        // Clean up
        barrierBreakController?.RemoveSelf();
        barrierBreakController = null;

        yield return 1f;
    }

    #endregion

    #region Triggers for CH20_GOODBYE

    // {trigger 0 asriel powerdown and say goodbye to everyone}
    private IEnumerator AsrielPowerDown()
    {
        // Asriel powers down
        if (asriel?.Sprite != null)
        {
            asriel.Sprite.Play("idle");
        }
        
        Level.ZoomSnap(new Vector2(160f, 90f), 1.5f);
        yield return Level.ZoomTo(new Vector2(160f, 90f), 1.2f, 2f);
    }

    // {trigger 1 asriel slowly fades away into the flower form}
    private IEnumerator AsrielFadesAway()
    {
        Audio.Play("event:/new_content/char/pusheen/asriel/fade_to_flower");
        
        // Slowly fade Asriel
        for (float p = 1f; p > 0f; p -= Engine.DeltaTime / 3f)
        {
            if (asriel?.Sprite != null)
            {
                asriel.Sprite.Color = Color.White * p;
            }
            
            // Soul particles as he fades
            if (Calc.Random.Chance(0.3f))
            {
                Level.ParticlesFG.Emit(soulParticle, asriel.Center, Color.Gold * p);
            }
            
            yield return null;
        }
        
        // Hide Asriel and show Flowey
        asriel.Visible = false;
        flowey.Position = asriel.Position;
        flowey.Visible = true;
        
        yield return 0.5f;
    }

    // {trigger 3 chara walk in left and look at asriel}
    private IEnumerator CharaWalksIn()
    {
        Add(new Coroutine(WalkNpcTo(chara, asriel.X - 30f)));
        yield return 1.5f;
        
        // Chara faces right toward where Asriel was
        if (chara?.Sprite != null) chara.Sprite.Scale.X = 1f;
        yield return null;
    }

    // {trigger 4 asriel granny titan king and kirby parent begin to fade away}
    private IEnumerator SpiritsFadeAway()
    {
        Audio.Play("event:/new_content/char/pusheen/spirits/fade_away");
        
        // Fade out all spirit characters
        for (float p = 1f; p > 0f; p -= Engine.DeltaTime / 4f)
        {
            Color fadeColor = Color.White * p;
            
            if (granny?.Sprite != null) granny.Sprite.Color = fadeColor;
            if (titanKing?.Sprite != null) titanKing.Sprite.Color = fadeColor;
            if (stellar?.Sprite != null) stellar.Sprite.Color = fadeColor;
            if (voidDreamer?.Sprite != null) voidDreamer.Sprite.Color = fadeColor;
            
            // Particles as they fade
            if (Calc.Random.Chance(0.2f))
            {
                if (granny != null) Level.ParticlesFG.Emit(soulParticle, granny.Center, Color.White * p);
                if (stellar != null) Level.ParticlesFG.Emit(soulParticle, stellar.Center, Color.Pink * p);
            }
            
            yield return null;
        }
        
        // Hide the spirit NPCs
        if (granny != null) granny.Visible = false;
        if (titanKing != null) titanKing.Visible = false;
        if (stellar != null) stellar.Visible = false;
        if (voidDreamer != null) voidDreamer.Visible = false;
        
        yield return 0.5f;
    }

    // {trigger 5 faded back to black}
    private IEnumerator FadeToWhite()
    {
        Add(new Coroutine(DoFadeToWhite()));
        yield break;
    }

    private IEnumerator DoFadeToWhite()
    {
        Add(new Coroutine(Level.ZoomBack(8f)));
        while (fade < 1f)
        {
            fade = Calc.Approach(fade, 1f, Engine.DeltaTime / 8f);
            yield return null;
        }
    }

    #endregion

    #region Helper Methods

    private IEnumerator WalkNpcTo(NPC npc, float targetX)
    {
        if (npc == null) yield break;
        
        float direction = Math.Sign(targetX - npc.X);
        if (npc.Sprite != null)
        {
            npc.Sprite.Scale.X = direction;
            npc.Sprite.Play("walk");
        }
        
        float speed = 40f;
        while (Math.Abs(npc.X - targetX) > 2f)
        {
            npc.X += direction * speed * Engine.DeltaTime;
            yield return null;
        }
        
        npc.X = targetX;
        if (npc.Sprite != null)
        {
            npc.Sprite.Play("idle");
        }
    }

    private IEnumerator WalkNpcTo(Npc20_Madeline npc, float targetX)
    {
        if (npc == null) yield break;
        yield return npc.MoveTo(targetX);
    }

    private IEnumerator WalkNpcTo(Npc20_Asriel npc, float targetX)
    {
        if (npc == null) yield break;
        yield return npc.MoveTo(targetX, 0f, true);
    }

    private IEnumerator WalkNpcTo(Npc20_Granny npc, float targetX)
    {
        if (npc == null) yield break;
        yield return npc.MoveTo(targetX);
    }

    private IEnumerator RunNpcTo(NPC npc, float targetX)
    {
        if (npc == null) yield break;
        
        float direction = Math.Sign(targetX - npc.X);
        if (npc.Sprite != null)
        {
            npc.Sprite.Scale.X = direction;
            npc.Sprite.Play("run");
        }
        
        float speed = 80f; // Faster for running
        while (Math.Abs(npc.X - targetX) > 2f)
        {
            npc.X += direction * speed * Engine.DeltaTime;
            yield return null;
        }
        
        npc.X = targetX;
        if (npc.Sprite != null)
        {
            npc.Sprite.Play("idle");
        }
    }

    private IEnumerator CameraTo(Vector2 target, float duration, Ease.Easer ease = null)
    {
        ease ??= Ease.Linear;
        Vector2 from = Level.Camera.Position;
        
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
        {
            Level.Camera.Position = Vector2.Lerp(from, target, ease(p));
            yield return null;
        }
        
        Level.Camera.Position = target;
    }

    #endregion

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        Dispose();
        
        if (WasSkipped)
        {
            // Clean up if skipped
            CleanupNPCs();
        }
        
        // Transition to the end cinematic
        level.OnEndOfFrame += () =>
        {
            level.TeleportTo(player, "end-later", Player.IntroTypes.Transition);
        };
    }

    private void CleanupNPCs()
    {
        madeline?.RemoveSelf();
        badeline?.RemoveSelf();
        asriel?.RemoveSelf();
        granny?.RemoveSelf();
        titanKing?.RemoveSelf();
        kirby?.RemoveSelf();
        toriel?.RemoveSelf();
        asgore?.RemoveSelf();
        theo?.RemoveSelf();
        sans?.RemoveSelf();
        undyne?.RemoveSelf();
        papyrus?.RemoveSelf();
        alphys?.RemoveSelf();
        suzy?.RemoveSelf();
        berdly?.RemoveSelf();
        noelle?.RemoveSelf();
        chara?.RemoveSelf();
        flowey?.RemoveSelf();
        stellar?.RemoveSelf();
        voidDreamer?.RemoveSelf();
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Dispose();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Dispose();
    }

    private void Dispose()
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        base.Render();
        
        // Render fade to black overlay
        if (fade > 0f)
        {
            Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.Black * fade);
        }
    }
}
