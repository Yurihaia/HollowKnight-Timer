using System.IO;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;

namespace HKTimer {
    public class HKTimer : Mod {

        public static Settings settings { get; private set; } = new Settings();

        public static HKTimer instance { get; private set; }

        public GameObject gameObject { get; private set; }
        public Timer timer { get; private set; }
        public TriggerManager triggerManager { get; private set; }
        // oh god oh fuck
        public UI.UIManager ui { get; private set; }

        public HKTimer() : base("HKTimer") {
            if(instance != null) {
                return;
            }
            instance = this;
            gameObject = new GameObject();

            timer = gameObject.AddComponent<Timer>();
            timer.InitDisplay();

            triggerManager = gameObject.AddComponent<TriggerManager>().Initialize(timer);
            triggerManager.InitDisplay();

            ui = gameObject.AddComponent<UI.UIManager>().Initialize(triggerManager, this, timer);
            ui.InitDisplay();

            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(gameObject);

            this.ReloadSettings();
        }


        public void Unload() {
            this.timer.UnloadHooks();
            GameObject.Destroy(gameObject);
            USceneManager.activeSceneChanged -= SceneChanged;
            HKTimer.instance = null;
        }

        public void ReloadSettings() {
            string path = Application.persistentDataPath + "/hktimer.json";
            if(!File.Exists(path)) {
                Log("Writing default settings to " + path);
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
            } else {
                Log("Reading settings from " + path);
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
                // just to add the default shit I guess
                // might remove this when the format stabilizes
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
                settings.LogBindErrors();
            }
            // Reload text positions
            timer.InitDisplay();
            triggerManager.InitDisplay();
        }

        private void SceneChanged(Scene from, Scene to) {
            triggerManager.SpawnTriggers();
        }
    }
}