using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS07_SaveMaddy : CutsceneEntity
    {
        public const string Flag = "foundMaddyInCrystal";
        private Player player;
        private MaddyCrystal maddy;
        private Vector2 playerEndPosition;
        private bool wasDashAssistOn;

        public CS07_SaveMaddy(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            maddy = level.Tracker.GetEntity<MaddyCrystal>();
            playerEndPosition = maddy.Position + new Vector2(-24f, 0.0f);
            wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS07_SaveMaddy cs05SaveTheo = this;
            cs05SaveTheo.player.StateMachine.State = 11;
            cs05SaveTheo.player.StateMachine.Locked = true;
            cs05SaveTheo.player.ForceCameraUpdate = true;
            level.Session.Audio.Music.Layer(6, 0.0f);
            level.Session.Audio.Apply();
            yield return cs05SaveTheo.player.DummyWalkTo(cs05SaveTheo.maddy.X - 18f);
            cs05SaveTheo.player.Facing = Facings.Right;
            yield return Textbox.Say("DZ_CH7_FOUND_MADDY", cs05SaveTheo.TryToBreakCrystal);
            yield return 0.25f;
            yield return cs05SaveTheo.Level.ZoomBack(0.5f);
            cs05SaveTheo.EndCutscene(level);
        }

        private IEnumerator TryToBreakCrystal()
        {
            CS07_SaveMaddy cs05SaveTheo = this;
            cs05SaveTheo.Scene.Entities.FindFirst<TheoCrystalPedestal>().Collidable = true;
            yield return cs05SaveTheo.player.DummyWalkTo(cs05SaveTheo.maddy.X);
            yield return 0.1f;
            yield return cs05SaveTheo.Level.ZoomTo(new Vector2(160f, 90f), 2f, 0.5f);
            cs05SaveTheo.player.DummyAutoAnimate = false;
            cs05SaveTheo.player.Sprite.Play("lookUp");
            yield return 1f;
            cs05SaveTheo.wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            SaveData.Instance.Assists.DashAssist = false;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            MInput.Disabled = true;
            cs05SaveTheo.player.OverrideDashDirection = new Vector2(0.0f, -1f);
            cs05SaveTheo.player.StateMachine.Locked = false;
            cs05SaveTheo.player.StateMachine.State = cs05SaveTheo.player.StartDash();
            cs05SaveTheo.player.Dashes = 0;
            yield return 0.1f;
            while (!cs05SaveTheo.player.OnGround() || cs05SaveTheo.player.Speed.Y < 0.0)
            {
                cs05SaveTheo.player.Dashes = 0;
                Input.MoveY.Value = -1;
                Input.MoveX.Value = 0;
                yield return null;
            }
            cs05SaveTheo.player.OverrideDashDirection = new Vector2?();
            cs05SaveTheo.player.StateMachine.State = 11;
            cs05SaveTheo.player.StateMachine.Locked = true;
            MInput.Disabled = false;
            cs05SaveTheo.player.DummyAutoAnimate = true;
            yield return cs05SaveTheo.player.DummyWalkToExact((int)cs05SaveTheo.playerEndPosition.X, true);
            yield return 1.5f;
        }

        public override void OnEnd(Level level)
        {
            SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
            player.Position = playerEndPosition;
            while (!player.OnGround())
                player.MoveV(1f);
            level.Camera.Position = player.CameraTarget;
            level.Session.SetFlag("foundMaddyInCrystal");
            level.ResetZoom();
            level.Session.Audio.Music.Layer(6, 1f);
            level.Session.Audio.Apply();
            List<Follower> followerList = new List<Follower>(player.Leader.Followers);
            player.RemoveSelf();
            level.Add(player = new Player(player.Position, player.DefaultSpriteMode));
            foreach (Follower follower in followerList)
            {
                player.Leader.Followers.Add(follower);
                follower.Leader = player.Leader;
            }
            player.Facing = Facings.Right;
            player.IntroType = Player.IntroTypes.None;
            MaddyCrystalPedestal first = Scene.Entities.FindFirst<MaddyCrystalPedestal>();
            first.Collidable = false;
            first.DroppedMaddy = true;
            maddy.Depth = 100;
            maddy.OnPedestal = false;
            maddy.Speed = Vector2.Zero;
            while (!maddy.OnGround())
                maddy.MoveV(1f);
        }
    }
}