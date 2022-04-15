using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace HKTimer.UI {
    // AaaaAAAaAaaAAA
    public class UIManager : MonoBehaviour {
        private bool uiOpen = false;

        private GameObject menu;

        public TriggerManager tm { get; set; }
        public HKTimer hktimer { get; set; }
        public Timer timer { get; set; }

        private static readonly Vector2 MIDDLE = new Vector2(0.5f, 0.5f);

        public UIOption<ToggleOption> showTimer { get; private set; }
        public UIButton resetPb { get; private set; }

        public UIOption<TriggerTypeOption> triggerType { get; private set; }
        public UIButton saveTriggers { get; private set; }
        public UIButton loadTriggers { get; private set; }

        public UIButton reloadSettings { get; private set; }

        public UIElement[] elements { get; private set; }

        public GameObject cursorDisplay { get; private set; }

        private int cursor;

        public UIManager Initialize(TriggerManager tm, HKTimer hktimer, Timer timer) {
            this.tm = tm;
            this.hktimer = hktimer;
            this.timer = timer;
            return this;
        }

        public void InitDisplay() {
            if(menu != null) {
                GameObject.DestroyImmediate(menu);
            }
            CanvasUtil.CreateFonts();
            menu = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            this.showTimer = new UIOption<ToggleOption>(
                CanvasUtil.CreateTextPanel(
                    menu, "Show Timer", 30, TextAnchor.MiddleLeft,
                    new CanvasUtil.RectData(new Vector2(300, 60), new Vector2(-150, 180), MIDDLE, MIDDLE)
                ),
                CanvasUtil.CreateTextPanel(
                    menu, "", 30, TextAnchor.MiddleRight,
                    new CanvasUtil.RectData(new Vector2(300, 60), new Vector2(150, 180), MIDDLE, MIDDLE)
                ),
                new Vector2(-330, 180),
                ToggleOption.values,
                (v) => this.timer.ShowDisplay(v.enabled),
                1
            );
            this.resetPb = new UIButton(
                CanvasUtil.CreateTextPanel(
                    menu, "Reset PB", 30, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(new Vector2(360, 60), new Vector2(0, 120), MIDDLE, MIDDLE)
                ),
                new Vector2(-210, 120),
                () => {
                    if(this.tm != null) {
                        this.tm.ResetPB();
                        this.tm.ShowAlert("Reset PB");
                    }
                }
            );

            this.triggerType = new UIOption<TriggerTypeOption>(
                CanvasUtil.CreateTextPanel(
                    menu, "Trigger Type", 30, TextAnchor.MiddleLeft,
                    new CanvasUtil.RectData(new Vector2(300, 60), new Vector2(-150, 0), MIDDLE, MIDDLE)
                ),
                CanvasUtil.CreateTextPanel(
                    menu, "", 30, TextAnchor.MiddleRight,
                    new CanvasUtil.RectData(new Vector2(300, 60), new Vector2(150, 0), MIDDLE, MIDDLE)
                ),
                new Vector2(-330, 0),
                TriggerTypeOption.values,
                (e) => this.tm.triggerPlaceType = e.variant
            );
            this.saveTriggers = new UIButton(
                CanvasUtil.CreateTextPanel(
                    menu, "Save Triggers", 30, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(new Vector2(360, 60), new Vector2(0, -60), MIDDLE, MIDDLE)
                ),
                new Vector2(-210, -60),
                () => {
                    if(this.tm != null) {
                        this.tm.SaveTriggers();
                        this.tm.ShowAlert("Saved Triggers");
                    }
                }
            );
            this.loadTriggers = new UIButton(
                CanvasUtil.CreateTextPanel(
                    menu, "Load Triggers", 30, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(new Vector2(360, 60), new Vector2(0, -120), MIDDLE, MIDDLE)
                ),
                new Vector2(-210, -120),
                () => {
                    if(this.tm != null) {
                        this.tm.LoadTriggers();
                        this.tm.ShowAlert("Loaded Triggers");
                    }
                }
            );

            this.reloadSettings = new UIButton(
                CanvasUtil.CreateTextPanel(
                    menu, "Reload Settings", 30, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(new Vector2(360, 60), new Vector2(0, -240), MIDDLE, MIDDLE)
                ),
                new Vector2(-210, -240),
                () => {
                    if(this.hktimer != null) {
                        this.hktimer.ReloadSettings();
                        if(this.tm != null) this.tm.ShowAlert("Reloaded Settings");
                    }
                }
            );

            this.cursorDisplay = CanvasUtil.CreateTextPanel(
                menu, ">", 50, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(60, 60), new Vector2(0, 0), MIDDLE, MIDDLE)
            );

            this.elements = new UIElement[]{
                this.showTimer,
                this.resetPb,
                this.triggerType,
                this.saveTriggers,
                this.loadTriggers,
                this.reloadSettings
            };
            this.SetShown(false);
            this.uiOpen = false;
            GameObject.DontDestroyOnLoad(this.menu);
        }

        public void SetShown(bool show) {
            foreach(var e in this.elements) {
                e.SetShown(show);
            }
            this.cursorDisplay.SetActive(show);
            if(show) GameObject.DontDestroyOnLoad(this.cursorDisplay);
        }

        private void UpdateCursor() {
            this.cursorDisplay
                .GetComponent<RectTransform>()
                .anchoredPosition = this.elements[this.cursor].cursorPos;
        }

        public void Update() {
            if(StringInputManager.GetKeyDown(HKTimer.settings.open_ui)) {
                if(this.uiOpen) {
                    this.uiOpen = false;
                    this.SetShown(false);
                    if(GameManager.instance.hero_ctrl != null) GameManager.instance.hero_ctrl.RegainControl();
                } else {
                    this.uiOpen = true;
                    this.SetShown(true);
                    this.cursor = 0;
                    this.UpdateCursor();
                    if(GameManager.instance.hero_ctrl != null) GameManager.instance.hero_ctrl.RelinquishControl();
                }
            } else if(this.uiOpen) {
                var inputHandler = GameManager.instance.inputHandler;
                if (inputHandler == null) return;
                if (inputHandler.inputActions.left.WasPressed) {
                    this.elements[this.cursor].Left();
                } else if(inputHandler.inputActions.right.WasPressed) {
                    this.elements[this.cursor].Right();
                } else if(inputHandler.inputActions.up.WasPressed) {
                    if(this.cursor == 0) {
                        this.cursor = elements.Length;
                    }
                    this.cursor -= 1;
                    this.UpdateCursor();
                } else if(inputHandler.inputActions.down.WasPressed) {
                    this.cursor += 1;
                    if(this.cursor == elements.Length) {
                        this.cursor = 0;
                    }
                    this.UpdateCursor();
                } else if(inputHandler.inputActions.menuSubmit.WasPressed) {
                    this.elements[this.cursor].Accept();
                }
            }
        }
    }

    public class ToggleOption {
        public static readonly ToggleOption[] values = {
            new ToggleOption(false),
            new ToggleOption(true)
        };
        public bool enabled { get; set; }
        public ToggleOption(bool enabled) => this.enabled = enabled;
        public override string ToString() {
            return enabled ? "On" : "Off";
        }
    }

    public class TriggerTypeOption {
        public static readonly TriggerTypeOption[] values = {
            new TriggerTypeOption(TriggerManager.TriggerPlaceType.Collision),
            new TriggerTypeOption(TriggerManager.TriggerPlaceType.Movement),
            new TriggerTypeOption(TriggerManager.TriggerPlaceType.Scene)
        };
        public TriggerManager.TriggerPlaceType variant { get; set; }
        public TriggerTypeOption(TriggerManager.TriggerPlaceType variant) => this.variant = variant;
        public override string ToString() {
            return this.variant switch {
                TriggerManager.TriggerPlaceType.Collision => "Collision",
                TriggerManager.TriggerPlaceType.Movement => "Movement",
                TriggerManager.TriggerPlaceType.Scene => "Scene",
                _ => "unknown"
            };
        }
    }

    public abstract class UIElement {
        public abstract Vector2 cursorPos { get; protected set; }
        public abstract void SetShown(bool show);
        public virtual void Left() { }
        public virtual void Right() { }
        public virtual void Accept() { }
    }

    public class UIOption<E> : UIElement {
        private GameObject nameEl;
        private GameObject valEl;
        private Action<E> onUpdate;
        public Text nameText { get; private set; }
        public Text valText { get; private set; }

        private int valueInd = 0;
        public E value { get => values[valueInd]; }

        public override Vector2 cursorPos { get; protected set; }

        private readonly E[] values;

        public UIOption(
            GameObject nameEl,
            GameObject valEl,
            Vector2 cursorAnchor,
            E[] values
        ) : this(nameEl, valEl, cursorAnchor, values, default, 0) { }

        public UIOption(
            GameObject nameEl,
            GameObject valEl,
            Vector2 cursorAnchor,
            E[] values,
            Action<E> onUpdate
        ) : this(nameEl, valEl, cursorAnchor, values, onUpdate, 0) { }

        public UIOption(
            GameObject nameEl,
            GameObject valEl,
            Vector2 cursorAnchor,
            E[] values,
            Action<E> onUpdate,
            int defaultInd
        ) {
            this.values = values;
            this.valueInd = defaultInd;
            this.nameEl = nameEl;
            this.cursorPos = cursorAnchor;
            this.valEl = valEl;
            this.onUpdate = onUpdate;
            this.nameText = nameEl.GetComponent<Text>();
            this.valText = valEl.GetComponent<Text>();
            this.UpdateText();
        }

        private void UpdateText() {
            this.valText.text = this.value.ToString();
        }

        public override void Left() {
            if(valueInd == 0) {
                valueInd = values.Length;
            }
            valueInd -= 1;
            if(this.onUpdate != null) this.onUpdate(this.value);
            this.UpdateText();
            base.Left();
        }

        public override void Right() {
            valueInd += 1;
            if(valueInd == values.Length) {
                valueInd = 0;
            }
            if(this.onUpdate != null) this.onUpdate(this.value);
            this.UpdateText();
            base.Right();
        }

        public override void SetShown(bool show) {
            this.nameEl.SetActive(show);
            this.valEl.SetActive(show);
            if(show) {
                GameObject.DontDestroyOnLoad(this.nameEl);
                GameObject.DontDestroyOnLoad(this.valEl);
            }
        }
    }

    public class UIButton : UIElement {
        private GameObject el;
        public Text text { get; private set; }

        public override Vector2 cursorPos { get; protected set; }

        private Action onClick;

        public UIButton(GameObject el, Vector2 cursorAnchor, Action onClick) {
            this.el = el;
            this.onClick = onClick;
            this.cursorPos = cursorAnchor;
            this.text = el.GetComponent<Text>();
            GameObject.DontDestroyOnLoad(this.el);
        }

        public override void Accept() {
            this.onClick();
            base.Accept();
        }

        public override void SetShown(bool show) {
            this.el.SetActive(show);
            if(show) GameObject.DontDestroyOnLoad(this.el);
        }
    }
}