using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Celeste.Entities;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Credits cutscene for Chapter 9 that displays image-based credits
    /// from pre-made font images before transitioning to the Sans message.
    /// Uses frame-based animation similar to CS20_Ending.
    /// </summary>
    public class CS09_Credits : CutsceneEntity
    {
        private const int FPS = 12;
        private const float DELAY = 1f / 12f;
        
        public const string FlagCreditsComplete = "ch9_credits_complete";
        
        private global::Celeste.Player player;
        
        // Atlas-based frame system (like CS20_Ending)
        private Atlas Atlas;
        private List<MTexture> Frames;
        private int frame;
        private float fade = 1f;
        private float shine = 0f;
        private Color fadeColor = Color.Black;
        private Vector2 center = CelesteGame.TargetCenter;
        
#pragma warning disable CS0414
        private bool showingCredits;
#pragma warning restore CS0414
        
        public CS09_Credits(global::Celeste.Player player) : base(true, false)
        {
            // Depth -10000 renders in front of player (depth 0) but behind HUD/textbox (depth -10500+)
            base.Depth = -10000;
            this.player = player;
            
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                player.DummyAutoAnimate = false;
                player.Sprite.Rate = 0f;
            }
            
            RemoveOnSkipped = false;
        }
        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level obj = scene as Level;
            obj.TimerStopped = true;
            obj.TimerHidden = true;
            obj.SaveQuitDisabled = true;
            obj.PauseLock = true;
            obj.AllowHudHide = false;
        }
        
        public override void OnBegin(Level level)
        {
            Audio.SetAmbience(null);
            Add(new Coroutine(CreditsSequence(level), true));
        }
        
        private IEnumerator CreditsSequence(Level level)
        {
            // Set player to dummy state and make invisible
            if (player != null)
            {
                player.Visible = false;
            }
            
            if (level.Wipe != null)
            {
                level.Wipe.Cancel();
            }
            
            yield return 1f;
            
            // Load the credits atlas (similar to CS20_Ending loading TheEnd atlas)
            Atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Gui", "credits", "Part2"), Atlas.AtlasDataFormat.PackerNoAtlas);
            Frames = Atlas.GetAtlasSubtextures("");
            
            level.PauseLock = false;
            
            // Fade to black initially
            yield return 0.5f;
            
            // Show credits intro dialog
            yield return Textbox.Say("DZ_CH9_CREDITS_INTRO");
            
            // Start showing credits
            showingCredits = true;
            
            // Play credits music
            Audio.SetMusic("event:/DZ/music/lvl9/credit", true, true);
            
            // Fade in from black
            Add(new Coroutine(Fade(1f, 0f, 1f)));
            yield return 1f;
            
            // Play through all credit frames with transitions
            // Music duration: 1:07 (67 seconds) - timing adjusted to match
            if (Frames != null && Frames.Count > 0)
            {
                // Calculate hold time per frame to fit ~67 second music
                // Total time: ~1s intro fade + (frames × per-frame time) + 3s end = 67s
                // Per frame: 0.7s fade in + 5.0s hold + 0.7s fade out = 6.4s per frame
                
                // Show each frame with hold time
                for (int i = 0; i < Frames.Count; i++)
                {
                    frame = i;
                    
                    // Fade in this frame
                    Add(new Coroutine(Fade(1f, 0f, 0.7f)));
                    yield return 0.7f;
                    
                    // Hold the frame (allow skipping with confirm)
                    for (float t = 0; t < 5.0f; t += Engine.DeltaTime)
                    {
                        if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                        {
                            break;
                        }
                        yield return null;
                    }
                    
                    // Fade out this frame
                    Add(new Coroutine(Fade(0f, 1f, 0.7f)));
                    yield return 0.7f;
                }
            }
            
            // Hold on final state
            yield return 3f;
            
            // Fade out
            showingCredits = false;
            Add(new Coroutine(Fade(0f, 1f, 1f)));
            yield return 1f;
            
            // Mark credits as complete
            level.Session.SetFlag(FlagCreditsComplete, true);
            
            // Teleport to message end cutscene
            level.Add(new CS09_MessageEnd(player));
            
            EndCutscene(level, false);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<int> GetFrameData(string data)
        {
            List<int> list = new List<int>();
            string[] array = data.Split(new char[1] { ',' });
            for (int i = 0; i < array.Length; i++)
            {
                if (Enumerable.Contains(array[i], '*'))
                {
                    string[] array2 = array[i].Split(new char[1] { '*' });
                    int item = int.Parse(array2[0]);
                    int num = int.Parse(array2[1]);
                    for (int j = 0; j < num; j++)
                    {
                        list.Add(item);
                    }
                }
                else if (Enumerable.Contains(array[i], '-'))
                {
                    string[] array3 = array[i].Split(new char[1] { '-' });
                    int num2 = int.Parse(array3[0]);
                    int num3 = int.Parse(array3[1]);
                    for (int k = num2; k <= num3; k++)
                    {
                        list.Add(k);
                    }
                }
                else
                {
                    list.Add(int.Parse(array[i]));
                }
            }
            return list;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Shine(float from, float to, float duration, Ease.Easer ease = null)
        {
            if (ease == null)
            {
                ease = Ease.Linear;
            }
            shine = from;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                shine = from + (to - from) * ease(p);
                yield return null;
            }
            shine = to;
        }
        
        private IEnumerator Play(string data)
        {
            return Play(GetFrameData(data));
        }
        
        private IEnumerator Play(List<int> frames)
        {
            for (int i = 0; i < frames.Count; i++)
            {
                frame = frames[i];
                yield return DELAY;
            }
        }
        
        private IEnumerator Loop(string data, float duration = -1f)
        {
            List<int> frames = GetFrameData(data);
            float time = 0f;
            while (time < duration || duration < 0f)
            {
                frame = frames[(int)(time / DELAY) % frames.Count];
                time += Engine.DeltaTime;
                yield return null;
            }
        }
        
        private IEnumerator Fade(float from, float to, float duration, float delay = 0f)
        {
            fade = from;
            yield return delay;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                fade = from + (to - from) * p;
                yield return null;
            }
            fade = to;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            Level level = base.Scene as Level;
            if (level == null) return;
            
            // Get camera position for world-space rendering
            Camera camera = level.Camera;
            float camX = camera.X;
            float camY = camera.Y;
            
            // Draw black background covering entire visible area (in front of player, behind textbox)
            Draw.Rect(camX - 100f, camY - 100f, 420f, 280f, Color.Black);
            
            // Draw the current frame (like CS20_Ending) - centered on camera view
            if (Atlas != null && Frames != null && frame < Frames.Count)
            {
                MTexture mTexture = Frames[frame];
                MTexture linkedTexture = Atlas.GetLinkedTexture(mTexture.AtlasPath);
                Vector2 camCenter = new Vector2(camX + 160f, camY + 90f);
                float scaleX = 320f / mTexture.Width;
                float scaleY = 180f / mTexture.Height;
                float scale = Math.Min(scaleX, scaleY);
                linkedTexture?.DrawCentered(camCenter, Color.White, scale);
                mTexture.DrawCentered(camCenter, Color.White, scale);
            }
            
            // Draw shining bright overlay (white additive glow)
            if (shine > 0f)
            {
                Draw.Rect(camX - 100f, camY - 100f, 420f, 280f, Color.White * shine);
            }
            
            // Draw fade overlay
            Draw.Rect(camX - 100f, camY - 100f, 420f, 280f, fadeColor * fade);
        }
        
        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyGravity = true;
            }
            
            // Dispose of the atlas when done
            if (Atlas != null)
            {
                Atlas.Dispose();
            }
            Atlas = null;
            
            Audio.SetMusic(null, true, true);
        }
    }
}
