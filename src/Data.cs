using System;

namespace HKTimer {
    [Serializable]
    public class Settings {
        public string pause = "[1]";
        public string reset = "[0]";
        public string open_ui = "f1";
        public string set_start = "[7]";
        public string set_end = "[8]";
        public float timerAnchorX = 0.15f;
        public float timerAnchorY = 0.2f;

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