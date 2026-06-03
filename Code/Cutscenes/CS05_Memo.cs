#nullable enable
namespace Celeste.Cutscenes;
// Note: Removed [CustomEntity] - cutscene entities are created programmatically, not placed in maps
[HotReloadable]
public class Cs05Memo : CutsceneEntity
{
    private const string read_once_flag = "memo_read_mod";
    private readonly global::Celeste.Player player;
    private MemoPage? memo;

    public Cs05Memo(global::Celeste.Player player, MemoPage memo)
    {
        this.player = player;
        this.memo = memo;
    }

    public Cs05Memo(IEnumerable<Component> components) : base()
    {
        foreach (var component in components) Add(component);
        player = new global::Celeste.Player(Vector2.Zero, (global::Celeste.PlayerSpriteMode)0); // Initialize 'player' with a default value.
        memo = null; // Initialize 'memo' with a default instance.
    }

    public override void OnBegin(Level level) => Add(new Coroutine(Routine()));

    private IEnumerator Routine()
    {
        Cs05Memo cs03Memo = this;
        cs03Memo.player.StateMachine.State = Player.StDummy;
        cs03Memo.player.StateMachine.Locked = true;
        if (!cs03Memo.Level.Session.GetFlag("memo_read_mod"))
        {
            yield return Textbox.Say("ch5_memo_opening");
            yield return 0.1f;
        }
        cs03Memo.memo = [];
        cs03Memo.Scene.Add(cs03Memo.memo);
        yield return cs03Memo.memo!.EaseIn();
        yield return cs03Memo.memo!.Wait();
        yield return cs03Memo.memo!.EaseOut();
        cs03Memo.memo = null!;
        EndCutscene(cs03Memo.Level);
    }

    private static void EndCutscene(Level level)
    {
        if (level != null)
            level.EndCutscene();
    }

    public override void OnEnd(Level level)
    {
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        level.Session.SetFlag("memo_read");
        if (memo == null)
            return;
        memo.RemoveSelf();
    }

    public class MemoPage : Entity
    {
        private const float text_scale = 0.75f;
        private const float paper_scale = 1.5f;
        private readonly Atlas atlas;
        private readonly MTexture paper;
        private readonly MTexture title;
        private VirtualRenderTarget? target = null!;
        private readonly FancyText.Text text;
        private readonly float textDownscale = 1f;
        private float alpha = 1f;
        private readonly float scale = 1f;
        private float rotation;
        private float timer;
        private bool easingOut;

        public MemoPage(VirtualRenderTarget? target)
        {
            this.target = target;
            Tag = (int)Tags.HUD;
            atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Memo"), Atlas.AtlasDataFormat.Packer);
            paper = atlas[nameof(memo)];
            title = !atlas.Has("title_" + Dialog.Language) ? atlas["title_english"] : atlas["title_" + Dialog.Language];
            float num1 = (float)(paper.Width * 1.5 - 120.0);
            text = FancyText.Parse(Dialog.Get("MAGGYHELPER_CH5_MEMO"), (int)(num1 / 0.75), -1, defaultColor: Color.Black * 0.6f);
            float num2 = text.WidestLine() * 0.75f;
            if (num2 > (double)num1)
                textDownscale = num1 / num2;
            Add(new BeforeRenderHook(BeforeRender));
        }

        public MemoPage()
        {
            Tag = (int)Tags.HUD; // Assigns the tag for MemoPage.

            // Loads the atlas for memo graphics.
            atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Memo"), Atlas.AtlasDataFormat.Packer);

            // Loads the 'paper' texture from the atlas.
            paper = atlas[nameof(memo)];

            // Loads the title texture based on the current language.
            title = atlas.Has("title_" + Dialog.Language)
                ? atlas["title_" + Dialog.Language]
                : atlas["title_english"];

            // Calculates the width for the text box and parses the text content.
            float textAreaWidth = (float)(paper.Width * paper_scale - 120.0);
            text = FancyText.Parse(Dialog.Get("MAGGYHELPER_CH5_MEMO"), (int)(textAreaWidth / text_scale), -1,
                defaultColor: Color.Black * 0.6f);

            // Adjusts text scale if necessary, based on the widest text line.
            float widestLine = text.WidestLine() * text_scale;
            if (widestLine > textAreaWidth)
            {
                textDownscale = textAreaWidth / widestLine;
            }

            // Adds a hook to handle rendering before the main rendering pass.
            Add(new BeforeRenderHook(BeforeRender));
        }

        public IEnumerator EaseIn()
        {
            MemoPage memoPage = this;
            Audio.Play("event:/game/03_resort/memo_in");
            Vector2 from = new(Engine.Width / 2, Engine.Height + 100);
            Vector2 to = new(Engine.Width / 2, Engine.Height / 2 - 150);
            float rFrom = -0.1f;
            float rTo = 0.05f;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                memoPage.Position = from + (to - from) * Ease.CubeOut(p);
                memoPage.alpha = Ease.CubeOut(p);
                memoPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                yield return null;
            }
        }

        public IEnumerator Wait()
        {
            MemoPage memoPage = this;
            float start = memoPage.Position.Y;
            int index = 0;
            while (!Input.MenuCancel.Pressed)
            {
                float num = start - index * 400;
                memoPage.Position.Y += (float)((num - (double)memoPage.Position.Y) * (1.0 - Math.Pow(0.0099999997764825821, Engine.DeltaTime)));
                if (Input.MenuUp.Pressed && index > 0)
                    --index;
                else if (index < 2)
                {
                    if (Input.MenuDown.Pressed && !Input.MenuDown.Repeating || Input.MenuConfirm.Pressed)
                        ++index;
                }
                else if (Input.MenuConfirm.Pressed)
                    break;
                yield return null;
            }
            Audio.Play("event:/ui/main/button_lowkey");
        }

        public IEnumerator EaseOut()
        {
            MemoPage memoPage = this;
            Audio.Play("event:/game/03_resort/memo_out");
            memoPage.easingOut = true;
            Vector2 from = memoPage.Position;
            if (memoPage.target != null)
            {
                Vector2 to = new(Engine.Width / 2, -memoPage.target.Height);
                float rFrom = memoPage.rotation;
                float rTo = memoPage.rotation + 0.1f;
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 1.5f)
                {
                    memoPage.Position = from + (to - from) * Ease.CubeIn(p);
                    memoPage.alpha = 1f - Ease.CubeIn(p);
                    memoPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                    yield return null;
                }
            }

            memoPage.RemoveSelf();
        }

        public void BeforeRender()
        {
            target ??= VirtualContent.CreateRenderTarget("oshiro-memo", (int)(paper.Width * 1.5), (int)(paper.Height * 1.5));
            Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            paper.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
            title.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
            text.Draw(new Vector2((float)(paper.Width * 1.5 / 2.0), 210f), new Vector2(0.5f, 0.0f), Vector2.One * 0.75f * textDownscale, 1f);
            Draw.SpriteBatch.End();
        }

        public override void Removed(Scene scene)
        {
            target?.Dispose();
            target = null;
            atlas.Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            target?.Dispose();
            target = null;
            atlas.Dispose();
            base.SceneEnd(scene);
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene) || target == null)
                return;
            Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, 0.0f) / 2f, scale, SpriteEffects.None, 0.0f);
            if (easingOut)
                return;
            GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height + (timer % 1.0 < 0.25 ? 6 : 0)));
        }
    }
}




