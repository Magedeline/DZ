using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    // =============================================
    // CompanionNPC - Follower NPC that helps
    // =============================================
    [CustomEntity("MaggyHelper/CompanionNPC")]
    [Tracked]
    public class CompanionNPC : Entity
    {
        private string companionType;
        private float followSpeed;
        private float followDistance;
        private bool active = false;
        private bool canPressSwitch;
        private string spritePath;

        public CompanionNPC(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            companionType = data.Attr("companionType", "waddle_dee");
            followSpeed = data.Float("followSpeed", 100f);
            followDistance = data.Float("followDistance", 30f);
            canPressSwitch = data.Bool("canPressSwitch", true);
            spritePath = data.Attr("sprite", "");

            Collider = new Hitbox(10f, 12f, -5f, -12f);
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();
            if (!active)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && CollideCheck(player) && Input.Talk.Pressed)
                {
                    active = true;
                    SceneAs<Level>().Session.SetFlag("companion_active", true);
                    Audio.Play("event:/game/general/seed_touch", Position);
                }
                return;
            }

            FollowPlayer();
        }

        private void FollowPlayer()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;

            float dist = Vector2.Distance(Position, player.Position);
            if (dist > followDistance)
            {
                Vector2 dir = (player.Position - Position).SafeNormalize();
                Position += dir * followSpeed * Engine.DeltaTime;
            }

            // Press weight switches
            if (canPressSwitch)
            {
                foreach (WeightSwitch ws in Scene.Tracker.GetEntities<WeightSwitch>())
                {
                    if (CollideCheck(ws))
                    {
                        // Companion is considered weight
                    }
                }
            }
        }

        public void Dismiss()
        {
            active = false;
            SceneAs<Level>().Session.SetFlag("companion_active", false);
        }

        public override void Render()
        {
            Color c = companionType switch
            {
                "waddle_dee" => Color.Orange,
                "badeline" => Color.MediumPurple,
                "kirby" => Color.Pink,
                "ralsei" => Color.LimeGreen,
                _ => Color.White
            };

            Draw.Rect(X - 5f, Y - 12f, 10f, 12f, c * 0.7f);
            Draw.Rect(X - 3f, Y - 10f, 2f, 2f, Color.Black); // eye
            Draw.Rect(X + 1f, Y - 10f, 2f, 2f, Color.Black); // eye

            if (!active)
            {
                // Talk indicator
                float bob = (float)Math.Sin(Scene.TimeActive * 3f) * 2f;
                Draw.Rect(X - 1f, Y - 18f + bob, 2f, 4f, Color.White * 0.5f);
            }
        }
    }

    // =============================================
    // ShopKeeper - Trades coins for power-ups
    // =============================================
    [CustomEntity("MaggyHelper/ShopKeeper")]
    [Tracked]
    public class ShopKeeper : Entity
    {
        private string shopId;
        private int itemCost;
        private string itemReward; // flag to set
        private string dialogId;
        private bool sold = false;

        public ShopKeeper(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            shopId = data.Attr("shopId", "shop_1");
            itemCost = data.Int("itemCost", 5);
            itemReward = data.Attr("itemReward", "powerup_speed");
            dialogId = data.Attr("dialogId", "SHOP_DEFAULT");

            Collider = new Hitbox(16f, 24f, -8f, -24f);
            Depth = -100;
            Add(new VertexLight(Color.Goldenrod, 0.5f, 16, 48));
        }

        public override void Update()
        {
            base.Update();
            if (sold) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && Input.Talk.Pressed)
            {
                TryBuy(player);
            }
        }

        private void TryBuy(Player player)
        {
            Level level = SceneAs<Level>();
            // Count coins collected via session flags
            int coins = 0;
            for (int i = 0; i < 100; i++)
            {
                if (level.Session.GetFlag("coin_" + i)) coins++;
            }

            if (coins >= itemCost)
            {
                // Deduct coins
                int deducted = 0;
                for (int i = 0; i < 100 && deducted < itemCost; i++)
                {
                    if (level.Session.GetFlag("coin_" + i))
                    {
                        level.Session.SetFlag("coin_" + i, false);
                        deducted++;
                    }
                }

                level.Session.SetFlag(itemReward, true);
                sold = true;
                Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
                Scene.Add(new MiniTextbox(dialogId + "_BUY"));
            }
            else
            {
                Audio.Play("event:/game/general/assist_screenbottom", Position);
                Scene.Add(new MiniTextbox(dialogId + "_CANT_AFFORD"));
            }
        }

        public override void Render()
        {
            Draw.Rect(X - 8f, Y - 24f, 16f, 24f, Color.SaddleBrown * 0.7f);
            Draw.Rect(X - 6f, Y - 22f, 4f, 3f, Color.Black); // eye
            Draw.Rect(X + 2f, Y - 22f, 4f, 3f, Color.Black); // eye
            Draw.Rect(X - 4f, Y - 16f, 8f, 2f, Color.Black); // mouth

            // Price tag
            if (!sold)
            {
                float bob = (float)Math.Sin(Scene.TimeActive * 2f) * 2f;
                Draw.Rect(X - 8f, Y - 34f + bob, 16f, 8f, Color.Gold * 0.5f);
            }
        }
    }

    // =============================================
    // QuestGiver - Assigns fetch/defeat quests
    // =============================================
    [CustomEntity("MaggyHelper/QuestGiver")]
    [Tracked]
    public class QuestGiver : Entity
    {
        private string questId;
        private string questType; // "collect", "defeat", "reach"
        private string targetFlag;
        private int targetCount;
        private string dialogBefore;
        private string dialogAfter;
        private string rewardFlag;
        private bool questComplete = false;

        public QuestGiver(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            questId = data.Attr("questId", "quest_1");
            questType = data.Attr("questType", "collect");
            targetFlag = data.Attr("targetFlag", "quest_item");
            targetCount = data.Int("targetCount", 3);
            dialogBefore = data.Attr("dialogBefore", "QUEST_START");
            dialogAfter = data.Attr("dialogAfter", "QUEST_COMPLETE");
            rewardFlag = data.Attr("rewardFlag", "quest_1_complete");

            Collider = new Hitbox(16f, 24f, -8f, -24f);
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || !CollideCheck(player) || !Input.Talk.Pressed) return;

            Level level = SceneAs<Level>();
            if (questComplete || level.Session.GetFlag(rewardFlag))
            {
                Scene.Add(new MiniTextbox(dialogAfter));
                return;
            }

            // Check quest progress
            int progress = 0;
            for (int i = 0; i < targetCount; i++)
            {
                if (level.Session.GetFlag(targetFlag + "_" + i)) progress++;
            }

            if (progress >= targetCount)
            {
                questComplete = true;
                level.Session.SetFlag(rewardFlag, true);
                Scene.Add(new MiniTextbox(dialogAfter));
                Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
            }
            else
            {
                Scene.Add(new MiniTextbox(dialogBefore));
            }
        }

        public override void Render()
        {
            Draw.Rect(X - 8f, Y - 24f, 16f, 24f, Color.RoyalBlue * 0.7f);
            // Quest marker
            if (!questComplete)
            {
                float bob = (float)Math.Sin(Scene.TimeActive * 3f) * 3f;
                Draw.Rect(X - 2f, Y - 34f + bob, 4f, 8f, Color.Yellow * 0.8f); // !
                Draw.Rect(X - 2f, Y - 24f + bob, 4f, 4f, Color.Yellow * 0.8f); // . of !
            }
            else
            {
                Draw.Rect(X - 4f, Y - 34f, 8f, 8f, Color.LimeGreen * 0.8f); // checkmark
            }
        }
    }

    // =============================================
    // TrainingDummy - Shows damage numbers
    // =============================================
    [CustomEntity("MaggyHelper/TrainingDummy")]
    [Tracked]
    public class TrainingDummy : Enemy
    {
        private List<DamageNumber> damageNumbers = new List<DamageNumber>();
        private int totalDamage = 0;

        private struct DamageNumber
        {
            public Vector2 Position;
            public int Amount;
            public float Timer;
        }

        public TrainingDummy(EntityData data, Vector2 offset)
            : base(data.Position + offset, 9999)
        {
            Collider = new Hitbox(16f, 24f, -8f, -24f);
        }

        public override void TakeDamage(int damage)
        {
            Health -= damage;
            totalDamage += damage;
            Health = MaxHealth; // Don't actually die

            damageNumbers.Add(new DamageNumber
            {
                Position = Position + new Vector2(Calc.Random.Range(-10f, 10f), -28f),
                Amount = damage,
                Timer = 1f
            });

            InvincibilityTimer = 0.1f;
            Audio.Play("event:/game/general/spring", Position);
        }

        protected override void Die() { /* Never dies */ }

        public override void Update()
        {
            base.Update();
            for (int i = damageNumbers.Count - 1; i >= 0; i--)
            {
                var dn = damageNumbers[i];
                dn.Timer -= Engine.DeltaTime;
                dn.Position.Y -= 20f * Engine.DeltaTime;
                damageNumbers[i] = dn;
                if (dn.Timer <= 0) damageNumbers.RemoveAt(i);
            }
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(X - 8f, Y - 24f, 16f, 24f, Color.Tan * 0.7f);
            Draw.Rect(X - 2f, Y - 20f, 4f, 4f, Color.Red * 0.5f); // target

            foreach (var dn in damageNumbers)
            {
                float alpha = Math.Max(dn.Timer, 0);
                Draw.Rect(dn.Position.X - 4f, dn.Position.Y - 4f, 8f, 8f, Color.Red * alpha);
            }
        }
    }

    // =============================================
    // GhostReplay - Shows previous run ghost
    // =============================================
    [CustomEntity("MaggyHelper/GhostReplay")]
    [Tracked]
    public class GhostReplay : Entity
    {
        private List<Vector2> recordedPath = new List<Vector2>();
        private int playbackIndex = 0;
        private bool recording = true;
        private float recordInterval = 0.05f;
        private float timer = 0f;
        private string ghostId;
        private Color ghostColor;

        public GhostReplay(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            ghostId = data.Attr("ghostId", "ghost_1");
            ghostColor = Calc.HexToColor(data.Attr("color", "ffffff"));
            Depth = 500;
        }

        public override void Update()
        {
            base.Update();
            timer -= Engine.DeltaTime;
            if (timer > 0) return;
            timer = recordInterval;

            if (recording)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    recordedPath.Add(player.Position);
                }
            }
            else
            {
                if (playbackIndex < recordedPath.Count)
                {
                    Position = recordedPath[playbackIndex];
                    playbackIndex++;
                }
                else
                {
                    playbackIndex = 0; // Loop
                }
            }
        }

        public void StartPlayback()
        {
            recording = false;
            playbackIndex = 0;
        }

        public override void Render()
        {
            if (!recording && playbackIndex > 0 && playbackIndex < recordedPath.Count)
            {
                Draw.Rect(Position.X - 4f, Position.Y - 11f, 8f, 11f, ghostColor * 0.3f);
            }
        }
    }
}

