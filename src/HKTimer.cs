using System.IO;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;
using System.Collections;

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

            triggerManager = gameObject.AddComponent<TriggerManager>().Initialize(timer);

            ui = gameObject.AddComponent<UI.UIManager>().Initialize(triggerManager, this, timer);

            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(gameObject);
            On.GameManager.Start += GameManager_Start;
            this.ReloadSettings();

        }

        private void GameManager_Start(On.GameManager.orig_Start orig, GameManager self)
        {
            timer.InitDisplay();
            triggerManager.InitDisplay();
            ui.gm = self;
            ui.InitDisplay();


            orig(self);
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

            GameManager.instance.StartCoroutine(Triggers(from,to));
        }

        private IEnumerator Triggers(Scene from, Scene to)
        {
            yield return new WaitWhile(() => !to.isLoaded);
            triggerManager.SpawnTriggers();
        }

    }
}