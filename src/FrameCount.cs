using Modding;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections;

namespace FrameDisplay
{
    public class FrameCount : MonoBehaviour
    {
        private int frame = 0;
        private bool timerActive = false;

        private string goalRoom = null;

        private Text frameDisplay;
        private Text goalDisplay;

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
            goalDisplay = CanvasUtil.CreateTextPanel(go, "None", 25, TextAnchor.LowerRight, goalRd).GetComponent<Text>();
            Object.DontDestroyOnLoad(go);
            StartCoroutine(Tick());
        }

        public IEnumerator Tick()
        {
            Modding.Logger.Log("[FrameDisplay] Starting Coroutine");
            while (true)
            {
                if (GameManager.instance.gameState != GameState.LOADING && timerActive)
                {
                    frame += 1;
                }
                yield return new WaitForSecondsRealtime(0.02f);
            }
        }

        public void Update()
        {
            if (GameManager.instance.gameState != GameState.LOADING)
            {
                if (Input.GetKeyDown(FrameDisplay.Instance.settings.SetGoal))
                {
                    if (goalRoom == GameManager.instance.sceneName)
                    {
                        goalRoom = null;
                    }
                    else
                    {
                        goalRoom = GameManager.instance.sceneName;
                    }
                }
                if (goalRoom != null && goalRoom == GameManager.instance.sceneName)
                {
                    timerActive = false;
                }
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Pause))
            {
                timerActive ^= true;
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.Reset))
            {
                frame = 0;
                timerActive = false;
            }
            frameDisplay.text = frame.ToString();
            if (goalRoom == null)
            {
                goalDisplay.text = "No Goal";
            }
            else
            {
                goalDisplay.text = goalRoom;
            }
        }
    }
}