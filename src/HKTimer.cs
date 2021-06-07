using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;
using Modding.Menu;
using Modding.Menu.Config;

namespace HKTimer {
    public class HKTimer : Mod, ITogglableMod, ICustomMenuMod, IGlobalSettings<Settings> {
        public static Settings settings { get; set; } = new Settings();

        public void OnLoadGlobal(Settings s) => settings = s;
        public Settings OnSaveGlobal() => settings;

        public void OnLoadLocal(double s) => Log($"Loaded {s}");
        public double OnSaveLocal() {
            if(triggerManager != null) return triggerManager.pb.TotalSeconds;
            else return 0;
        }

        public static HKTimer instance { get; private set; }

        public GameObject gameObject { get; private set; }
        public Timer timer { get; private set; }
        public TriggerManager triggerManager { get; private set; }

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() {
            if(instance != null) {
                return;
            }
            instance = this;
            gameObject = new GameObject();

            timer = gameObject.AddComponent<Timer>();
            timer.InitDisplay();
            timer.ShowDisplay(settings.showTimer);

            triggerManager = gameObject.AddComponent<TriggerManager>().Initialize(timer);
            triggerManager.InitDisplay();
            if(System.Enum.TryParse<TriggerManager.TriggerPlaceType>(settings.trigger, out var t)) {
                triggerManager.triggerPlaceType = t;
            } else {
                LogError($"Invalid trigger name {settings.trigger}");
            }

            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(gameObject);
        }

        public void Unload() {
            this.timer.UnloadHooks();
            GameObject.DestroyImmediate(gameObject);
            USceneManager.activeSceneChanged -= SceneChanged;
            HKTimer.instance = null;
        }

        private void SceneChanged(Scene from, Scene to) {
            triggerManager.SpawnTriggers();
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu) {
            return new MenuBuilder(UIManager.instance.UICanvas.gameObject, "HKTimerMenu")
                .CreateTitle("HKTimer", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .CreateAutoMenuNav()
                .AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => {
                        c.AddHorizontalOption(
                            "ShowTimerOption",
                            new HorizontalOptionConfig {
                                Label = "Show Timer",
                                Options = new string[] { "Off", "On" },
                                ApplySetting = (_, i) => {
                                    settings.showTimer = i == 1;
                                    if(HKTimer.instance != null) {
                                        HKTimer.instance.timer.ShowDisplay(settings.showTimer);
                                    }
                                },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(settings.showTimer ? 1 : 0),
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                Style = HorizontalOptionStyle.VanillaStyle
                            },
                            out var showTimerOption
                        ).AddMenuButton(
                            "ResetBestButton",
                            new MenuButtonConfig {
                                Label = "Reset Personal Best",
                                SubmitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.ResetPB();
                                },
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        ).AddHorizontalOption(
                            "TriggerTypeOption",
                            new HorizontalOptionConfig {
                                Label = "Trigger Type",
                                Options = new string[] { "Collision", "Movement", "Scene" },
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                ApplySetting = (_, i) => {
                                    var trigger = i switch {
                                        0 => TriggerManager.TriggerPlaceType.Collision,
                                        1 => TriggerManager.TriggerPlaceType.Movement,
                                        2 => TriggerManager.TriggerPlaceType.Scene,
                                        _ => default // shouldn't ever happen
                                    };
                                    if(HKTimer.instance != null) {
                                        HKTimer.instance.triggerManager.triggerPlaceType = trigger;
                                    }
                                    settings.trigger = trigger.ToString();
                                },
                                RefreshSetting = (s, _) => {
                                    if(System.Enum.TryParse(settings.trigger, out TriggerManager.TriggerPlaceType t)) {
                                        s.optionList.SetOptionTo((int) t);
                                    }
                                }
                            },
                            out var triggerTypeOption
                        ).AddMenuButton(
                            "LoadTriggersButton",
                            new MenuButtonConfig {
                                Label = "Load Triggers",
                                SubmitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.LoadTriggers();
                                },
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        ).AddMenuButton(
                            "SaveTriggersButton",
                            new MenuButtonConfig {
                                Label = "Save Triggers",
                                SubmitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.SaveTriggers();
                                },
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        );
                        // should be guaranteed from `MenuBuilder.AddContent`
                        if(c.layout is RegularGridLayout layout) {
                            var l = layout.ItemAdvance;
                            l.x = new RelLength(750f);
                            layout.ChangeColumns(2, 0.5f, l, 0.5f);
                        }
                        c.AddKeybind(
                            "PauseKeybind",
                            settings.keybinds.pause,
                            new KeybindConfig {
                                Label = "Pause",
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                            }
                        ).AddKeybind(
                            "ResetKeybind",
                            settings.keybinds.reset,
                            new KeybindConfig {
                                Label = "Reset",
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                            }
                        ).AddKeybind(
                            "SetStartKeybind",
                            settings.keybinds.setStart,
                            new KeybindConfig {
                                Label = "Set Start",
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                            }
                        ).AddKeybind(
                            "SetStartKeybind",
                            settings.keybinds.setEnd,
                            new KeybindConfig {
                                Label = "Set End",
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                            }
                        );
                        showTimerOption.GetComponent<MenuSetting>().RefreshValueFromGameSettings();
                        triggerTypeOption.GetComponent<MenuSetting>().RefreshValueFromGameSettings();
                    }
                )
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )),
                    c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig {
                            Label = "Back",
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        }
                    )
                )
                .Build();
        }
    }
}