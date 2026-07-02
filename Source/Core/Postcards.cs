using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace DZ
{
    /// <summary>
    /// A postcard entity that displays a postcard with custom text and plays custom sounds when appearing and disappearing.
    /// Used for the postcards shown after beating each chapter in DesoloZantas.
    /// </summary>
    public class PostcardDZ : Entity
    {
    private const float TextScale = 0.7f;

    private MTexture postcard;

    private VirtualRenderTarget target;

    private FancyText.Text text;

    private float alpha;

    private float scale;

    private float rotation;

    private float buttonEase;

    private string sfxEventIn;

    private string sfxEventOut;

    private Coroutine easeButtonIn;

    public MTexture Postcard
    {
        get
        {
            return postcard;
        }
        set
        {
            postcard = value;
        }
    }

    public PostcardDZ(string msg, int area)
        : this(msg, "event:/ui/main/postcard_ch" + area + "_in", "event:/ui/main/postcard_ch" + area + "_out")
    {
    }

    public PostcardDZ(string msg, string sfxEventIn, string sfxEventOut)
    {
        alpha = 1f;
        scale = 1f;
        Visible = false;
        base.Tag = Tags.HUD;
        this.sfxEventIn = sfxEventIn;
        this.sfxEventOut = sfxEventOut;
        postcard = GFX.Gui["postcard"];
        text = FancyText.Parse(msg, (int)((float)(postcard.Width - 120) / 0.7f), -1, 1f, Color.Black * 0.6f);
    }

    public IEnumerator DisplayRoutine()
    {
        yield return EaseIn();
        yield return 0.75f;
        while (!Input.MenuConfirm.Pressed)
        {
            yield return null;
        }
        Audio.Play("event:/ui/main/button_lowkey");
        yield return EaseOut();
        yield return 1.2f;
    }

    public IEnumerator EaseIn()
    {
        Audio.Play(sfxEventIn);
        Vector2 vector = new Vector2(Engine.Width, Engine.Height) / 2f;
        Vector2 from = vector + new Vector2(0f, 200f);
        Vector2 to = vector;
        float rFrom = -0.1f;
        float rTo = 0.05f;
        Visible = true;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.8f)
        {
            Position = from + (to - from) * Ease.CubeOut(p);
            alpha = Ease.CubeOut(p);
            rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
            yield return null;
        }
        Add(easeButtonIn = new Coroutine(EaseButtinIn()));
    }

    private IEnumerator EaseButtinIn()
    {
        yield return 0.75f;
        while ((buttonEase += Engine.DeltaTime * 2f) < 1f)
        {
            yield return null;
        }
    }

    public IEnumerator EaseOut()
    {
        Audio.Play(sfxEventOut);
        if (easeButtonIn != null)
        {
            easeButtonIn.RemoveSelf();
        }
        Vector2 from = Position;
        Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0f, -200f);
        float rFrom = rotation;
        float rTo = rotation + 0.1f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            Position = from + (to - from) * Ease.CubeIn(p);
            alpha = 1f - Ease.CubeIn(p);
            rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
            buttonEase = Calc.Approach(buttonEase, 0f, Engine.DeltaTime * 8f);
            yield return null;
        }
        alpha = 0f;
        RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void BeforeRender()
    {
        if (target == null)
        {
            target = VirtualContent.CreateRenderTarget("postcard", postcard.Width, postcard.Height);
        }
        Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin();
        string text = Dialog.Clean("FILE_DEFAULT");
        if (SaveData.Instance != null && Dialog.Language.CanDisplay(SaveData.Instance.Name))
        {
            text = SaveData.Instance.Name;
        }
        postcard.Draw(Vector2.Zero);
        ActiveFont.Draw(text, new Vector2(115f, 30f), Vector2.Zero, Vector2.One * 0.9f, Color.Black * 0.7f);
        this.text.DrawJustifyPerLine(new Vector2(postcard.Width, postcard.Height) / 2f + new Vector2(0f, 40f), new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
        Draw.SpriteBatch.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        if (target != null)
        {
            Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0f);
        }
        if (buttonEase > 0f)
        {
            Input.GuiButton(Input.MenuConfirm, Input.PrefixMode.Latest, null).DrawCentered(new Vector2(Engine.Width - 120, (float)(Engine.Height - 100) - 20f * Ease.CubeOut(buttonEase)), Color.White * Ease.CubeOut(buttonEase));
        }
    }

    public override void Removed(Scene scene)
    {
        Dispose();
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        Dispose();
        base.SceneEnd(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Dispose()
    {
        if (target != null)
        {
            target.Dispose();
        }
        target = null;
    }

    public PostcardDZ(string msg)
        : this(msg, "event:/pusheen/ui/main/postcard_csides_in", "event:/pusheen/ui/main/postcard_csides_out")
    {
    }

    public static PostcardDZ CreateDSides(string msg)
    {
        return new PostcardDZ(msg, "event:/pusheen/ui/main/postcard_dsides_in", "event:/pusheen/ui/main/postcard_dsides_out");
    }

    public PostcardDZ(string msg, string soundId)
        : this(msg, GetSoundEventIn(soundId), GetSoundEventOut(soundId))
    {
    }

    private static string GetSoundEventIn(string soundId)
    {
        return GetSoundEventBase(soundId) + "_in";
    }

    private static string GetSoundEventOut(string soundId)
    {
        return GetSoundEventBase(soundId) + "_out";
    }

    private static string GetSoundEventBase(string soundId)
    {
        if (string.IsNullOrEmpty(soundId))
        {
            soundId = "csides";
        }

        if (soundId.StartsWith("event:/"))
        {
            return soundId;
        }
        else if (soundId == "variants")
        {
            return "event:/pusheen/new_content/ui/postcard_variants";
        }
        else
        {
            string text = "event:/ui/main/postcard_";
            if (int.TryParse(soundId, out var _))
            {
                text += "ch";
            }
            return text + soundId;
        }
    }
}

    /// <summary>
    /// Vignette that displays a postcard dialog when entering a chapter for the first time.
    /// Shows chapter intro, character art, and description before transitioning to the map.
    /// </summary>
    public class PostcardDialogVignette : Scene
    {
        private Session session;
        private int chapterNumber;
        private PostcardDZ postcard;
        private bool completed;

        public PostcardDialogVignette(Session session, int chapterNumber)
        {
            this.session = session;
            this.chapterNumber = chapterNumber;
            this.completed = false;
        }

        public override void Begin()
        {
            base.Begin();
            var entity = new Entity();
            entity.Add(new Coroutine(ShowPostcardRoutine()));
            Add(entity);
        }

        private IEnumerator ShowPostcardRoutine()
        {
            yield return 0.5f;

            string postcardMsg = GetPostcardMessage(chapterNumber);
            postcard = new PostcardDZ(postcardMsg, chapterNumber);
            Add(postcard);

            yield return postcard.DisplayRoutine();

            completed = true;
            yield return TransitionToMap();
        }

        private string GetPostcardMessage(int chapter)
        {
            return chapter switch
            {
                1 => "{LIGHT}Chapter 1: The City{/}\n{SMALL}Enter the urban landscape and uncover ancient secrets.",
                2 => "{LIGHT}Chapter 2: Nightmare{/}\n{SMALL}Descend into a world of darkness and illusion.",
                3 => "{LIGHT}Chapter 3: The Stars{/}\n{SMALL}Reach for the celestial heights.",
                4 => "{LIGHT}Chapter 4: Legend{/}\n{SMALL}Face the trials of myth and destiny.",
                5 => "{LIGHT}Chapter 5: Restore{/}\n{SMALL}Rebuild what was broken.",
                6 => "{LIGHT}Chapter 6: Stronghold{/}\n{SMALL}Fortify your resolve.",
                7 => "{LIGHT}Chapter 7: Hell{/}\n{SMALL}Brave the infernal depths.",
                8 => "{LIGHT}Chapter 8: Truth{/}\n{SMALL}Discover the ultimate reality.",
                9 => "{LIGHT}Chapter 9: Summit{/}\n{SMALL}Reach the peak of your journey.",
                10 => "{LIGHT}Chapter 10: Ruins{/}\n{SMALL}Explore the remnants of the past.",
                11 => "{LIGHT}Chapter 11: Snow{/}\n{SMALL}Navigate frozen wastelands.",
                12 => "{LIGHT}Chapter 12: Water{/}\n{SMALL}Master the flowing currents.",
                13 => "{LIGHT}Chapter 13: Fire{/}\n{SMALL}Embrace the burning passion.",
                14 => "{LIGHT}Chapter 14: Digital{/}\n{SMALL}Enter the virtual realm.",
                15 => "{LIGHT}Chapter 15: Castle{/}\n{SMALL}Ascend the fortress of power.",
                16 => "{LIGHT}Chapter 16: Corruption{/}\n{SMALL}Face the spreading darkness.",
                _ => "{LIGHT}Welcome{/}\n{SMALL}Your adventure awaits."
            };
        }

        private IEnumerator TransitionToMap()
        {
            yield return 0.3f;
            // Directly load level - patch will detect we're coming from PostcardDialogVignette
            LevelEnter.Go(session, fromSaveData: false);
        }
    }

    /// <summary>
    /// Vignette that displays an outro postcard when exiting a chapter.
    /// Shows completion message and transitions back to overworld.
    /// </summary>
    public class PostcardOutroVignette : Scene
    {
        private Session session;
        private int chapterNumber;
        private PostcardDZ postcard;

        public PostcardOutroVignette(Session session, int chapterNumber)
        {
            this.session = session;
            this.chapterNumber = chapterNumber;
        }

        public override void Begin()
        {
            base.Begin();
            var entity = new Entity();
            entity.Add(new Coroutine(ShowPostcardRoutine()));
            Add(entity);
        }

        private IEnumerator ShowPostcardRoutine()
        {
            yield return 0.5f;

            string postcardMsg = GetOutroPostcardMessage(chapterNumber);
            postcard = new PostcardDZ(postcardMsg, chapterNumber);
            Add(postcard);

            yield return postcard.DisplayRoutine();

            yield return TransitionToOverworld();
        }

        private string GetOutroPostcardMessage(int chapter)
        {
            return chapter switch
            {
                18 => "{LIGHT}Chapter 18: Complete{/}\n{SMALL}The heart of existence has been reached.",
                _ => "{LIGHT}Chapter Complete{/}\n{SMALL}Your journey continues..."
            };
        }

        private IEnumerator TransitionToOverworld()
        {
            yield return 0.3f;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, null);
        }
    }
}
