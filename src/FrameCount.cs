using Modding;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;

namespace FrameDisplay
{
    public class FrameCount : MonoBehaviour
    {
        private int frame = 0;
        private bool timerActive = false;

        private Text display;

        public void ShowDisplay()
        {
            Modding.Logger.Log("[FrameDisplay] Creating display");
            GameObject go = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData rd = new CanvasUtil.RectData(
                new Vector2(400, 100),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.15f, 0.1f),
                new Vector2(0.15f, 0.1f)
            );
            display = CanvasUtil.CreateTextPanel(go, "test", 40, TextAnchor.LowerLeft, rd).GetComponent<Text>();
            display.text = "0";
            Object.DontDestroyOnLoad(go);
        }

        public void Update()
        {
            GameState state = GameManager.instance.gameState;
            if (state != GameState.LOADING && timerActive)
            {
                frame += 1;
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Pause))
            {
                timerActive ^= true;
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Reset))
            {
                frame = 0;
            }
            display.text = frame.ToString();
        }
    }
}