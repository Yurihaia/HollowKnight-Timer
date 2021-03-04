using System;

namespace HKTimer
{
    [Serializable]
    public class Settings
    {
        public string pause = "[1]";
        public string reset = "[0]";
        public string reload_settings = "[5]";
        public string set_start = "[7]";
        public string set_end = "[8]";
        public string load_triggers = "[9]";
        public string save_triggers = "[6]";
        public float timerAnchorX = 0.15f;
        public float timerAnchorY = 0.2f;

        public void LogBindErrors()
        {
            StringInputManager.LogBindErors(new string[] {
                reload_settings,
                set_start,
                set_end,
                load_triggers,
                save_triggers
            });
        }
    }
}