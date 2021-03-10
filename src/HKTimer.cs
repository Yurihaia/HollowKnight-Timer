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

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() {
            if (instance != null) {
                return;
            }
            instance = this;
            gameObject = new GameObject();

            timer = gameObject.AddComponent<Timer>();
            timer.ShowDisplay();

            gameObject.AddComponent<SettingsManager>();

            triggerManager = gameObject.AddComponent<TriggerManager>().Initialize(timer);
            triggerManager.ShowDisplay();

            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(gameObject);
        }

        public void Unload() {
            GameObject.Destroy(gameObject);
            USceneManager.activeSceneChanged -= SceneChanged;
            HKTimer.instance = null;
        }

        public void ReloadSettings() {
            string path = Application.persistentDataPath + "/hktimer.json";
            if (!File.Exists(path)) {
                Modding.Logger.Log("[HKTimer] Writing default settings to " + path);
                File.WriteAllText(path, JsonUtility.ToJson(settings, true));
            } else {
                Modding.Logger.Log("[HKTimer] Reading settings from " + path);
                settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
                settings.LogBindErrors();
            }
            // Reload text positions
            timer.ShowDisplay();
            triggerManager.ShowDisplay();
        }

        private void SceneChanged(Scene from, Scene to) {
            triggerManager.SpawnTriggers();
        }
    }

    public class SettingsManager : MonoBehaviour {
        public void Start() {
            Modding.Logger.Log("[HKTimer] Reloading settings");
            HKTimer.instance.ReloadSettings();
        }

        public void Update() {
            if (StringInputManager.GetKeyDown(HKTimer.settings.reload_settings)) {
                Modding.Logger.Log("[HKTimer] Reloading settings");
                HKTimer.instance.ReloadSettings();
            }
        }
    }
}