using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

public class PrologueEndingVignetteText : Entity
{
    private FancyText.Text text;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public PrologueEndingVignetteText(bool instant)
    {
        base.Tag = Tags.HUD;
        text = FancyText.Parse(Dialog.Clean("CH0_END_YOUR_CHOICE_MATTERS"), 960, 4, 0f);
        Add(new Coroutine(Routine(instant)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Routine(bool instant)
    {
        if (!instant)
        {
            yield return 4f;
        }
        for (int i = 0; i < text.Count; i++)
        {
            if (text[i] is FancyText.Char c)
            {
                while ((c.Fade += Engine.DeltaTime * 20f) < 1f)
                {
                    yield return null;
                }
                c.Fade = 1f;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        text.Draw(Position, new Vector2(0.5f, 0.5f), Vector2.One, 1f);
    }
}
