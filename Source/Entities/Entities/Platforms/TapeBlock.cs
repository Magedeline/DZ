using System.Runtime.CompilerServices;

namespace Celeste.Entities;
[CustomEntity(ids: "DZ/TapeBlock")]
[Tracked(true)]
public class TapeBlock : Solid
{
    public enum Modes
    {
        Solid,
        Leaving,
        Disabled,
        Returning
    }

    private class BoxSide : Entity
    {
        private TapeBlock block;

        private Color color;

        public BoxSide(TapeBlock block, Color color)
        {
            this.block = block;
            this.color = color;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.blockHeight, color);
        }
    }

    public int Index;

    public float Tempo;

    public bool Activated;

    public Modes Mode;

    public EntityID ID;

    private int blockHeight = 2;

    private List<TapeBlock> group;

    private bool groupLeader;

    private Vector2 groupOrigin;

    private Color color;

    private List<Image> pressed = new List<Image>();

    private List<Image> solid = new List<Image>();

    private List<Image> all = new List<Image>();

    private LightOcclude occluder;

    private Wiggler wiggler;

    private Vector2 wigglerScaler;

    private BoxSide side;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public TapeBlock(Vector2 position, EntityID id, float width, float height, int index, float tempo)
        : base(position, width, height, safe: false)
    {
        SurfaceSoundIndex = 35;
        Index = index;
        Tempo = tempo;
        Collidable = false;
        ID = id;
        switch (Index)
        {
            default:
                color = Calc.HexToColor("49aaf0");
                break;
            case 1:
                color = Calc.HexToColor("f049be");
                break;
            case 2:
                color = Calc.HexToColor("fcdc3a");
                break;
            case 3:
                color = Calc.HexToColor("38e04e");
                break;
        }
        Add(occluder = new LightOcclude());
    }
    public TapeBlock(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f))
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Color color = Calc.HexToColor("667da5");
        Color disabledColor = new Color((float)(int)color.R / 255f * ((float)(int)this.color.R / 255f), (float)(int)color.G / 255f * ((float)(int)this.color.G / 255f), (float)(int)color.B / 255f * ((float)(int)this.color.B / 255f), 1f);
        scene.Add(side = new BoxSide(this, disabledColor));
        foreach (StaticMover staticMover in staticMovers)
        {
            if (staticMover.Entity is Spikes spikes)
            {
                spikes.EnabledColor = this.color;
                spikes.DisabledColor = disabledColor;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(this.color);
            }
            if (staticMover.Entity is Spring spring)
            {
                spring.DisabledColor = disabledColor;
                spring.VisibleWhenDisabled = true;
            }
        }
        if (group == null)
        {
            groupLeader = true;
            group = new List<TapeBlock>();
            group.Add(this);
            FindInGroup(this);
            float minLeft = float.MaxValue;
            float maxRight = float.MinValue;
            float minTop = float.MaxValue;
            float maxBottom = float.MinValue;
            foreach (TapeBlock item in group)
            {
                if (item.Left < minLeft)
                {
                    minLeft = item.Left;
                }
                if (item.Right > maxRight)
                {
                    maxRight = item.Right;
                }
                if (item.Bottom > maxBottom)
                {
                    maxBottom = item.Bottom;
                }
                if (item.Top < minTop)
                {
                    minTop = item.Top;
                }
            }
            groupOrigin = new Vector2((int)(minLeft + (maxRight - minLeft) / 2f), (int)maxBottom);
            wigglerScaler = new Vector2(Calc.ClampedMap(maxRight - minLeft, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(maxBottom - minTop, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (TapeBlock item2 in group)
            {
                item2.wiggler = wiggler;
                item2.wigglerScaler = wigglerScaler;
                item2.groupOrigin = groupOrigin;
            }
        }
        foreach (StaticMover staticMover2 in staticMovers)
        {
            if (staticMover2.Entity is Spikes spikes2)
            {
                spikes2.SetOrigins(groupOrigin);
            }
        }
        // Autotiling: for each 8x8 tile, check the four cardinal neighbors and
        // pick a subtexture based on which sides have a matching TapeBlock.
        for (float x = base.Left; x < base.Right; x += 8f)
        {
            for (float y = base.Top; y < base.Bottom; y += 8f)
            {
                bool hasLeft = CheckForSame(x - 8f, y);
                bool hasRight = CheckForSame(x + 8f, y);
                bool hasTop = CheckForSame(x, y - 8f);
                bool hasBottom = CheckForSame(x, y + 8f);
                if (hasLeft && hasRight && hasTop && hasBottom)
                {
                    // Fully surrounded - inspect diagonals to pick the inner corner.
                    if (!CheckForSame(x + 8f, y - 8f))
                    {
                        SetImage(x, y, 3, 0);
                    }
                    else if (!CheckForSame(x - 8f, y - 8f))
                    {
                        SetImage(x, y, 3, 1);
                    }
                    else if (!CheckForSame(x + 8f, y + 8f))
                    {
                        SetImage(x, y, 3, 2);
                    }
                    else if (!CheckForSame(x - 8f, y + 8f))
                    {
                        SetImage(x, y, 3, 3);
                    }
                    else
                    {
                        SetImage(x, y, 1, 1);
                    }
                }
                else if (hasLeft && hasRight && !hasTop && hasBottom)
                {
                    SetImage(x, y, 1, 0);
                }
                else if (hasLeft && hasRight && hasTop && !hasBottom)
                {
                    SetImage(x, y, 1, 2);
                }
                else if (hasLeft && !hasRight && hasTop && hasBottom)
                {
                    SetImage(x, y, 2, 1);
                }
                else if (!hasLeft && hasRight && hasTop && hasBottom)
                {
                    SetImage(x, y, 0, 1);
                }
                else if (hasLeft && !hasRight && !hasTop && hasBottom)
                {
                    SetImage(x, y, 2, 0);
                }
                else if (!hasLeft && hasRight && !hasTop && hasBottom)
                {
                    SetImage(x, y, 0, 0);
                }
                else if (hasLeft && !hasRight && hasTop && !hasBottom)
                {
                    SetImage(x, y, 2, 2);
                }
                else if (!hasLeft && hasRight && hasTop && !hasBottom)
                {
                    SetImage(x, y, 0, 2);
                }
            }
        }
        if (!Collidable)
        {
            DisableStaticMovers();
        }
        UpdateVisualState();
    }
    private void FindInGroup(TapeBlock block)
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            if (entity != this && entity != block && entity.Index == Index && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
            {
                group.Add(entity);
                FindInGroup(entity);
                entity.group = group;
            }
        }
    }

    private bool CheckForSame(float x, float y)
    {
        foreach (TapeBlock entity in base.Scene.Tracker.GetEntities<TapeBlock>())
        {
            if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
            {
                return true;
            }
        }
        return false;
    }

    private void SetImage(float x, float y, int tx, int ty)
    {
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/cassetteblock/pressed");
        pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
        solid.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/cassetteblock/solid"]));
    }

    private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
    {
        Vector2 vector = new Vector2(x - base.X, y - base.Y);
        Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
        Vector2 vector2 = groupOrigin - Position;
        image.Origin = vector2 - vector;
        image.Position = vector2;
        image.Color = color;
        Add(image);
        all.Add(image);
        return image;
    }

    public override void Update()
    {
        base.Update();
        if (groupLeader && Activated && !Collidable)
        {
            bool anyBlocked = false;
            foreach (TapeBlock item in group)
            {
                if (item.BlockedCheck())
                {
                    anyBlocked = true;
                    break;
                }
            }
            if (!anyBlocked)
            {
                foreach (TapeBlock item2 in group)
                {
                    item2.Collidable = true;
                    item2.EnableStaticMovers();
                    item2.ShiftSize(-1);
                }
                wiggler.Start();
            }
        }
        else if (!Activated && Collidable)
        {
            ShiftSize(1);
            Collidable = false;
            DisableStaticMovers();
        }
        UpdateVisualState();
    }

    public bool BlockedCheck()
    {
        MaddyCrystal maddyCrystal = CollideFirst<MaddyCrystal>();
        if (maddyCrystal != null && !TryActorWiggleUp(maddyCrystal))
        {
            return true;
        }
        global::Celeste.Player player = CollideFirst<global::Celeste.Player>();
        if (player != null && !TryActorWiggleUp(player))
        {
            return true;
        }
        return false;
    }

    private void UpdateVisualState()
    {
        if (!Collidable)
        {
            base.Depth = 8990;
        }
        else
        {
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null && entity.Top >= base.Bottom - 1f)
            {
                base.Depth = 10;
            }
            else
            {
                base.Depth = -10;
            }
        }
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = base.Depth + 1;
        }
        side.Depth = base.Depth + 5;
        side.Visible = blockHeight > 0;
        occluder.Visible = Collidable;
        foreach (Image item in solid)
        {
            item.Visible = Collidable;
        }
        foreach (Image item2 in pressed)
        {
            item2.Visible = !Collidable;
        }
        if (!groupLeader)
        {
            return;
        }
        Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
        foreach (TapeBlock item3 in group)
        {
            foreach (Image item4 in item3.all)
            {
                item4.Scale = scale;
            }
            foreach (StaticMover staticMover2 in item3.staticMovers)
            {
                if (!(staticMover2.Entity is Spikes spikes))
                {
                    continue;
                }
                foreach (Component component in spikes.Components)
                {
                    if (component is Image image)
                    {
                        image.Scale = scale;
                    }
                }
            }
        }
    }

    public void SetActivatedSilently(bool activated)
    {
        Activated = (Collidable = activated);
        UpdateVisualState();
        if (activated)
        {
            EnableStaticMovers();
            return;
        }
        ShiftSize(2);
        DisableStaticMovers();
    }

    public void Finish()
    {
        Activated = false;
    }

    public void WillToggle()
    {
        ShiftSize(Collidable ? 1 : (-1));
        UpdateVisualState();
    }

    private void ShiftSize(int amount)
    {
        MoveV(amount);
        blockHeight -= amount;
    }

    private bool TryActorWiggleUp(Entity actor)
    {
        foreach (TapeBlock item in group)
        {
            if (item != this && item.CollideCheck(actor, item.Position + Vector2.UnitY * 4f))
            {
                return false;
            }
        }
        bool collidable = Collidable;
        Collidable = true;
        for (int i = 1; i <= 4; i++)
        {
            if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
            {
                actor.Position -= Vector2.UnitY * i;
                Collidable = collidable;
                return true;
            }
        }
        Collidable = collidable;
        return false;
    }

    public TapeBlock(EntityData data, Vector2 offset)
        : this(data, offset, new EntityID(data.Level.Name, data.ID))
    {
    }
}




