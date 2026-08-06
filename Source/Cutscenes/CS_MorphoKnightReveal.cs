using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Wordless reveal cutscene triggered by collecting the purple (E-Side)
    /// heart gem: a silhouette materializes, unveils itself as Morpho Knight
    /// Delta, then the boss is spawned and the fight begins immediately.
    /// </summary>
    [HotReloadable]
    public class CS_MorphoKnightReveal : CutsceneEntity
    {
        private readonly global::Celeste.Player player;
        private Sprite reveal;

        public CS_MorphoKnightReveal(global::Celeste.Player player, Vector2 spawnPosition) : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            Position = spawnPosition;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            if (player?.StateMachine == null) yield break;

            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.Facing = Position.X >= player.X ? Facings.Right : Facings.Left;

            Add(reveal = GFX.SpriteBank.Create("morpho_knight_delta"));
            reveal.Play("mkd_appearground");
            reveal.Visible = false;

            level.Displacement.AddBurst(Position, 0.4f, 8f, 32f, 0.2f);
            yield return 0.4f;

            // A dark silhouette materializes on the ground.
            Audio.Play("event:/DZ/char/bosses/mkd/intromorphodelta", Position);
            reveal.Visible = true;
            reveal.Color = Color.Black;
            yield return 0.6f;

            // It stirs.
            reveal.Play("mkd_awaken");
            yield return 0.5f;

            // Dramatic unveil.
            level.Shake(0.6f);
            level.Flash(Color.White * 0.8f, true);
            Audio.Play("event:/DZ/char/bosses/mkd/morpholandedA", Position);
            for (float t = 0f; t < 0.5f; t += Engine.DeltaTime)
            {
                reveal.Color = Color.Lerp(Color.Black, Color.White, t / 0.5f);
                yield return null;
            }
            reveal.Color = Color.White;

            // On-guard stance, ready to fight.
            reveal.Play("mkd_firstultraintro");
            Audio.Play("event:/DZ/char/bosses/mkd/introintense", Position);
            yield return 1f;

            reveal.RemoveSelf();
            level.Add(new global::Celeste.Entities.Bosses.MorphoKnightDeltaBoss(Position));

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            reveal?.RemoveSelf();
            if (player != null)
                player.StateMachine.State = global::Celeste.Player.StNormal;
        }
    }
}
