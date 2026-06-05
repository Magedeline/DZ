#nullable enable
using Celeste.NPCs;
using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS05_MagolorEscape : CutsceneEntity
    {
        public const string Flag = "resort_maggy";

        private NPC05_Magolor_Escaping magolor;
        private global::Celeste.Player player;
        private Vector2 magolorStart;

        public CS05_MagolorEscape(NPC05_Magolor_Escaping theo, global::Celeste.Player player) : base()
        {
            this.magolor = theo;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            magolorStart = magolor.Position;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkTo(magolor.X - 64f);
            player.Facing = Facings.Right;
            yield return Level.ZoomTo(new Vector2(240f, 135f), 2f, 0.5f);
            Func<IEnumerator>[] events = [StopRemovingVent, StartRemoveVent, RemoveVent, GivePhone];
            string dialog = "MAGGYHELPER_CH5_MAGGY_INTRO";
            if (!SaveData.Instance.HasFlag("MetMaggy"))
            {
                dialog = "MAGGYHELPER_CH5_MAGGY_NEVER_MET";
            }
            else if (!SaveData.Instance.HasFlag("MaggyKnowsName"))
            {
                dialog = "MAGGYHELPER_CH5_MAGGY_NEVER_INTRODUCED";
            }
            yield return Textbox.Say(dialog, events);
            magolor.Sprite.Scale.X = 1f;
            yield return 0.2f;
            magolor.Sprite.Play("walk");
            while (!magolor.CollideCheck<Solid>(magolor.Position + new Vector2(2f, 0f)))
            {
                yield return null;
                magolor.X += 48f * Engine.DeltaTime;
            }
            magolor.Sprite.Play("idle");
            yield return 0.2f;
            Audio.Play("event:/char/theo/resort_standtocrawl", magolor.Position);
            magolor.Sprite.Play("duck");
            yield return 0.5f;
            // Talker component not used in this NPC
            level.Session.SetFlag("resort_maggy_escaped");
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
            magolor.CrawlUntilOut();
            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        private IEnumerator StartRemoveVent()
        {
            magolor.Sprite.Scale.X = 1f;
            yield return 0.1f;
            Audio.Play("event:/char/theo/resort_vent_grab", magolor.Position);
            magolor.Sprite.Play("goToVent");
            yield return 0.25f;
        }

        private IEnumerator StopRemovingVent()
        {
            magolor.Sprite.Play("idle");
            yield return 0.1f;
            magolor.Sprite.Scale.X = -1f;
        }

        private IEnumerator RemoveVent()
        {
            yield return 0.8f;
            Audio.Play("event:/char/theo/resort_vent_rip", magolor.Position);
            magolor.Sprite.Play("fallVent");
            yield return 0.8f;
            magolor.grate.Fall();
            yield return 0.8f;
            magolor.Sprite.Scale.X = -1f;
            yield return 0.25f;
        }

        private IEnumerator GivePhone()
        {
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                magolor.Sprite.Play("walk");
                magolor.Sprite.Scale.X = -1f;
                while (magolor.X > player.X + 24f)
                {
                    magolor.X -= 48f * Engine.DeltaTime;
                    yield return null;
                }
            }
            magolor.Sprite.Play("idle");
            yield return 1f;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
            level.Session.SetFlag("resort_maggy_escaped");
            SaveData.Instance.SetFlag("MetMaggy");
            SaveData.Instance.SetFlag("MaggyKnowsName");
            if (magolor != null && WasSkipped)
            {
                magolor.Position = magolorStart;
                magolor.CrawlUntilOut();
                magolor.grate?.RemoveSelf();
            }
        }
    }
}

