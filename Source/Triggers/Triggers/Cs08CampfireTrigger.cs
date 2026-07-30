using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    [CustomEntity("DZ/Cs08CampfireTrigger")]
    [Tracked]
    public class Cs08CampfireTrigger : Trigger
    {
        private bool triggerOnce;
        private bool hasTriggered;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Cs08CampfireTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            hasTriggered = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!player.Collidable)
            {
                return;
            }

            if (triggerOnce && hasTriggered)
            {
                return;
            }

            if (base.Scene is not Level level)
            {
                return;
            }

            if (level.Session.GetFlag(Cs08Campfire.FLAG) || level.Session.GetFlag(CS08_StarJumpEnd.Flag))
            {
                return;
            }

            Npc08MadelinePlateau npc = base.Scene.Tracker.GetEntity<Npc08MadelinePlateau>();
            if (npc == null)
            {
                return;
            }

            hasTriggered = true;
            player.StateMachine.State = global::Celeste.Player.StDummy;
            level.StartCutscene(OnCutsceneEnd);
            base.Scene.Add(new Cs08Campfire(npc, player, npc));
        }

        private void OnCutsceneEnd(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }
    }
}
