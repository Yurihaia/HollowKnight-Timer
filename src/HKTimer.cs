using System.IO;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;

namespace HKTimer
{
    public class HKTimer : Mod
    {

        public Settings settings = new Settings();

        internal static HKTimer instance;

        public FrameCount frameCount;
        public TargetManager targetManager;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            GameObject obj = new GameObject();
            frameCount = obj.AddComponent<FrameCount>();
            frameCount.ShowDisplay();
            obj.AddComponent<SettingsManager>();
            targetManager = obj.AddComponent<TargetManager>();
            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(obj);
        }

        public void ReloadSettings()
        {
            string path = Application.persistentDataPath + "/hktimer.json";
            if (!File.Exists(path))
            {
                Modding.Logger.Log("[HKTimer] Writing default settings to " + path);
                File.WriteAllText(path, JsonUtility.ToJson(this.settings, true));
            }
            else
            {
                Modding.Logger.Log("[HKTimer] Reading settings from " + path);
                this.settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
            }
        }

        private static void SceneChanged(Scene from, Scene to)
        {
            HKTimer.instance.targetManager.SpawnTriggers(to.name);
        }
    }

    public class SettingsManager : MonoBehaviour
    {
        public void Start()
        {
            Modding.Logger.Log("[HKTimer] Reloading settings");
            HKTimer.instance.ReloadSettings();
        }

        public void Update()
        {
            if (Input.GetKeyDown(HKTimer.instance.settings.reload_settings))
            {
                Modding.Logger.Log("[HKTimer] Reloading settings");
                HKTimer.instance.ReloadSettings();
            }
        }
    }
}