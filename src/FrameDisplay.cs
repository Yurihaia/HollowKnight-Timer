using System.Reflection;
using Modding;
using UnityEngine;

namespace FrameDisplay
{
    public class FrameDisplay : Mod<SaveSettings, GlobalSettings>
    {

        public GlobalSettings settings;

        internal static FrameDisplay Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            if (Instance != null)
            {
                return;
            }
            settings = GlobalSettings;
            Instance = this;
            GameObject obj = new GameObject();
            FrameCount ct = obj.AddComponent<FrameCount>();
            ct.ShowDisplay();
            Object.DontDestroyOnLoad(obj);
        }
    }
}