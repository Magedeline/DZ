// Decompiled with JetBrains decompiler
// Type: Celeste.CreditsTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
using MaggyHelper.Cutscenes;

namespace MaggyHelper.Triggers
{
    [CustomEntity(ids: "MaggyHelper/CreditsTriggerPart2")]
    [Tracked(true)]
    public class CreditsTriggerPart2 : global::Celeste.Trigger
    {
        public string Event;

        public CreditsTriggerPart2(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Event = data.Attr("event");
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            this.Triggered = true;
            if (CS17_Credits.Instance == null)
                return;
            CS17_Credits.Instance.Event = this.Event;
        }
    }

    // Added: Trigger for CsPart1Credit cutscene
    [CustomEntity(ids: "MaggyHelper/CreditsTriggerPart1")]
    [Tracked(true)]
    public class CreditsTriggerPart1 : global::Celeste.Trigger
    {
        public CreditsTriggerPart1(EntityData data, Vector2 offset) : base(data, offset)
        {
            this.Event = data.Attr("event");
        }

        public string Event { get; set; }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            // Defer Oui creation to Overworld to avoid atlas/state issues
            IngesteModule.LaunchPart1Credits = true;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu, null);
        }
    }
}



