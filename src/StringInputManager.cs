using UnityEngine;

namespace HKTimer {
    public class StringInputManager {
        public static void LogBindErors(string[] keys) {
            foreach (var s in keys) {
                try {
                    if (s != null) Input.GetKeyDown(s);
                } catch {
                    HKTimer.instance.Log("Invalid key code '" + s + "'");
                }
            }
        }

        public static bool GetKeyDown(string key) {
            try {
                return key != null && Input.GetKeyDown(key);
            } catch {
                return false;
            }
        }
    }
}