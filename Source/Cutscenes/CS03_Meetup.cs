using Celeste.NPCs;

namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs03Meetup(
    Npc03DZ magolor,
    global::Celeste.Player player,
    Coroutine zoomCoroutine,
    int currentConversation = 0)
    : CutsceneEntity {
    public const string FLAG = "cs03_meetup";
    private readonly Vector2 endPlayerPosition = magolor.Position + new Vector2(48f, 0.0f);
    private Coroutine zoomCoroutine = zoomCoroutine;
    private object badelineDummy;

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(cutscene()));
    }

    private IEnumerator cutscene()
    {
        if (player == null || player.StateMachine == null)
            yield break;
        if (magolor == null)
            yield break;

        var level = Level;
        if (level == null)
            yield break;

        // Determine conversation gating based on SaveData, like the NPC Talk logic
        int conv = currentConversation;
        if (!(global::Celeste.SaveData.Instance?.HasFlag("WassupMagolor") ?? false) ||
            !(global::Celeste.SaveData.Instance?.HasFlag("BadelineJoinKirby") ?? false))
        {
            conv = -1;
        }

        // Player enters a controlled state
        player.StateMachine.State = Player.StDummy;

        // If we have a recognized conversation (1..4), run that path
        if (conv >= 1 && conv <= 4)
        {
            // Trigger 0: Init and pan/zoom to Kirby and Magolor (use the zoom coroutine from trigger)
            if (zoomCoroutine != null)
            {
                yield return zoomCoroutine;
            }
            yield return 0.3f;

            // Trigger 1: Badeline walks to left of Magolor
            var badelineType = System.Type.GetType("global::Celeste.Mod.DesoloZantasHelper.Entities.BadelineDummy");
            if (badelineType != null)
            {
                var badeline = System.Activator.CreateInstance(badelineType, magolor.Position + new Vector2(-40f, 0f));
                badelineDummy = badeline;
                level.Add((Entity)badeline);
                
                var walkToMethod = badelineType.GetMethod("WalkTo");
                if (walkToMethod != null)
                {
                    var enumerator = (IEnumerator)walkToMethod.Invoke(badeline, new object[] { magolor.X - 24f, 64f });
                    while (enumerator.MoveNext())
                        yield return enumerator.Current;
                }
            }

            yield return 0.3f;

            // Trigger 2: Magolor turns left and encourages Badeline for Kirby to help find Madeline
            // Use reflection to access the private sprite field and flip it
            var spriteField = typeof(Npc03DZ).GetField("sprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (spriteField != null)
            {
                var sprite = spriteField.GetValue(magolor) as Sprite;
                if (sprite != null)
                {
                    sprite.Scale.X = -1f;
                }
            }
            yield return Textbox.Say("DZ_CH3_DZ_A");

            yield return 0.5f;

            // Trigger 3: Badeline sighs and says fine, but if he messes up she'll tear his soul apart with scary tentacle hair
            // (This is part of the dialog, shown via triggers in the dialog file)

            yield return 0.5f;

            // Trigger 4: Magolor suggests never doing that and Badeline puts tentacle hair away
            // (This is also part of the dialog sequence)

            yield return 0.5f;

            // Trigger 5: Badeline joins Kirby and merges
            if (badelineDummy != null && level != null && player != null)
            {
                var posProp = badelineType.GetProperty(nameof(Position));
                var from = (Vector2)posProp.GetValue(badelineDummy);

                // Move Badeline to player position
                for (float p = 0f; p < 1f; p += global::Monocle.Engine.DeltaTime / 0.25f)
                {
                    posProp.SetValue(badelineDummy, Vector2.Lerp(from, player.Position, global::Monocle.Ease.CubeIn(p)));
                    yield return null;
                }

                // Remove Badeline and give player dashes
                var removeSelf = badelineType.GetMethod(nameof(RemoveSelf));
                removeSelf?.Invoke(badelineDummy, null);
                badelineDummy = null;

                player.Dashes = 2;
                level.Session.Inventory.Dashes = 2;
            }

            endCutscene(level);
            yield break;
        }
    }

    private void endCutscene(Level level)
    {
        level.EndCutscene();
        OnEnd(level);
    }

    // Add this method to the CS03_Meetup class to resolve CS0103
    private IEnumerator playerApproachRightSideForConversation()
    {
        // Move the player to the right side of DZ for the conversation
        // This is a placeholder implementation; adjust as needed for your game logic
        if (player != null)
        {
            Vector2 target = endPlayerPosition;
            while (Vector2.Distance(player.Position, target) > 2f)
            {
                player.Position = Vector2.Lerp(player.Position, target, 0.1f);
                yield return null;
            }
        }
    }
    public override void OnEnd(Level level)
    {
        // Simplified: Remove problematic DZSprite reference
        player.Position.X = endPlayerPosition.X;
        player.Facing = Facings.Left;
        player.StateMachine.State = Player.StNormal;

        // Mark conversations complete if we reached the final one
        if (currentConversation == 4)
        {
            level.Session.SetFlag("cs03_meetup");
        }

        level.Session.SetFlag(FLAG);
        level.ResetZoom();
    }
}



