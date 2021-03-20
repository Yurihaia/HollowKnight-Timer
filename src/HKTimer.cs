using System.IO;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;

namespace HKTimer {
    public class HKTimer : Mod, ITogglableMod {

        public static Settings settings { get; private set; } = new Settings();

        public static HKTimer instance { get; private set; }

        public GameObject gameObject { get; private set; }
        public Timer timer { get; private set; }
        public TriggerManager triggerManager { get; private set; }
        // oh god oh fuck
        public UI.UIManager ui { get; private set; }

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() {
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
            GameObject.Destroy(gameObject);
            USceneManager.activeSceneChanged -= SceneChanged;
            HKTimer.instance = null;
        }

        public void ReloadSettings() {
            string path = Application.persistentDataPath + "/hktimer.json";
            if(!File.Exists(path)) {
                Modding.Logger.Log("[HKTimer] Writing default settings to " + path);
                File.WriteAllText(path, JsonUtility.ToJson(settings, true));
            } else {
                Modding.Logger.Log("[HKTimer] Reading settings from " + path);
                settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
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