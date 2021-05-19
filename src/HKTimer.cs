using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;
using Modding.Menu;
using Modding.Menu.Config;

namespace HKTimer {
    public class HKTimer : Mod, ITogglableMod, ICustomMenuMod {
        public static Settings settings { get; set; } = new Settings();
        public override ModSettings GlobalSettings {
            get => settings;
            set {
                if(value is Settings s) {
                    s.keybinds.SetDefaultBinds();
                    settings = s;
                }
            }
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
                                label = "Show Timer",
                                options = new string[] { "Off", "On" },
                                applySetting = (_, i) => {
                                    settings.showTimer = i == 1;
                                    if(HKTimer.instance != null) {
                                        HKTimer.instance.timer.ShowDisplay(settings.showTimer);
                                    }
                                },
                                refreshSetting = (s, _) => s.optionList.SetOptionTo(settings.showTimer ? 1 : 0),
                                cancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                style = HorizontalOptionStyle.vanillaStyle
                            },
                            out var showTimerButton
                        ).AddMenuButton(
                            "ResetBestButton",
                            new MenuButtonConfig {
                                label = "Reset Personal Best",
                                submitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.ResetPB();
                                },
                                cancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                style = MenuButtonStyle.vanillaStyle
                            }
                        ).AddHorizontalOption(
                            "TriggerTypeOption",
                            new HorizontalOptionConfig {
                                label = "Trigger Type",
                                options = new string[] { "Collision", "Movement", "Scene" },
                                applySetting = (_, i) => {
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
                                }
                            }
                        ).AddMenuButton(
                            "LoadTriggersButton",
                            new MenuButtonConfig {
                                label = "Load Triggers",
                                submitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.LoadTriggers();
                                },
                                cancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                style = MenuButtonStyle.vanillaStyle
                            }
                        ).AddMenuButton(
                            "SaveTriggersButton",
                            new MenuButtonConfig {
                                label = "Save Triggers",
                                submitAction = _ => {
                                    if(HKTimer.instance != null) HKTimer.instance.triggerManager.SaveTriggers();
                                },
                                cancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                                style = MenuButtonStyle.vanillaStyle
                            }
                        );
                        showTimerButton.GetComponent<MenuSetting>().RefreshValueFromGameSettings();
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
                            label = "Back",
                            cancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            submitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            style = MenuButtonStyle.vanillaStyle,
                            proceed = true
                        }
                    )
                )
                .Build();
        }
    }
}