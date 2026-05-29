using Celeste.Entities;
using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 20: Fake Madeline and Badeline
    /// Kirby discovers what appear to be Madeline and Badeline, approaches them,
    /// only to be lured into a trap by Els using them as vessels.
    /// 
    /// Dialog triggers:
    ///   {trigger 0 kirby look closer}   → camera zooms in, Kirby faces them
    ///   {trigger 1 approach them}       → Kirby walks toward the NPCs
    /// </summary>
    public class CS20_FakeMadelineAndBadeline : CutsceneEntity
    {
        public const string Flag = "ch20_fake_madeline_and_badeline_trigger";

        private global::Celeste.Player player;
        private Entity fakeMadeline;
        private Entity fakeBadeline;

        // How far ahead the NPCs are placed relative to the player
        private const float NPC_FORWARD_OFFSET = 120f;
        private const float NPC_SIDE_SPREAD   = 20f;

        public CS20_FakeMadelineAndBadeline(global::Celeste.Player player) : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            if (player?.StateMachine == null) yield break;

            // ── Setup ────────────────────────────────────────────────────────────────
            player.StateMachine.State = Player.StDummy; // Player.StDummy
            player.DummyGravity       = true;
            player.DummyAutoAnimate   = true;
            player.Speed              = Vector2.Zero;

            // Face the pair — they appear ahead and to the right
            player.Facing = Facings.Right;
            player.Sprite.Play("lookUp");

            // Brief pause before the scene kicks off
            yield return 0.4f;

            // Spawn glowing silhouette dummies for Madeline & Badeline
            SpawnFakeDummies(level);

            yield return 0.3f;

            // Play the dialogue with two action triggers
            yield return Textbox.Say("CH20_FAKE_MADELINE_AND_BADELINE", new Func<IEnumerator>[]
            {
                LookCloser,   // trigger 0 – kirby look closer
                ApproachThem  // trigger 1 – approach them
            });

            // ── Els' reveal: flash and screen shake before the "final act" ──────────
            yield return 0.2f;

            Audio.Play("event:/pusheen/final_content/game/19_the_end/glitch_long",
                       player.Position);

            level.Shake(0.8f);

            yield return 0.15f;

            level.Flash(Color.DarkRed * 0.55f, false);

            level.Displacement.AddBurst(player.Center, 1.2f, 64f, 160f, 1.0f);

            yield return 0.35f;

            // Remove the fake NPCs — they were illusions
            fakeMadeline?.RemoveSelf();
            fakeBadeline?.RemoveSelf();

            yield return 0.3f;

            EndCutscene(level);
        }

        // ── Action sequence: trigger 0 – "kirby look closer" ─────────────────────
        private IEnumerator LookCloser()
        {
            // Kirby leans forward and the camera gently zooms in
            player.Sprite.Play("lookUp");
            player.Facing = Facings.Right;

            Vector2 zoomTarget = new Vector2(
                player.X - Level.Camera.X + NPC_FORWARD_OFFSET * 0.5f,
                90f);

            Add(new Coroutine(Level.ZoomTo(zoomTarget, 1.8f, 0.6f), true));

            yield return 0.6f;
        }

        // ── Action sequence: trigger 1 – "approach them" ─────────────────────────
        private IEnumerator ApproachThem()
        {
            // Kirby walks toward the fake NPCs' position
            player.DummyAutoAnimate = true;
            float targetX = player.X + NPC_FORWARD_OFFSET * 0.55f;

            yield return player.DummyWalkTo(targetX, false, 1f, false);

            player.DummyAutoAnimate = false;
            player.Sprite.Play("idle");

            yield return 0.2f;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void SpawnFakeDummies(Level level)
        {
            // Use Celeste's ghost / NPC sprite bank so the figures look like the real deal
            // but slightly off-colour to hint at the deception
            Vector2 basePos = player.Position + new Vector2(NPC_FORWARD_OFFSET, 0f);

            // Madeline silhouette
            fakeMadeline = BuildSilhouette(
                basePos + new Vector2(-NPC_SIDE_SPREAD, 0f),
                "madeline",
                Color.Red * 0.7f);

            // Badeline silhouette (purple tinted)
            fakeBadeline = BuildSilhouette(
                basePos + new Vector2(NPC_SIDE_SPREAD, 0f),
                "badeline",
                Color.Purple * 0.7f);

            level.Add(fakeMadeline);
            level.Add(fakeBadeline);
        }

        /// <summary>Builds a simple sprite entity from the sprite bank.</summary>
        private static Entity BuildSilhouette(Vector2 position, string spriteName, Color tint)
        {
            Entity e = new Entity(position) { Depth = 100 };

            if (GFX.SpriteBank.Has(spriteName))
            {
                Sprite s = GFX.SpriteBank.Create(spriteName);
                s.Play("idle");
                s.Color = tint;
                e.Add(s);
            }

            return e;
        }

        // ── Cleanup on skip ───────────────────────────────────────────────────────
        public override void OnEnd(Level level)
        {
            fakeMadeline?.RemoveSelf();
            fakeBadeline?.RemoveSelf();

            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Player.StNormal
                player.DummyAutoAnimate   = true;
                player.DummyGravity       = true;
                player.Speed              = Vector2.Zero;
            }

            // Mark the scene as played so it doesn't replay
            level.Session.SetFlag(Flag);
        }
    }
}
