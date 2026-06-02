using Celeste.NPCs;

namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs03Meetup(
    Npc03Maggy magolor,
    global::Celeste.Player player,
    Coroutine zoomCoroutine,
    int currentConversation = 0)
    : CutsceneEntity {
    public const string FLAG = "maggy_03_Meetup";
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
            yield return playerApproachRightSideForConversation();
            switch (conv)
            {
                case 1:
                    // Dynamically create BadelineDummy and float to player before dialog
                    {
                        var level = Level;
                        if (level != null && player != null)
                        {
                            var badelineType = System.Type.GetType("global::Celeste.Mod.DesoloZantasHelper.Entities.BadelineDummy, IngesteHelper");
                            if (badelineType != null)
                            {
                                var badeline = System.Activator.CreateInstance(badelineType, player.Position);
                                badelineDummy = badeline;
                                level.Add((Entity)badeline);
                                var floatToMethod = badelineType.GetMethod("FloatTo");
                                if (floatToMethod != null)
                                {
                                    var enumerator = (IEnumerator)floatToMethod.Invoke(badeline, new object[] { player.Position + new Vector2(20f, -16f), false });
                                    while (enumerator.MoveNext())
                                        yield return enumerator.Current;
                                }
                            }
                        }
                    }

                    yield return Textbox.Say("CH3_MAGGY_A");

                    // Merge Badeline after dialog and give dashes
                    {
                        var level = Level;
                        if (badelineDummy != null && level != null && player != null)
                        {
                            var badelineType = badelineDummy.GetType();
                            var posProp = badelineType.GetProperty(nameof(Position));
                            var from = (Vector2)posProp.GetValue(badelineDummy);

                            for (float p = 0f; p < 1f; p += global::Monocle.Engine.DeltaTime / 0.25f)
                            {
                                posProp.SetValue(badelineDummy, Vector2.Lerp(from, player.Position, global::Monocle.Ease.CubeIn(p)));
                                yield return null;
                            }

                            var removeSelf = badelineType.GetMethod(nameof(RemoveSelf));
                            removeSelf?.Invoke(badelineDummy, null);
                            badelineDummy = null;

                            player.Dashes = 2;
                            level.Session.Inventory.Dashes = 2;
                        }
                    }
                    break;

                case 2:
                    yield return Textbox.Say("CH3_MAGGY_B");
                    break;

                case 3:
                    yield return Textbox.Say("CH3_MAGGY_C");
                    break;

                case 4:
                    yield return Textbox.Say("CH3_MAGGY_D");
                    break;
            }

            endCutscene(Level);
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
        // Move the player to the right side of Maggy for the conversation
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
        // Simplified: Remove problematic MaggySprite reference
        player.Position.X = endPlayerPosition.X;
        player.Facing = Facings.Left;
        player.StateMachine.State = Player.StNormal;

        // Mark conversations complete if we reached the final one
        if (currentConversation == 4)
        {
            level.Session.SetFlag("maggy_03_Meetup");
        }

        level.Session.SetFlag(FLAG);
        level.ResetZoom();
    }
}



