using Modding;
using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System;
using System.Reflection;

namespace HKTimer
{
    public class FrameCount : MonoBehaviour
    {
        public TimeSpan time = TimeSpan.Zero;
        public bool timerActive = false;

        private Text frameDisplay;

        public void ShowDisplay()
        {
            Modding.Logger.Log("[HKTimer] Creating display");
            GameObject go = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData timerRd = new CanvasUtil.RectData(
                new Vector2(400, 100),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.15f, 0.1f),
                new Vector2(0.15f, 0.1f)
            );
            // lower right corner text
            // CanvasUtil.RectData endRd = new CanvasUtil.RectData(
            //     new Vector2(800, 100),
            //     new Vector2(0.5f, 0.5f),
            //     new Vector2(0.7f, 0.1f),
            //     new Vector2(0.7f, 0.1f)
            // );
            frameDisplay = CanvasUtil.CreateTextPanel(go, "0:00:000", 40, TextAnchor.LowerLeft, timerRd).GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public void Update()
        {
            var updateTimer = false;
            if (Input.GetKeyDown(HKTimer.instance.settings.pause))
            {
                timerActive ^= true;
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.reset))
            {
                time = TimeSpan.Zero;
                timerActive = false;
                updateTimer = true;
            }
            if (timerActive && !TimerShouldBePaused())
            {
                time += System.TimeSpan.FromSeconds(Time.unscaledDeltaTime);
                if (Time.unscaledDeltaTime > 0) updateTimer = true;
            }
            if (updateTimer) frameDisplay.text = string.Format("{0}:{1:D2}.{2:D3}", Math.Floor(time.TotalMinutes), time.Seconds, time.Milliseconds);
        }


        // This uses the same disgusting logic as the autosplitter
        private bool lookForTeleporting;
        private GameState lastGameState = GameState.INACTIVE;

        private static FieldInfo cameraControlTeleporting = typeof(CameraController).GetField(
            "teleporting",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        private static FieldInfo gameManagerDirtyTileMap = typeof(GameManager).GetField(
            "tilemapDirty",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        private bool TimerShouldBePaused()
        {
            if(GameManager.instance == null) {
                // GameState is INACTIVE, so the teleporting code will run
                // teleporting defaults to false
                // (lookForTeleporting && (
                //    teleporting || (gameState != GameState.PLAYING && gameState != GameState.ENTERING_LEVEL)
                // ))
                if(lookForTeleporting) {
                    lookForTeleporting = false;
                }
                lastGameState = GameState.INACTIVE;
                return false;
            }

            var nextScene = GameManager.instance.nextSceneName;
            var sceneName = GameManager.instance.sceneName;
            var uiState = GameManager.instance.ui.uiState;
            var gameState = GameManager.instance.gameState;

            bool loadingMenu = (string.IsNullOrEmpty(nextScene) && sceneName != "Menu_Title") || (nextScene == "Menu_Title" && sceneName != "Menu_Title");
            if (gameState == GameState.PLAYING && lastGameState == GameState.MAIN_MENU)
            {
                lookForTeleporting = true;
            }
            bool teleporting = (bool)cameraControlTeleporting.GetValue(GameManager.instance.cameraCtrl);
            if (lookForTeleporting && (teleporting || (gameState != GameState.PLAYING && gameState != GameState.ENTERING_LEVEL)))
            {
                lookForTeleporting = false;
            }

            var shouldPause = (
                gameState == GameState.PLAYING
                && teleporting
                && !GameManager.instance.hero_ctrl.cState.hazardRespawning
            ) || lookForTeleporting
                || ((gameState == GameState.PLAYING || gameState == GameState.ENTERING_LEVEL) && uiState != UIState.PLAYING)
                    || (gameState != GameState.PLAYING && !GameManager.instance.inputHandler.acceptingInput)
                    || gameState == GameState.EXITING_LEVEL
                    || gameState == GameState.LOADING
                    || GameManager.instance.hero_ctrl.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
                    || (uiState != UIState.PLAYING
                        && (uiState != UIState.PAUSED || loadingMenu)
                        && (!string.IsNullOrEmpty(nextScene) || sceneName == "_test_charms" || loadingMenu)
                        && nextScene != sceneName)
                    || (bool)gameManagerDirtyTileMap.GetValue(GameManager.instance);

            lastGameState = gameState;

            return shouldPause;
        }
    }
}