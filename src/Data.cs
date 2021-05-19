using InControl;
using Newtonsoft.Json;
using UnityEngine;
using Modding.Converters;
using Modding;

namespace HKTimer {
    public class Settings : ModSettings {
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public TimerActions keybinds = new TimerActions().SetDefaultBinds();
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 timerPosition = new Vector2(1880, 1020);
        public int textSize = 30;
        public bool showTimer = true;
        public string trigger = "Collision";
    }

    public class TimerActions : PlayerActionSet {
        public PlayerAction pause;
        public PlayerAction reset;
        public PlayerAction setStart;
        public PlayerAction setEnd;

        public TimerActions() {
            this.pause = base.CreatePlayerAction("pause");
            this.reset = base.CreatePlayerAction("reset");
            this.setStart = base.CreatePlayerAction("set_start");
            this.setEnd = base.CreatePlayerAction("set_end");
        }

        public TimerActions SetDefaultBinds() {
            if(this.pause.Bindings.Count < 1) this.pause.AddDefaultBinding(Key.Pad1);
            if(this.reset.Bindings.Count < 1) this.reset.AddDefaultBinding(Key.Pad0);
            if(this.setStart.Bindings.Count < 1) this.setStart.AddDefaultBinding(Key.Pad7);
            if(this.setEnd.Bindings.Count < 1) this.setEnd.AddDefaultBinding(Key.Pad8);
            return this;
        }
    }
}