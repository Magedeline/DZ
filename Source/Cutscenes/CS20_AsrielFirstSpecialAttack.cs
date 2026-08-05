using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS20_AsrielFirstSpecialAttack : CutsceneEntity
    {
        private const string PlayedFlag = "dz_asriel_azzyboss14_special_attack_played";

        private readonly string firstSpecialAttackDialogKey;
        private readonly string madelineBadelineSaveDialogKey;
        private global::Celeste.Player player;
        private Level level;
        private SummonedMeteorite meteorite;

        public CS20_AsrielFirstSpecialAttack(string firstSpecialAttackDialogKey, string madelineBadelineSaveDialogKey)
            : base(true, false)
        {
            this.firstSpecialAttackDialogKey = string.IsNullOrWhiteSpace(firstSpecialAttackDialogKey)
                ? "DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK"
                : firstSpecialAttackDialogKey;
            this.madelineBadelineSaveDialogKey = string.IsNullOrWhiteSpace(madelineBadelineSaveDialogKey)
                ? "DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK"
                : madelineBadelineSaveDialogKey;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            this.level = level;
            player = level?.Tracker?.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StDummy;
                player.StateMachine.Locked = true;
                player.Speed = Vector2.Zero;
            }

            Func<IEnumerator>[] sequenceTriggers = CreateSequenceTriggers();
            yield return Textbox.Say(firstSpecialAttackDialogKey, sequenceTriggers);
            yield return 0.3f;
            yield return Textbox.Say(madelineBadelineSaveDialogKey, sequenceTriggers);
            yield return 0.5f;

            EndCutscene(level);
        }

        private Func<IEnumerator>[] CreateSequenceTriggers()
        {
            return new Func<IEnumerator>[]
            {
                Trigger0,
                Trigger1,
                Trigger2,
                Trigger3,
                Trigger4,
                Trigger5,
                Trigger6,
                Trigger7,
                Trigger8,
                Trigger9,
                Trigger10
            };
        }

        private IEnumerator Trigger0()
        {
            if (level == null || player == null)
            {
                yield break;
            }

            Vector2 summonTarget = player.Center + new Vector2(0f, -72f);
            Vector2 summonOrigin = summonTarget + new Vector2(0f, -96f);
            var asrielBoss = level.Tracker.GetEntity<global::Celeste.Entities.AsrielGodBoss>();
            if (asrielBoss != null)
            {
                summonOrigin = asrielBoss.Center + new Vector2(0f, 12f);
                asrielBoss.Sprite?.Play("castsp");
            }

            meteorite?.RemoveSelf();
            meteorite = new SummonedMeteorite(summonOrigin);
            level.Add(meteorite);

            Audio.Play("event:/DZ/new_content/char/bosses/asriel/big_star", summonOrigin);
            level.Shake(0.2f);
            level.Displacement.AddBurst(summonTarget, 0.3f, 16f, 48f, 0.6f);

            Vector2 start = summonOrigin;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.45f)
            {
                if (meteorite == null)
                {
                    yield break;
                }

                meteorite.Position = Vector2.Lerp(start, summonTarget, Ease.CubeOut(t));
                yield return null;
            }

            if (meteorite != null)
            {
                meteorite.Position = summonTarget;
            }

            for (int i = 0; i < 8; i++)
            {
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 1, summonTarget, Vector2.One * 8f);
                yield return 0.02f;
            }

            asrielBoss?.Sprite?.Play("idle");
            yield return 0.15f;
        }

        private IEnumerator Trigger1()
        {
            if (level == null)
            {
                yield break;
            }

            Vector2 slashCenter = meteorite?.Position ?? (player?.Center ?? Vector2.Zero) + new Vector2(0f, -72f);
            level.Add(new CutsceneSlashEffect(slashCenter, -0.55f, 96f, Color.White));
            Audio.Play("event:/DZ/new_content/char/bosses/asriel/cinematiccut", slashCenter);
            level.Flash(Color.White * 0.2f, false);
            level.Shake(0.25f);
            level.Displacement.AddBurst(slashCenter, 0.4f, 20f, 72f, 0.8f);

            if (meteorite != null)
            {
                Vector2 center = meteorite.Position;
                meteorite.RemoveSelf();
                meteorite = null;

                level.Add(new MeteoriteFragment(center + new Vector2(-10f, -2f), new Vector2(-90f, -60f), 10f));
                level.Add(new MeteoriteFragment(center + new Vector2(10f, 2f), new Vector2(90f, 60f), 10f));
            }

            for (int i = 0; i < 12; i++)
            {
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 1, slashCenter, new Vector2(18f, 10f));
                yield return 0.015f;
            }

            yield return 0.2f;
        }

        private IEnumerator Trigger2() { yield break; }

        private IEnumerator Trigger3() { yield break; }

        private IEnumerator Trigger4() { yield break; }

        private IEnumerator Trigger5() { yield break; }

        private IEnumerator Trigger6() { yield break; }

        private IEnumerator Trigger7() { yield break; }

        private IEnumerator Trigger8() { yield break; }

        private IEnumerator Trigger9() { yield break; }

        private IEnumerator Trigger10() { yield break; }

        private sealed class SummonedMeteorite : Entity
        {
            private float pulse;

            public SummonedMeteorite(Vector2 position)
                : base(position)
            {
                Depth = -1000000;
                Add(new VertexLight(Color.OrangeRed, 0.8f, 24, 48));
            }

            public override void Update()
            {
                base.Update();
                pulse += Engine.DeltaTime * 6f;
            }

            public override void Render()
            {
                base.Render();

                float scale = 1f + 0.08f * (float)Math.Sin(pulse);
                Draw.Circle(Position, 14f * scale, Color.OrangeRed, 12);
                Draw.Circle(Position, 8f * scale, Color.Yellow, 10);
                Draw.Line(Position + new Vector2(-18f, -18f), Position + new Vector2(-8f, -8f), Color.Orange * 0.8f);
                Draw.Line(Position + new Vector2(-24f, -8f), Position + new Vector2(-10f, -2f), Color.Yellow * 0.65f);
            }
        }

        private sealed class MeteoriteFragment : Entity
        {
            private Vector2 velocity;
            private readonly float radius;
            private float lifetime = 0.45f;
            private float rotation;
            private readonly float spin;

            public MeteoriteFragment(Vector2 position, Vector2 velocity, float radius)
                : base(position)
            {
                this.velocity = velocity;
                this.radius = radius;
                spin = Calc.Random.Range(-6f, 6f);
                Depth = -1000000;
            }

            public override void Update()
            {
                base.Update();

                Position += velocity * Engine.DeltaTime;
                velocity.Y += 180f * Engine.DeltaTime;
                rotation += spin * Engine.DeltaTime;
                lifetime -= Engine.DeltaTime;
                if (lifetime <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                base.Render();

                float alpha = Calc.ClampedMap(lifetime, 0f, 0.45f);
                Vector2 offset = Calc.AngleToVector(rotation, radius * 0.35f);
                Draw.Circle(Position + offset, radius, Color.OrangeRed * alpha, 10);
                Draw.Circle(Position - offset * 0.5f, radius * 0.6f, Color.Yellow * alpha, 8);
            }
        }

        private sealed class CutsceneSlashEffect : Entity
        {
            private readonly float angle;
            private readonly float length;
            private readonly Color color;
            private float lifetime = 0.2f;

            public CutsceneSlashEffect(Vector2 position, float angle, float length, Color color)
                : base(position)
            {
                this.angle = angle;
                this.length = length;
                this.color = color;
                Depth = -1000001;
            }

            public override void Update()
            {
                base.Update();

                lifetime -= Engine.DeltaTime;
                if (lifetime <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                base.Render();

                float alpha = Calc.ClampedMap(lifetime, 0f, 0.2f);
                Vector2 dir = Calc.AngleToVector(angle, length * (1f - alpha * 0.25f));
                Vector2 perp = new Vector2(-dir.Y, dir.X).SafeNormalize();
                Vector2 start = Position - dir * 0.5f;
                Vector2 end = Position + dir * 0.5f;

                for (int i = -2; i <= 2; i++)
                {
                    Vector2 offset = perp * (i * 2f);
                    Draw.Line(start + offset, end + offset, color * alpha);
                }

                Draw.Line(start, end, Color.White * alpha);
            }
        }

        public override void OnEnd(Level level)
        {
            meteorite?.RemoveSelf();
            meteorite = null;

            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            level?.Session?.SetFlag(PlayedFlag, true);
        }
    }
}
