using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;
public class CS20_Later : CutsceneEntity
{
    private Player player;

    public CS20_Later(CelestePlayer player)
    {
        this.player = player;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Level level = scene as Level;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = Player.StDummy; // StDummy
        
        yield return Textbox.Say(
            "DZ_CH20_MONTHS_LATER");
        
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        
        // Transition to the end cinematic
        Level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
        {
            // Trigger the 20 years later / end cinematic
            Level.TeleportTo(player, "end-ending", Player.IntroTypes.Transition);
        };
    }
        public override void Render()
    {
        base.Render();
        
        // Render fade to black overlay
        {
            Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.Black);
        }
    }
}