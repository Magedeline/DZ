using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using MaggyHelper.Cutscenes;

namespace MaggyHelper;

public class AreaComplete : Scene
{
    public Session Session;

    private bool finishedSlide;

    private bool canConfirm = true;

    private HiresSnow snow;

    private float speedrunTimerDelay = 1.1f;

    private float speedrunTimerEase;

    private string speedrunTimerChapterString;

    private string speedrunTimerFileString;

    private string chapterSpeedrunText = Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER") + ":";

    private AreaCompleteTitle title;

    private CompleteRenderer complete;

    private string version;

    private static string versionFull;

    private static float versionOffset;

    private static Texture2D identicon;

    private static float everestTime;

    private static bool isPieScreen;

    private float buttonTimerDelay;

    private float buttonTimerEase;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public AreaComplete(Session session, XmlElement xml, Atlas atlas, HiresSnow snow, MapMetaCompleteScreen meta)
    {
        Session = session;
        version = CelesteGame.Instance.Version.ToString();
        if (session.Area.ID != 7)
        {
            string text = GetCustomCompleteScreenTitle();
            if (text == null)
            {
                text = Dialog.Clean(string.Concat("areacomplete_", session.Area.Mode, session.FullClear ? "_fullclear" : ""));
            }
            Vector2 origin = new Vector2(960f, 200f);
            float scale = Math.Min(1600f / ActiveFont.Measure(text).X, 3f);
            title = new AreaCompleteTitle(origin, text, scale);
        }
        Add(complete = new CompleteRenderer(xml, atlas, 1f, delegate
        {
            finishedSlide = true;
        }, meta));
        if (title != null)
        {
            complete.RenderUI = delegate
            {
                title.DrawLineUI();
            };
        }
        complete.RenderPostUI = RenderUI;
        speedrunTimerChapterString = TimeSpan.FromTicks(Session.Time).ShortGameplayFormat();
        speedrunTimerFileString = Dialog.FileTime(SaveData.Instance.Time);
        SpeedrunTimerDisplay.CalculateBaseSizes();
        Add(this.snow = snow);
        base.RendererList.UpdateLists();
        Audio.Play(OverworldMusicManager.GetCompletionMusic((int)session.Area.Mode));
    }

