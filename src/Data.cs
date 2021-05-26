using Newtonsoft.Json;
using UnityEngine;

namespace HKTimer {
    public class Settings {
        public string pause = "[1]";
        public string reset = "[0]";
        public string open_ui = "f1";
        public string set_start = "[7]";
        public string set_end = "[8]";

        [JsonConverter(typeof(JsonVec2Converter))]
        public Vector2 timerPosition = new Vector2(1880, 1020);
        public int textSize = 40;

        public void LogBindErrors() {
            StringInputManager.LogBindErors(new string[] {
                pause,
                reset,
                open_ui,
                set_start,
                set_end
            });
        }
    }
}