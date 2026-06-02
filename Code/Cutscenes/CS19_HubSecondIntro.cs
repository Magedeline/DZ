using System.Runtime.CompilerServices;
using FMOD.Studio;

namespace Celeste.Cutscenes;

public class CS19_HubSecondIntro : CutsceneEntity
{
    private class Bird : Entity
    {
        private Sprite sprite;

        public EventInstance sfx;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Bird(Vector2 position)
        {
            Position = position;
            base.Depth = -8500;
            Add(sprite = GFX.SpriteBank.Create("bird"));
            sprite.Play("hover");
            sprite.OnFrameChange = delegate
            {
                BirdNPC.FlapSfxCheck(sprite);
            };
        }

        public IEnumerator IdleRoutine()
        {
            yield return 0.5f;
        }

        public IEnumerator FlyAwayRoutine()
        {
            Level level = Scene as Level;
            sfx = Audio.Play("event:/new_content/game/10_farewell/bird_fly_uptonext", Position);
            sprite.Play("flyup");
            float spd = -32f;
            while (Y > (float)(level.Bounds.Top - 32))
            {
                spd -= 400f * Engine.DeltaTime;
                Y += spd * Engine.DeltaTime;
                yield return null;
            }
        }
    }

    public const string Flag = "hub_introsecondtime";

    public const float BirdOffset = 190f;

    private global::Celeste.Player player;

    private List<LockBlock> locks;

    private Booster booster;

    private Bird bird;

    private Vector2 spawn;

    private List<EventInstance> sfxs = new List<EventInstance>();

    public CS19_HubSecondIntro(global::Celeste.Player player)
    {
        this.player = player;
    }
    
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        spawn = (scene as Level).GetSpawnPoint(player.Position);
        locks = scene.Entities.FindAll<LockBlock>();
        locks.Sort((LockBlock a, LockBlock b) => (int)(a.Y - b.Y));
        foreach (LockBlock @lock in locks)
        {
            @lock.Visible = false;
        }
        booster = scene.Entities.FindFirst<Booster>();
        if (booster != null)
        {
            booster.Visible = false;
        }
    }
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }
    private IEnumerator Cutscene(Level level)
    {
        if (player.Holding != null)
        {
            player.Throw();
        }
        player.StateMachine.State = Player.StDummy;
        player.ForceCameraUpdate = true;
        while (!player.OnGround())
        {
            yield return null;
        }
        player.ForceCameraUpdate = false;
        yield return 0.1f;
        player.DummyAutoAnimate = false;
        player.Sprite.Play("lookUp");
        yield return 0.25f;
        level.Add(bird = new Bird(new Vector2(spawn.X, (float)level.Bounds.Top + 190f)));
        Audio.Play("event:/new_content/game/10_farewell/bird_camera_pan_up");
        yield return CutsceneEntity.CameraTo(new Vector2(spawn.X - 160f, (float)level.Bounds.Top + 190f - 90f), 2f, Ease.CubeInOut);
        yield return bird.IdleRoutine();
        Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X, level.Bounds.Top), 0.8f, Ease.CubeInOut, 0.1f)));
        Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
        yield return bird.FlyAwayRoutine();
        bird.RemoveSelf();
        bird = null;
        yield return 0.5f;

        // Kirby observes the second hub and realizes about the keys
        yield return Textbox.Say("CS19_HUB_SECOND_INTRO");

        yield return 0.3f;
        float duration = 6f;
        string sfx = "event:/new_content/game/10_farewell/locked_door_appear_1".Substring(0, "event:/new_content/game/10_farewell/locked_door_appear_1".Length - 1);
        int doorIndex = 1;
        Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X, level.Bounds.Bottom - 180), duration, Ease.SineInOut)));
        Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 90f), 1.5f, duration)));
        for (float t = 0f; t < duration; t += Engine.DeltaTime)
        {
            foreach (LockBlock @lock in locks)
            {
                if (!@lock.Visible && level.Camera.Y + 90f > @lock.Y - 20f)
                {
                    sfxs.Add(Audio.Play(sfx + doorIndex, @lock.Center));
                    @lock.Appear();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    doorIndex++;
                }
            }
            yield return null;
        }
        yield return 0.5f;
        if (booster != null)
        {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            booster.Appear();
        }
        yield return 0.3f;
        yield return Level.ZoomBack(0.3f);
        EndCutscene(level);
    }
    

    public override void OnEnd(Level level)
    {
        if (WasSkipped)
        {
            foreach (EventInstance sfx in sfxs)
            {
                Audio.Stop(sfx);
            }
            if (bird != null)
            {
                Audio.Stop(bird.sfx);
            }
        }
        foreach (LockBlock @lock in locks)
        {
            @lock.Visible = true;
        }
        if (booster != null)
        {
            booster.Visible = true;
        }
        if (bird != null)
        {
            bird.RemoveSelf();
        }
        if (WasSkipped)
        {
            player.Position = spawn;
        }
        player.Speed = Vector2.Zero;
        player.DummyAutoAnimate = true;
        player.ForceCameraUpdate = false;
        player.StateMachine.State = Player.StNormal;
        level.Camera.Y = level.Bounds.Bottom - 180;
        level.Session.SetFlag("hub_introsecondtime");
        level.ResetZoom();
    }
}




