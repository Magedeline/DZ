using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// A reimplementation of Celeste's ReflectionHeartStatue (Chapter 6 secret heart puzzle)
/// for use in modded chapters. The player must perform a sequence of directional dashes
/// to light four torches, which then spawns a heart gem.
///
/// Requires 5 nodes in the editor:
///   Nodes 0-3 = torch positions
///   Node 4    = gem hint display position (above the statue)
///
/// All sprite/sound paths, the dash code, torch count, and flag prefix are configurable
/// via EntityData so multiple instances can coexist.
/// </summary>
[CustomEntity("MaggyHelper/ReflectionHeartStatue")]
[Tracked]
public class MaggyReflectionHeartStatue : Entity
{
    // â”€â”€ Inner: Torch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class Torch : Entity
    {
        public string[] Code;

        private Sprite sprite;
        private Session session;
        private string flagPrefix;
        private string torchSpritePath;
        private string hintSpritePath;
        private string torchSoundPrefix;

        public string Flag => flagPrefix + Index;
        public bool Activated => session.GetFlag(Flag);
        public int Index { get; private set; }

        public Torch(
            Session session,
            Vector2 position,
            int index,
            string[] code,
            string flagPrefix,
            string torchSpritePath,
            string hintSpritePath,
            string torchSoundPrefix)
            : base(position)
        {
            Index = index;
            Code = code;
            Depth = 8999;
            this.session = session;
            this.flagPrefix = flagPrefix;
            this.torchSpritePath = torchSpritePath;
            this.hintSpritePath = hintSpritePath;
            this.torchSoundPrefix = torchSoundPrefix;

            // Hint image (directional arrow for this torch)
            var subtextures = GFX.Game.GetAtlasSubtextures(hintSpritePath);
            if (index < subtextures.Count)
            {
                Image image = new Image(subtextures[index]);
                image.CenterOrigin();
                image.Position = new Vector2(0f, 28f);
                Add(image);
            }

            // Torch sprite
            Add(sprite = new Sprite(GFX.Game, torchSpritePath));
            sprite.AddLoop("idle", "", 0f, default(int));
            sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
            sprite.Play("idle");
            sprite.Origin = new Vector2(32f, 64f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Activated)
                PlayLit();
        }

        public void Activate()
        {
            session.SetFlag(Flag);
            Alarm.Set(this, 0.2f, () =>
            {
                Audio.Play(torchSoundPrefix + (Index + 1), Position);
                PlayLit();
            });
        }

        private void PlayLit()
        {
            sprite.Play("lit");
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            Add(new VertexLight(Color.LightSeaGreen, 1f, 24, 48));
            Add(new BloomPoint(0.6f, 16f));
        }
    }

    // â”€â”€ Default dash code (same as vanilla ch6) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private static readonly string[] DefaultCode = { "U", "L", "DR", "UR", "L", "UL" };

    // Configurable dash-direction colour map (same order as vanilla ForsakenCitySatellite.Colors)
    private static readonly Dictionary<string, Color> DirectionColors = new()
    {
        { "U",  Color.LightCyan },
        { "D",  Color.LightCoral },
        { "L",  Color.LightGreen },
        { "R",  Color.LightGoldenrodYellow },
        { "UL", Color.Aquamarine },
        { "UR", Color.CornflowerBlue },
        { "DL", Color.Salmon },
        { "DR", Color.Orchid },
    };

    // â”€â”€ Fields â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string[] code;
    private string flagPrefix;
    private string statueSpritePath;
    private string torchSpritePath;
    private string hintSpritePath;
    private string gemSpritePath;
    private string heartSpritePath;
    private string dashSoundEvent;
    private string torchSoundPrefix;
    private string heartAppearSoundEvent;

    private List<string> currentInputs = new();
    private List<Torch> torches = new();
    private Vector2 offset;
    private Vector2[] nodes;
    private DashListener dashListener;
    private bool enabled;

    // â”€â”€ Constructor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public MaggyReflectionHeartStatue(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        this.offset = offset;
        nodes = data.Nodes;
        Depth = 8999;

        // Parse configurable dash code (comma-separated, e.g. "U,L,DR,UR,L,UL")
        string codeStr = data.Attr("code", "U,L,DR,UR,L,UL");
        code = codeStr.Split(',');

        flagPrefix       = data.Attr("flagPrefix", "heartTorch_");
        statueSpritePath  = data.Attr("statueSprite", "objects/reflectionHeart/statue");
        torchSpritePath   = data.Attr("torchSprite", "objects/reflectionHeart/torch");
        hintSpritePath    = data.Attr("hintSprite", "objects/reflectionHeart/hint");
        gemSpritePath     = data.Attr("gemSprite", "objects/reflectionHeart/gem");
        heartSpritePath   = data.Attr("heartSprite", "collectables/heartgem/white00");