    public override void End()
    {
        Orig_End();
        DisposeAreaCompleteInfoForEverest();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Orig_Update();
        buttonTimerDelay -= Engine.DeltaTime;
        if (buttonTimerDelay <= 0f)
        {
            buttonTimerEase = Calc.Approach(buttonTimerEase, 1f, Engine.DeltaTime * 4f);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RenderUI()
    {
        Orig_RenderUI();
        if (!(buttonTimerEase > 0f) || Settings.Instance.SpeedrunClock != SpeedrunType.Off)
        {
            return;
        }
        MTexture mTexture = Input.GuiButton(Input.MenuConfirm, Input.PrefixMode.Latest, null);
        Vector2 vector = new Vector2(1860f - (float)mTexture.Width, 1020f - (float)mTexture.Height);
        float num = buttonTimerEase * buttonTimerEase;
        float num2 = 0.9f + buttonTimerEase * 0.1f;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 && j != 0)
                {
                    mTexture.DrawCentered(vector + new Vector2(i, j), Color.Black * num * num * num * num, Vector2.One * num2);
                }
            }
        }
        mTexture.DrawCentered(vector, Color.White * num, Vector2.One * num2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Info(float ease, string speedrunTimerChapterString, string speedrunTimerFileString, string chapterSpeedrunText, string versionText)
    {
        if (ease > 0f && Settings.Instance.SpeedrunClock != SpeedrunType.Off)
        {
            Vector2 vector = new Vector2(80f - 300f * (1f - Ease.CubeOut(ease)), 1000f);
            if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
            {
                SpeedrunTimerDisplay.DrawTime(vector, speedrunTimerChapterString);
            }
            else
            {
                vector.Y -= 16f;
                SpeedrunTimerDisplay.DrawTime(vector, speedrunTimerFileString);
                ActiveFont.DrawOutline(chapterSpeedrunText, vector + new Vector2(0f, 40f), new Vector2(0f, 1f), Vector2.One * 0.6f, Color.White, 2f, Color.Black);
                SpeedrunTimerDisplay.DrawTime(vector + new Vector2(ActiveFont.Measure(chapterSpeedrunText).X * 0.6f + 8f, 40f), speedrunTimerChapterString, 0.6f);
            }
            VersionNumberAndVariants(versionText, ease, 1f);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void VersionNumberAndVariants(string version, float ease, float alpha)
    {
        everestTime += Engine.RawDeltaTime;
        Orig_VersionNumberAndVariants(versionFull, ease, alpha);
        if (identicon != null)
        {
            _ = everestTime;
            float rotation = MathF.PI / 50f * (float)Math.Sin(everestTime * 0.8f);
            Vector2 position = new Vector2(1920f * (isPieScreen ? 0.05f : 0.5f), 930f);
            Rectangle bounds = identicon.Bounds;
            bounds.Height = 2;
            int num = 0;
            while (bounds.Y < identicon.Height)
            {
                Vector2 origin = new Vector2((float)identicon.Width * 0.5f + (float)Math.Round(2.5 + 2.5 * Math.Sin(everestTime + 0.12f * (float)num)), 2 * -num);
                Draw.SpriteBatch.Draw(identicon, position, bounds, Color.White * ease, rotation, origin, 1f, SpriteEffects.None, 0f);
                num++;
                bounds.Y += 2;
                bounds.Height = Math.Min(2, identicon.Height - bounds.Y);
            }
        }
    }

    public override void Begin()
    {
        base.Begin();
        InitAreaCompleteInfoForEverest2(pieScreen: false, Session);
        buttonTimerDelay = 2.2f;
        buttonTimerEase = 0f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InitAreaCompleteInfoForEverest(bool pieScreen)
    {
        InitAreaCompleteInfoForEverest2(pieScreen, null);
    }

    public static void InitAreaCompleteInfoForEverest2(bool pieScreen, Session session)
    {
        versionOffset = 0f;
        if (Settings.Instance.SpeedrunClock > SpeedrunType.Off)
        {
            versionFull = $"{CelesteGame.Instance.Version}\n{Everest.Build}";
            if (session != null && Everest.Content.TryGet($"Maps/Maggy/{AreaData.Get(session).Mode[(int)session.Area.Mode].Path}", out var metadata))
            {
                EverestModuleMetadata mod = metadata.Source.Mod;
                if (mod != null && mod.Multimeta?.Length >= 1)
                {
                    versionFull = $"{versionFull}\n{metadata.Source.Mod.Multimeta[0].Version}";
                    versionOffset -= 32f;
                }
            }
            identicon?.Dispose();
            identicon = CreateIdenticonTexture(CelesteGame.Instance.GraphicsDevice, Everest.InstallationHash, 100);
        }
        isPieScreen = pieScreen;
    }

    private static Texture2D CreateIdenticonTexture(GraphicsDevice graphicsDevice, byte[] hash, int size)
    {
        byte[] digest = SHA256.HashData(hash ?? Array.Empty<byte>());
        Color foreground = new Color(digest[0], digest[1], digest[2], 255);
        Color background = new Color(20, 20, 20, 255);
        Color[] data = new Color[size * size];

        for (int index = 0; index < data.Length; index++)
        {
            data[index] = background;
        }

        int grid = 5;
        int cellSize = Math.Max(1, size / grid);
        int xOffset = (size - cellSize * grid) / 2;
        int yOffset = (size - cellSize * grid) / 2;

        int bitIndex = 0;
        for (int y = 0; y < grid; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                bool filled = (digest[(bitIndex / 8) % digest.Length] & (1 << (bitIndex % 8))) != 0;
                bitIndex++;
                if (!filled)
                {
                    continue;
                }

                int left = xOffset + x * cellSize;
                int right = xOffset + (grid - 1 - x) * cellSize;
                int top = yOffset + y * cellSize;

                FillRect(data, size, left, top, cellSize, cellSize, foreground);
                if (right != left)
                {
                    FillRect(data, size, right, top, cellSize, cellSize, foreground);
                }
            }
        }

        Texture2D texture = new Texture2D(graphicsDevice, size, size);
        texture.SetData(data);
        return texture;
    }

    private static void FillRect(Color[] data, int textureWidth, int x, int y, int width, int height, Color color)
    {
        for (int row = 0; row < height; row++)
        {
            int yy = y + row;
            if (yy < 0)
            {
                continue;
            }

            for (int column = 0; column < width; column++)
            {
                int xx = x + column;
                if (xx < 0)
                {
                    continue;
                }

                int idx = yy * textureWidth + xx;
                if (idx >= 0 && idx < data.Length)
                {
                    data[idx] = color;
                }
            }
        }
    }

    public void Orig_End()
    {
        base.End();
        complete.Dispose();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Orig_Update()
    {
        base.Update();
        if (Input.MenuConfirm.Pressed && finishedSlide && canConfirm)
        {
            canConfirm = false;
            if (Session.Area.ID == 7 && Session.Area.Mode == AreaMode.Normal)
            {
                new FadeWipe(this, wipeIn: false, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                {
                    Session.RespawnPoint = null;
                    Session.FirstLevel = false;
                    Session.Level = "credits-summit";
                    Session.Audio.Music.Event = "event:/desolozantas/music/lvl17/main";
                    Session.Audio.Apply(false);
                    Engine.Scene = new LevelLoader(Session)
                    {
                        PlayerIntroTypeOverride = Player.IntroTypes.None,
                        Level = 
                        {
                            new CS17_Credits()
                        }
                    };
                });
            }
            else
            {
                HiresSnow outSnow = new HiresSnow();
                outSnow.Alpha = 0f;
                outSnow.AttachAlphaTo = new FadeWipe(this, wipeIn: false, delegate
                {
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, outSnow);
                });
                Add(outSnow);
            }
        }
        snow.Alpha = Calc.Approach(snow.Alpha, 0f, Engine.DeltaTime * 0.5f);
        snow.Direction.Y = Calc.Approach(snow.Direction.Y, 1f, Engine.DeltaTime * 24f);
        speedrunTimerDelay -= Engine.DeltaTime;
        if (speedrunTimerDelay <= 0f)
        {
            speedrunTimerEase = Calc.Approach(speedrunTimerEase, 1f, Engine.DeltaTime * 2f);
        }
        if (title != null)
        {
            title.Update();
        }
        if (CelesteGame.PlayMode == CelesteGame.PlayModes.Debug)
        {
            if (MInput.Keyboard.Pressed(Keys.F2))
            {
                CelesteGame.ReloadAssets(levels: false, graphics: true, hires: false);
                Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
            }
            else if (MInput.Keyboard.Pressed(Keys.F3))
            {
                CelesteGame.ReloadAssets(levels: false, graphics: true, hires: true);
                Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Orig_RenderUI()
    {
        Entities.Render();
        Info(speedrunTimerEase, speedrunTimerChapterString, speedrunTimerFileString, chapterSpeedrunText, version);
        if (complete.HasUI && title != null)
        {
            title.Render();
        }
    }

    public static void DisposeAreaCompleteInfoForEverest()
    {
        identicon?.Dispose();
        identicon = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Orig_VersionNumberAndVariants(string version, float ease, float _)
    {
        Vector2 vector = new(1820f + 300f * (1f - Ease.CubeOut(ease)), 1020f + versionOffset);
        if (SaveData.Instance.AssistMode || SaveData.Instance.VariantMode)
        {
            MTexture mTexture = GFX.Gui[SaveData.Instance.AssistMode ? "cs_assistmode" : "cs_desolovariantmode"];
            vector.Y -= 32f;
            mTexture.DrawJustified(vector + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Color.White, 0.6f);
            ActiveFont.DrawOutline(version, vector, new Vector2(0.5f, 0f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
        }
        else
        {
            ActiveFont.DrawOutline(version, vector, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
        }
    }

    private string GetCustomCompleteScreenTitle()
    {
        MapMetaCompleteScreenTitle mapMetaCompleteScreenTitle = AreaData.Get(Session.Area)?.Meta?.CompleteScreen?.Title;
        if (mapMetaCompleteScreenTitle == null)
        {
            return null;
        }
        string text = null;
        switch (Session.Area.Mode)
        {
            case AreaMode.Normal:
                text = !Session.FullClear ? mapMetaCompleteScreenTitle.ASide : mapMetaCompleteScreenTitle.FullClear;
                break;
            case AreaMode.BSide:
                text = mapMetaCompleteScreenTitle.BSide;
                break;
            case AreaMode.CSide:
                text = mapMetaCompleteScreenTitle.CSide;
                break;
        }
        if (text == null)
        {
            return null;
        }
        return Dialog.Clean(text);
    }
}

