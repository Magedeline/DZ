using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;
using CharaMirror = Celeste.Entities.CharaMirror;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS02_CharaMirror : CutsceneEntity
    {
        private Player player;
        private CharaMirror charaMirror;
        private float playerEndX;
        private int direction = 1;
        private SoundSource sfx;

        public CS02_CharaMirror(Player player, CharaMirror charaMirror)
        {
            this.player = player;
            this.charaMirror = charaMirror;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS02_CharaMirror cs02CharaMirror = this;
            cs02CharaMirror.Add(cs02CharaMirror.sfx = new SoundSource());
            cs02CharaMirror.sfx.Position = cs02CharaMirror.charaMirror.Center;
            cs02CharaMirror.sfx.Play("event:/music/pusheen/lvl2/dreamblock_sting_pt1");
            cs02CharaMirror.direction = Math.Sign(cs02CharaMirror.player.X - cs02CharaMirror.charaMirror.X);
            cs02CharaMirror.player.StateMachine.State = Player.StDummy;
            cs02CharaMirror.playerEndX = 8 * cs02CharaMirror.direction;
            yield return 1f;
            cs02CharaMirror.player.Facing = (Facings) (-cs02CharaMirror.direction);
            yield return 0.4f;
            yield return cs02CharaMirror.DummyDashTo(cs02CharaMirror.charaMirror.X + cs02CharaMirror.playerEndX);
            yield return 0.5f;
            yield return level.ZoomTo(cs02CharaMirror.charaMirror.Position - level.Camera.Position - Vector2.UnitY * 24f, 2f, 1f);
            yield return 0.5f;
            yield return cs02CharaMirror.charaMirror.BreakRoutine(cs02CharaMirror.direction);
            cs02CharaMirror.player.DummyAutoAnimate = false;
            cs02CharaMirror.player.Sprite.Play("lookUp");
            Vector2 from = level.Camera.Position;
            Vector2 to = level.Camera.Position + new Vector2(0.0f, -80f);
            for (float ease = 0.0f; ease < 1.0; ease += Engine.DeltaTime * 1.2f)
            {
                level.Camera.Position = from + (to - from) * Ease.CubeInOut(ease);
                yield return null;
            }
            cs02CharaMirror.Add(new Coroutine(cs02CharaMirror.ZoomBack()));
            List<Entity>.Enumerator enumerator = cs02CharaMirror.Scene.Tracker.GetEntities<NightmareBlock>().GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                    yield return ((NightmareBlock) enumerator.Current).Activate();
            }
            finally
            {
                enumerator.Dispose();
            }
            enumerator = new List<Entity>.Enumerator();
            from = new Vector2();
            to = new Vector2();
            yield return 0.5f;
            cs02CharaMirror.EndCutscene(level);
        }

        private IEnumerator ZoomBack()
        {
            CS02_CharaMirror cs02CharaMirror = this;
            yield return 1.2f;
            yield return cs02CharaMirror.Level.ZoomBack(3f);
        }

        private IEnumerator DummyDashTo(float targetX)
        {
            float dashSpeed = 240f;
            int dashDirection = Math.Sign(targetX - player.X);
            
            player.Facing = (Facings) dashDirection;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("dash");
            Audio.Play("event:/char/pusheen/kirby/dash_pink_right", player.Position);
            
            player.Hair.Color = Player.UsedHairColor;
            
            while (Math.Abs(player.X - targetX) > 4f)
            {
                player.Speed.X = dashDirection * dashSpeed;
                yield return null;
            }
            
            player.X = targetX;
            player.Speed.X = 0f;
            player.DummyAutoAnimate = true;
            player.Sprite.Play("idle");
            player.Hair.Color = Player.NormalHairColor;
        }

        public override void OnEnd(Level level)
        {
            charaMirror.Broken(WasSkipped);
            if (WasSkipped)
                SceneAs<Level>().ParticlesFG.Clear();
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            if (entity1 != null)
            {
                entity1.StateMachine.State = Player.StNormal;
                entity1.DummyAutoAnimate = true;
                entity1.Speed = Vector2.Zero;
                entity1.X = charaMirror.X + playerEndX;
                entity1.Facing = direction == 0 ? Facings.Right : (Facings) (-direction);
            }
            foreach (NightmareBlock entity2 in Scene.Tracker.GetEntities<NightmareBlock>())
                entity2.ActivateNoRoutine();
            level.ResetZoom();
            level.Session.Inventory.DreamDash = true;
            level.Session.Audio.Music.Event = "event:/music/pusheen/lvl2/mirror";
            level.Session.Audio.Apply();
        }
    }
}

