// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_Gravestone
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;
using BirdNPC = Celeste.Entities.BirdNPC;
using NPC19_Gravestone = Celeste.NPCs.NPC19_Gravestone;

namespace Celeste.Cutscenes
{
    public class CS19_Gravestone : CutsceneEntity
    {
        private const string Flag = "maddy_gravestone";

        private Player player;
        private NPC19_Gravestone gravestone;
        private CharaDummy chara;

        private BirdNPC bird;
        private BirdNPC birdClover;
        private BirdNPC birdCody;
        private BirdNPC birdEmily;
        private BirdNPC birdOdin;
        private BirdNPC birdRobin;
        private BirdNPC birdSabel;

        private CharaDummy undyne;
        private CharaDummy toriel;
        private CharaDummy theo;
        private CharaDummy asgore;
        private CharaDummy starsi;
        private CharaDummy ralsei;
        private CharaDummy sans;
        private CharaDummy papyrus;
        private CharaDummy alphy;
        private CharaDummy noelle;
        private CharaDummy suzy;
        private CharaDummy berdly;

        private Vector2 boostTarget;
        private bool addedBooster;

        private static readonly Color[] SoulColors =
        {
            Calc.HexToColor("ff0000"),
            Calc.HexToColor("ff8000"),
            Calc.HexToColor("ffff00"),
            Calc.HexToColor("00ff00"),
            Calc.HexToColor("00ffff"),
            Calc.HexToColor("0000ff"),
            Calc.HexToColor("ff00ff")
        };

