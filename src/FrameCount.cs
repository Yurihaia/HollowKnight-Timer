using Modding;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections;
using System;

namespace FrameDisplay
{
    public class FrameCount : MonoBehaviour
    {
        public TimeSpan time = TimeSpan.Zero;
        public bool timerActive = false;

        private Text frameDisplay;

        public void ShowDisplay()
        {
            Modding.Logger.Log("[FrameDisplay] Creating display");
            GameObject go = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData timerRd = new CanvasUtil.RectData(
                new Vector2(400, 100),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.15f, 0.1f),
                new Vector2(0.15f, 0.1f)
            );
            CanvasUtil.RectData goalRd = new CanvasUtil.RectData(
                new Vector2(800, 100),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.7f, 0.1f),
                new Vector2(0.7f, 0.1f)
            );
            frameDisplay = CanvasUtil.CreateTextPanel(go, "0", 40, TextAnchor.LowerLeft, timerRd).GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public void Update()
        {
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Pause))
            {
                timerActive ^= true;
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Reset))
            {
                time = TimeSpan.Zero;
                timerActive = false;
            }
            if (GameManager.instance.gameState != GameState.LOADING && timerActive)
            {
                time += System.TimeSpan.FromSeconds(Time.unscaledDeltaTime);
            }
            frameDisplay.text = string.Format("{0}:{1:D2}.{2:D3}", Math.Floor(time.TotalMinutes), time.Seconds, time.Milliseconds);
        }
    }
}