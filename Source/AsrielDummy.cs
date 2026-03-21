           using System;
            using MaggyHelper.Entities;
            using Microsoft.Xna.Framework;
            using Monocle;

            namespace MaggyHelper.Entities
            {
                /// <summary>
                /// Simple cutscene helper entity that only displays a sprite from the SpriteBank.
                /// Used by AsrielGodBoss reveal cutscene to play "back"/"turn"/"idle" animations.
                /// </summary>
                [CustomEntity("MaggyHelper/AsrielDummy")]
                [Tracked]
                [HotReloadable]
                public sealed class AsrielDummy : Entity
                {
                    private readonly Sprite sprite;
                    public Sprite Sprite => sprite;
                    private readonly bool autoFacePlayer;
                    private int facing;

                    public AsrielDummy(EntityData data, Vector2 offset)
                        : this(
                            data.Position + offset,
                            data.Attr("sprite", "asrielgodboss"),
                            data.Attr("startAnim", "back"),
                            data.Int("facing", -1),
                            data.Bool("autoFacePlayer", false)
                        )
                    {
                        Depth = data.Int("depth", Depth);
                    }

                    public AsrielDummy(
                        Vector2 position,
                        string spriteId = "asrielgodboss",
                        string startAnim = "back",
                        int facing = -1,
                        bool autoFacePlayer = false)
                        : base(position)
                    {
                        this.autoFacePlayer = autoFacePlayer;
                        this.facing = facing == 0 ? -1 : Math.Sign(facing);

                        Depth = -10000;

                        try
                        {
                            sprite = GFX.SpriteBank.Create(spriteId);
                        }
                        catch
                        {
                            sprite = new Sprite(GFX.Game, "idle");
                        }

                        sprite.Visible = true;
                        sprite.Scale.X = this.facing;
                        sprite.Scale.Y = 1f;

                        if (!string.IsNullOrWhiteSpace(startAnim))
                        {
                            if (sprite.Has(startAnim))
                                sprite.Play(startAnim);
                            else if (sprite.Has("idle"))
                                sprite.Play("idle");
                        }

                        Add(sprite);
                    }

                    public override void Update()
                    {
                        base.Update();

                        if (autoFacePlayer && Scene != null)
                        {
                            Player player = Scene.Tracker.GetEntity<Player>();
                            if (player != null)
                            {
                                int dir = Math.Sign(player.X - X);
                                if (dir != 0)
                                    facing = dir;
                            }
                        }

                        sprite.Scale.X = facing;
                    }
                }
            }