        dashSoundEvent        = data.Attr("dashSound", "event:/game/06_reflection/supersecret_dashflavour");
        torchSoundPrefix      = data.Attr("torchSoundPrefix", "event:/game/06_reflection/supersecret_torch_");
        heartAppearSoundEvent = data.Attr("heartAppearSound", "event:/game/06_reflection/supersecret_heartappear");
    }

    // â”€â”€ Added â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Session session = (Scene as Level).Session;

        // Statue image
        Image statueImage = new Image(GFX.Game[statueSpritePath]);
        statueImage.JustifyOrigin(0.5f, 1f);
        statueImage.Origin.Y -= 1f;
        Add(statueImage);

        // Build flipped code variants for 4 torches
        List<string[]> codeVariants = new()
        {
            code,
            FlipCode(h: true,  v: false),
            FlipCode(h: false, v: true),
            FlipCode(h: true,  v: true),
        };

        // Spawn torches at nodes 0-3
        int torchCount = System.Math.Min(4, nodes.Length);
        for (int i = 0; i < torchCount; i++)
        {
            var torch = new Torch(
                session, offset + nodes[i], i, codeVariants[i],
                flagPrefix, torchSpritePath, hintSpritePath, torchSoundPrefix);
            Scene.Add(torch);
            torches.Add(torch);
        }

        // Gem hint display at node 4 (if provided)
        if (nodes.Length > 4)
        {
            int codeLen = code.Length;
            Vector2 gemOrigin = nodes[4] + offset - Position;
            for (int j = 0; j < codeLen; j++)
            {
                Image gemImage = new Image(GFX.Game[gemSpritePath]);
                gemImage.CenterOrigin();

                // Use direction-based colour if available, fallback to white
                if (DirectionColors.TryGetValue(code[j], out Color col))
                    gemImage.Color = col;
                else
                    gemImage.Color = Color.White;

                gemImage.Position = gemOrigin + new Vector2(((float)j - (float)(codeLen - 1) / 2f) * 24f, 0f);
                Add(gemImage);
                Add(new BloomPoint(gemImage.Position, 0.3f, 12f));
            }
        }

        enabled = !session.HeartGem;
        if (!enabled)
            return;

        // Dash listener
        Add(dashListener = new DashListener());
        dashListener.OnDash = (Vector2 dir) =>
        {
            string text = "";
            if (dir.Y < 0f) text = "U";
            else if (dir.Y > 0f) text = "D";
            if (dir.X < 0f) text += "L";
            else if (dir.X > 0f) text += "R";

            // Compute direction index for sound parameter
            int dirIndex = 0;
            if      (dir.X < 0f && dir.Y == 0f) dirIndex = 1;
            else if (dir.X < 0f && dir.Y <  0f) dirIndex = 2;
            else if (dir.X == 0f && dir.Y < 0f) dirIndex = 3;
            else if (dir.X > 0f && dir.Y <  0f) dirIndex = 4;
            else if (dir.X > 0f && dir.Y == 0f) dirIndex = 5;
            else if (dir.X > 0f && dir.Y >  0f) dirIndex = 6;
            else if (dir.X == 0f && dir.Y > 0f) dirIndex = 7;
            else if (dir.X < 0f && dir.Y >  0f) dirIndex = 8;

            Audio.Play(
                dashSoundEvent,
                Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero,
                "dash_direction", dirIndex);

            currentInputs.Add(text);
            if (currentInputs.Count > code.Length)
                currentInputs.RemoveAt(0);

            foreach (var torch in torches)
            {
                if (!torch.Activated && CheckCode(torch.Code))
                    torch.Activate();
            }

            CheckIfAllActivated();
        };

        CheckIfAllActivated(skipRoutine: true);
    }

    // â”€â”€ Code helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string[] FlipCode(bool h, bool v)
    {
        string[] result = new string[code.Length];
        for (int i = 0; i < code.Length; i++)
        {
            string s = code[i];
            if (h) s = s.Contains('L') ? s.Replace('L', 'R') : s.Replace('R', 'L');
            if (v) s = s.Contains('U') ? s.Replace('U', 'D') : s.Replace('D', 'U');
            result[i] = s;
        }
        return result;
    }

    private bool CheckCode(string[] targetCode)
    {
        if (currentInputs.Count < targetCode.Length)
            return false;
        for (int i = 0; i < targetCode.Length; i++)
        {
            if (!currentInputs[i].Equals(targetCode[i]))
                return false;
        }
        return true;
    }

    // â”€â”€ Activation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void CheckIfAllActivated(bool skipRoutine = false)
    {
        if (!enabled) return;

        foreach (var torch in torches)
        {
            if (!torch.Activated) return;
        }

        ActivateHeart(skipRoutine);
    }

    private void ActivateHeart(bool skipRoutine)
    {
        enabled = false;
        if (skipRoutine)
        {
            Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
        }
        else
        {
            Add(new Coroutine(ActivateRoutine()));
        }
    }

    private IEnumerator ActivateRoutine()
    {
        yield return 0.533f;
        Audio.Play(heartAppearSoundEvent);

        Entity dummy = new Entity(Position + new Vector2(0f, -52f)) { Depth = 1 };
        Scene.Add(dummy);

        Image white = new Image(GFX.Game[heartSpritePath]);
        white.CenterOrigin();
        white.Scale = Vector2.Zero;
        dummy.Add(white);

        BloomPoint glow = new BloomPoint(0f, 16f);
        dummy.Add(glow);

        List<Entity> absorbs = new();
        for (int i = 0; i < 20; i++)
        {
            AbsorbOrb orb = new AbsorbOrb(Position + new Vector2(0f, -20f), dummy);
            Scene.Add(orb);
            absorbs.Add(orb);
            yield return null;
        }

        yield return 0.8f;

        float duration = 0.6f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
        {
            white.Scale = Vector2.One * p;
            glow.Alpha = p;
            (Scene as Level).Shake();
            yield return null;
        }

        foreach (var item in absorbs)
            item.RemoveSelf();

        (Scene as Level).Flash(Color.White);
        Scene.Remove(dummy);
        Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
    }

    // â”€â”€ Update â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public override void Update()
    {
        if (dashListener != null && !enabled)
        {
            Remove(dashListener);
            dashListener = null;
        }
        base.Update();
    }
}