        public CS19_Gravestone(Player player, NPC19_Gravestone gravestone, Vector2 boostTarget)
        {
            this.player = player;
            this.gravestone = gravestone;
            this.boostTarget = boostTarget;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene()));
        }

        private IEnumerator cutscene()
        {
            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;
            player.DummyGravity = false;
            player.Speed.Y = 0.0f;
            yield return 0.1f;

            yield return player.DummyWalkToExact((int)gravestone.X - 30);
            yield return 0.1f;
            player.Facing = Facings.Right;
            yield return 0.2f;

            yield return Level.ZoomTo(new Vector2(160f, 90f), 2f, 3f);
            player.ForceCameraUpdate = false;

            InitializeDummyCharacters();

            yield return 0.5f;

            yield return Textbox.Say("CH19_GRAVESTONE_SEVEN_BIRDS",
                new Func<IEnumerator>(stepForward),
                new Func<IEnumerator>(KirbyCollapsesToKnees),
                new Func<IEnumerator>(DarkEnergyBegins),
                new Func<IEnumerator>(charaAppears),
                new Func<IEnumerator>(EveryoneArrives),
                new Func<IEnumerator>(SevenBirdsFlyDown),
                new Func<IEnumerator>(ReactToSevenBirds),
                new Func<IEnumerator>(BirdsFlyUpCharaBoost));

            yield return 1f;
            yield return Level.ZoomBack(0.5f);
            yield return 0.3f;

            EndCutscene(Level);
        }

        private void InitializeDummyCharacters()
        {
            float baseX = player.Position.X;
            float baseY = player.Position.Y;

            undyne = new CharaDummy(new Vector2(baseX - 100f, baseY));
            toriel = new CharaDummy(new Vector2(baseX - 90f, baseY));
            theo = new CharaDummy(new Vector2(baseX - 80f, baseY));
            asgore = new CharaDummy(new Vector2(baseX - 70f, baseY));
            starsi = new CharaDummy(new Vector2(baseX - 60f, baseY));
            ralsei = new CharaDummy(new Vector2(baseX - 50f, baseY));
            sans = new CharaDummy(new Vector2(baseX - 40f, baseY));
            papyrus = new CharaDummy(new Vector2(baseX - 30f, baseY));
            alphy = new CharaDummy(new Vector2(baseX - 10f, baseY));
            noelle = new CharaDummy(new Vector2(baseX + 10f, baseY));
            suzy = new CharaDummy(new Vector2(baseX + 20f, baseY));
            berdly = new CharaDummy(new Vector2(baseX + 30f, baseY));

            undyne.Visible = false;
            toriel.Visible = false;
            theo.Visible = false;
            asgore.Visible = false;
            starsi.Visible = false;
            ralsei.Visible = false;
            sans.Visible = false;
            papyrus.Visible = false;
            alphy.Visible = false;
            noelle.Visible = false;
            suzy.Visible = false;
            berdly.Visible = false;

            Level.Add(undyne);
            Level.Add(toriel);
            Level.Add(theo);
            Level.Add(asgore);
            Level.Add(starsi);
            Level.Add(ralsei);
            Level.Add(sans);
            Level.Add(papyrus);
            Level.Add(alphy);
            Level.Add(noelle);
            Level.Add(suzy);
            Level.Add(berdly);
        }

        private IEnumerator stepForward()
        {
            yield return player.DummyWalkTo(player.X + 8f);
        }

        private IEnumerator KirbyCollapsesToKnees()
        {
            yield return 0.3f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("duck");
            Audio.Play("event:/pusheen/char/kirby/jump_superslide", player.Position);
            yield return 0.5f;
        }

        private IEnumerator DarkEnergyBegins()
        {
            for (int i = 0; i < 3; i++)
            {
                Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 64f, 0.5f);
                Audio.Play("event:/pusheen/final_content/game/19_the_end/lightning_strike", player.Position);
                yield return 0.3f;
            }
            yield return 0.2f;
        }

        private IEnumerator charaAppears()
        {
            Level.Session.Inventory.Dashes = 1;
            player.Dashes = 1;
            Vector2 position = player.Position + new Vector2(-20f, -10f);
            Level.Displacement.AddBurst(position, 0.5f, 8f, 32f, 0.5f);
            Level.Add(chara = new CharaDummy(position));
            Audio.Play("event:/char/badeline/maddy_split", position);
            chara.Sprite.Scale.X = 1f;
            yield return 0.3f;
        }

        private IEnumerator EveryoneArrives()
        {
            undyne.Visible = true;
            toriel.Visible = true;
            theo.Visible = true;
            asgore.Visible = true;
            starsi.Visible = true;
            ralsei.Visible = true;
            sans.Visible = true;
            papyrus.Visible = true;
            alphy.Visible = true;
            noelle.Visible = true;
            suzy.Visible = true;
            berdly.Visible = true;

            Audio.Play("event:/game/general/world_noise", player.Position);

            float targetX = gravestone.X - 60f;
            Add(new Coroutine(WalkCharacterTo(undyne, targetX - 50f)));
            Add(new Coroutine(WalkCharacterTo(toriel, targetX - 40f)));
            Add(new Coroutine(WalkCharacterTo(theo, targetX - 30f)));
            Add(new Coroutine(WalkCharacterTo(asgore, targetX - 20f)));
            Add(new Coroutine(WalkCharacterTo(starsi, targetX - 10f)));
            Add(new Coroutine(WalkCharacterTo(ralsei, targetX)));
            Add(new Coroutine(WalkCharacterTo(sans, targetX + 10f)));
            Add(new Coroutine(WalkCharacterTo(papyrus, targetX + 20f)));
            Add(new Coroutine(WalkCharacterTo(alphy, targetX + 40f)));
            Add(new Coroutine(WalkCharacterTo(noelle, targetX + 50f)));
            Add(new Coroutine(WalkCharacterTo(suzy, targetX + 60f)));
            Add(new Coroutine(WalkCharacterTo(berdly, targetX + 70f)));

            yield return 2f;
        }

        private IEnumerator WalkCharacterTo(CharaDummy character, float x)
        {
            if (character != null)
            {
                yield return character.WalkTo(x);
            }
        }

        private IEnumerator SevenBirdsFlyDown()
        {
            Vector2 graveCenter = gravestone.Position + new Vector2(0f, -16f);

            bird = CreateBirdNPC(graveCenter + new Vector2(0f, -200f), "bird");
            birdClover = CreateBirdNPC(graveCenter + new Vector2(20f, -220f), "birdgoner_clover");
            birdCody = CreateBirdNPC(graveCenter + new Vector2(-20f, -210f), "birdgoner_cody");
            birdEmily = CreateBirdNPC(graveCenter + new Vector2(40f, -190f), "birdgoner_emily");
            birdOdin = CreateBirdNPC(graveCenter + new Vector2(-40f, -230f), "birdgoner_odin");
            birdRobin = CreateBirdNPC(graveCenter + new Vector2(60f, -215f), "birdgoner_robin");
            birdSabel = CreateBirdNPC(graveCenter + new Vector2(-60f, -205f), "birdgoner_sabel");

            Level.Add(bird);
            Level.Add(birdClover);
            Level.Add(birdCody);
            Level.Add(birdEmily);
            Level.Add(birdOdin);
            Level.Add(birdRobin);
            Level.Add(birdSabel);

            Add(new Coroutine(FlyBirdToPosition(bird, graveCenter, 0f)));
            Add(new Coroutine(FlyBirdToPosition(birdClover, graveCenter + new Vector2(15f, 2f), 0.2f)));
            Add(new Coroutine(FlyBirdToPosition(birdCody, graveCenter + new Vector2(-15f, 2f), 0.4f)));
            Add(new Coroutine(FlyBirdToPosition(birdEmily, graveCenter + new Vector2(30f, 4f), 0.6f)));
            Add(new Coroutine(FlyBirdToPosition(birdOdin, graveCenter + new Vector2(-30f, 4f), 0.8f)));
            Add(new Coroutine(FlyBirdToPosition(birdRobin, graveCenter + new Vector2(45f, 6f), 1.0f)));
            Add(new Coroutine(FlyBirdToPosition(birdSabel, graveCenter + new Vector2(-45f, 6f), 1.2f)));

            yield return 3f;

            Audio.Play("event:/game/general/bird_squawk", graveCenter);
            yield return 0.5f;
        }

        private BirdNPC CreateBirdNPC(Vector2 position, string spriteId)
        {
            var birdNpc = new BirdNPC(position, BirdNPC.Modes.None);
            Sprite oldSprite = birdNpc.Sprite;
            Action<string> onFrameChange = oldSprite.OnFrameChange;
            oldSprite.RemoveSelf();

            Sprite customSprite = GFX.SpriteBank.Create(spriteId);
            customSprite.UseRawDeltaTime = true;
            customSprite.OnFrameChange = onFrameChange;
            birdNpc.Add(customSprite);
            birdNpc.Sprite = customSprite;
            birdNpc.DisableFlapSfx = true;
            birdNpc.Facing = Facings.Left;
            birdNpc.Sprite.Play("fall");
            return birdNpc;
        }

        private IEnumerator FlyBirdToPosition(BirdNPC targetBird, Vector2 destination, float delay)
        {
            if (delay > 0f)
            {
                yield return delay;
            }

            Vector2 from = targetBird.Position;
            float percent = 0f;
            while (percent < 1f)
            {
                targetBird.Position = from + (destination - from) * Ease.QuadOut(percent);
                if (percent > 0.5f)
                {
                    targetBird.Sprite.Play("fly");
                }
                percent += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            targetBird.Position = destination;
            targetBird.Sprite.Play("idle");
        }

        private IEnumerator ReactToSevenBirds()
        {
            yield return Textbox.Say("CH19_SEVEN_BIRDS_REACTION");
            yield return 0.5f;
        }

        private IEnumerator BirdsFlyUpCharaBoost()
        {
            Audio.Play("event:/game/06_reflection/badeline_boss_charge", gravestone.Position);
            yield return 0.3f;

            List<Coroutine> flyUpCoroutines = new List<Coroutine>();

            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(bird, 0)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdClover, 1)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdCody, 2)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdEmily, 3)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdOdin, 4)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdRobin, 5)));
            flyUpCoroutines.Add(new Coroutine(FlyBirdUp(birdSabel, 6)));

            foreach (var coroutine in flyUpCoroutines)
            {
                Add(coroutine);
            }

            yield return 1.5f;

            Level.Flash(Color.White * 0.5f);
            Audio.Play("event:/new_content/char/badeline/booster_first_appear", boostTarget);
            yield return 0.3f;

            addedBooster = true;
            Level.Displacement.AddBurst(boostTarget, 0.5f, 8f, 32f, 0.5f);
            Level.Add(new CustomCharaBoost(new Vector2[] { boostTarget }, false));

            yield return 0.5f;

            yield return CharaRejoin();
        }

        private IEnumerator FlyBirdUp(BirdNPC targetBird, int index)
        {
            if (targetBird == null)
            {
                yield break;
            }

            targetBird.Sprite.Play("fly");
            Audio.Play("event:/game/general/bird_squawk", targetBird.Position);

            Vector2 from = targetBird.Position;
            float angle = index * ((float)Math.PI * 2f / 7f);
            float radius = 30f;

            float timer = 0f;
            float duration = 2f;
            while (timer < duration)
            {
                float progress = timer / duration;
                float currentAngle = angle + progress * (float)Math.PI * 4f;
                float spiralRadius = radius * (1f - progress * 0.5f);
                float yOffset = -progress * 300f;

                Vector2 spiralOffset = new Vector2(
                    (float)Math.Cos(currentAngle) * spiralRadius,
                    yOffset
                );

                targetBird.Position = from + spiralOffset;

                if (timer % 0.1f < Engine.DeltaTime)
                {
                    Level.Particles.Emit(
                        CustomCharaBoost.P_Move,
                        1,
                        targetBird.Position,
                        Vector2.One * 4f,
                        SoulColors[index % SoulColors.Length],
                        (float)Math.PI * 0.5f
                    );
                }

                timer += Engine.DeltaTime;
                yield return null;
            }

            targetBird.RemoveSelf();
        }

        private IEnumerator CharaRejoin()
        {
            if (chara == null)
            {
                yield break;
            }

            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", chara.Position);
            Vector2 from = chara.Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.25f)
            {
                chara.Position = Vector2.Lerp(from, player.Position, Ease.CubeIn(p));
                yield return null;
            }
            Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
            Level.Session.Inventory.Dashes = 2;
            player.Dashes = 2;
            chara.RemoveSelf();
        }

        public override void OnEnd(Level level)
        {
            player.Facing = Facings.Right;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.StateMachine.State = Player.StNormal;
            Level.Session.Inventory.Dashes = 5;
            player.Dashes = 5;

            RemoveIfExists(chara);
            RemoveIfExists(bird);
            RemoveIfExists(birdClover);
            RemoveIfExists(birdCody);
            RemoveIfExists(birdEmily);
            RemoveIfExists(birdOdin);
            RemoveIfExists(birdRobin);
            RemoveIfExists(birdSabel);
            RemoveIfExists(undyne);
            RemoveIfExists(toriel);
            RemoveIfExists(theo);
            RemoveIfExists(asgore);
            RemoveIfExists(starsi);
            RemoveIfExists(ralsei);
            RemoveIfExists(sans);
            RemoveIfExists(papyrus);
            RemoveIfExists(alphy);
            RemoveIfExists(noelle);
            RemoveIfExists(suzy);
            RemoveIfExists(berdly);

            if (!addedBooster)
            {
                level.Add(new CustomCharaBoost(new Vector2[] { boostTarget }, false));
            }

            level.ResetZoom();
            level.Session.SetFlag(Flag, true);
        }

        private void RemoveIfExists(Entity entity)
        {
            if (entity != null && entity.Scene != null)
            {
                entity.RemoveSelf();
            }
        }
    }
}
