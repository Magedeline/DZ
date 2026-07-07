using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;
using DZ;

namespace Celeste.Entities;

    [CustomEntity("DZ/Bridge", "MaggyHelper/Bridge")]
    [Monocle.Tracked]
    [HotReloadable]


public class Bridge : Entity
{
    private List<DZBridgeTile> tiles;

    private Level level;

    private bool canCollapse;

    private bool canEndCollapseA;

    private bool canEndCollapseB;

    private float collapseTimer;

    private int width;

    private List<Rectangle> tileSizes = new List<Rectangle>();

    private bool ending;

    private float gapStartX;

    private float gapEndX;

    private SoundSource collapseSfx;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Bridge(Vector2 position, int width, float gapStartX, float gapEndX)
        : base(position)
    {
        this.width = width;
        this.gapStartX = gapStartX;
        this.gapEndX = gapEndX;
        tileSizes.Add(new Rectangle(0, 0, 16, 52));
        tileSizes.Add(new Rectangle(16, 0, 8, 52));
        tileSizes.Add(new Rectangle(24, 0, 8, 52));
        tileSizes.Add(new Rectangle(32, 0, 8, 52));
        tileSizes.Add(new Rectangle(40, 0, 8, 52));
        tileSizes.Add(new Rectangle(48, 0, 8, 52));
        tileSizes.Add(new Rectangle(56, 0, 8, 52));
        tileSizes.Add(new Rectangle(64, 0, 8, 52));
        tileSizes.Add(new Rectangle(72, 0, 8, 52));
        tileSizes.Add(new Rectangle(80, 0, 16, 52));
        tileSizes.Add(new Rectangle(96, 0, 8, 52));
        Add(collapseSfx = new SoundSource());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Bridge(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Nodes[0].X, data.Nodes[1].X)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        tiles = new List<DZBridgeTile>();
        gapStartX += level.Bounds.Left;
        gapEndX += level.Bounds.Left;
        Calc.PushRandom(1);
        Vector2 position = Position;
        int num = 0;
        while (position.X < base.X + (float)width)
        {
            Rectangle tileSize = ((num < 2 || num > 7) ? tileSizes[num] : tileSizes[2 + Calc.Random.Next(6)]);
            if (position.X < gapStartX || position.X >= gapEndX)
            {
                DZBridgeTile bridgeTile = new DZBridgeTile(position, tileSize);
                tiles.Add(bridgeTile);
                level.Add(bridgeTile);
            }
            position.X += tileSize.Width;
            num = (num + 1) % tileSizes.Count;
        }
        Calc.PopRandom();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        Player entity = level.Tracker.GetEntity<Player>();
        if (entity == null || entity.Dead)
        {
            collapseSfx.Stop();
        }
        if (!canCollapse)
        {
            if (entity != null && entity.X >= base.X + 112f)
            {
                Audio.SetMusic("event:/pusheen/music/lvl0/bridge");
                collapseSfx.Play("event:/pusheen/game/00_prologue/bridge_rumble_loop");
                canCollapse = true;
                canEndCollapseA = true;
                canEndCollapseB = true;
                for (int i = 0; i < 11; i++)
                {
                    tiles[0].Fall(Calc.Random.Range(0.1f, 0.5f));
                    tiles.RemoveAt(0);
                }
            }
        }
        else if (tiles.Count > 0)
        {
            if (entity == null)
            {
                return;
            }
            if (canEndCollapseA && entity.X > base.X + (float)width - 216f)
            {
                canEndCollapseA = false;
                for (int j = 0; j < 5; j++)
                {
                    tiles[tiles.Count - 8].Fall(Calc.Random.Range(0.1f, 0.5f));
                    tiles.RemoveAt(tiles.Count - 8);
                }
            }
            else if (canEndCollapseB && entity.X > base.X + (float)width - 104f)
            {
                canEndCollapseB = false;
                for (int k = 0; k < 7; k++)
                {
                    if (tiles.Count <= 0)
                    {
                        break;
                    }
                    tiles[tiles.Count - 1].Fall(Calc.Random.Range(0.1f, 0.3f));
                    tiles.RemoveAt(tiles.Count - 1);
                }
            }
            else if (collapseTimer > 0f)
            {
                collapseTimer -= Engine.DeltaTime;
                if (tiles.Count >= 5 && entity.X >= tiles[4].X)
                {
                    int index = 0;
                    tiles[index].Fall();
                    tiles.RemoveAt(index);
                }
            }
            else
            {
                tiles[0].Fall();
                tiles.RemoveAt(0);
                collapseTimer = 0.2f;
            }
        }
        else if (!ending)
        {
            ending = true;
            StopCollapseLoop();
        }
    }

    public void StopCollapseLoop()
    {
        collapseSfx.Stop();
    }
}