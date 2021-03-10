using System;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HKTimer.Triggers;

namespace HKTimer {
    public class TriggerManager : MonoBehaviour {

        public TimeSpan pb { get; set; } = TimeSpan.Zero;
        public TimeSpan pbDelta { get; set; } = TimeSpan.Zero;
        public bool runningSegment { get; set; } = false;

        private List<Trigger> triggers = new List<Trigger>();
        // Special start and end triggers you can place
        // only these ones will get changed using the keybinds
        private Trigger start;
        private Trigger end;

        private Text pbDisplay;
        private GameObject pbDisplayObject;

        private Text pbDeltaDisplay;
        private GameObject pbDeltaDisplayObject;

        public Dictionary<string, Type> triggerTypes { get; } = new Dictionary<string, Type>();

        // MonoBehaviours
        public Timer timer { get; private set; }

        public void TimerReset() {
            this.runningSegment = false;
            this.pbDeltaDisplayObject.SetActive(false);
        }

        public void ShowDisplay() {
            if(this.pbDisplayObject != null) {
                GameObject.DestroyImmediate(this.pbDisplayObject);
            }
            if(this.pbDeltaDisplayObject != null) {
                GameObject.DestroyImmediate(this.pbDeltaDisplayObject);
            }
            this.pbDisplayObject = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            this.pbDeltaDisplayObject = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData pbRd = new CanvasUtil.RectData(
                new Vector2(300, 75),
                new Vector2(0.5f, 0.5f),
                // offset x = 0.15f - 0.115f
                // offset y = 0.1f - 0.05f
                new Vector2(HKTimer.settings.timerAnchorX - 0.1f, HKTimer.settings.timerAnchorY - 0.05f),
                new Vector2(HKTimer.settings.timerAnchorX - 0.1f, HKTimer.settings.timerAnchorY - 0.05f)
            );
            CanvasUtil.RectData pbDeltaRd = new CanvasUtil.RectData(
                new Vector2(200, 50),
                new Vector2(0.5f, 0.5f),
                // offset x = 0.15f - 0.115f
                // offset y = 0.1f - 0.1f
                new Vector2(HKTimer.settings.timerAnchorX - 0.1f, HKTimer.settings.timerAnchorY - 0.1f),
                new Vector2(HKTimer.settings.timerAnchorX - 0.1f, HKTimer.settings.timerAnchorY - 0.1f)
            );
            this.pb = TimeSpan.Zero;
            this.pbDisplay = CanvasUtil.CreateTextPanel(
                pbDisplayObject,
                this.PbText(),
                30,
                TextAnchor.LowerRight,
                pbRd
            ).GetComponent<Text>();
            this.pbDeltaDisplay = CanvasUtil.CreateTextPanel(
                pbDeltaDisplayObject,
                this.PbDeltaText(),
                20,
                TextAnchor.LowerRight,
                pbDeltaRd
            ).GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(this.pbDisplayObject);
            UnityEngine.Object.DontDestroyOnLoad(this.pbDeltaDisplayObject);
            this.pbDeltaDisplayObject.SetActive(false);
            Modding.Logger.Log("[HKTimer] Created display");
        }

        public TriggerManager Initialize(Timer timer) {
            this.timer = timer;
            this.timer.OnTimerReset += TimerReset;
            return this;
        }

        private string PbText() {
            return string.Format(
                "PB {0,3}:{1:D2}.{2:D3}",
                Math.Floor(this.pb.TotalMinutes),
                this.pb.Seconds,
                this.pb.Milliseconds
            );
        }
        private string PbDeltaText() {
            var dur = this.pbDelta.Duration();
            return string.Format(
                "{0}{1}:{2:D2}.{3:D3}",
                this.pbDelta < TimeSpan.Zero ? "-" : "+",
                Math.Floor(dur.TotalMinutes),
                dur.Seconds,
                dur.Milliseconds
            );
        }

        public void UpdatePB() {
            var time = this.timer.time;
            if(this.timer.timerActive) {
                if(this.pb == null || this.pb == TimeSpan.Zero) {
                    this.pb = time;
                    this.pbDisplay.text = this.PbText();
                    this.pbDelta = TimeSpan.Zero;
                    this.pbDeltaDisplayObject.SetActive(false);
                } else if(this.pb > time) {
                    this.pbDelta = time - this.pb;
                    this.pb = time;
                    this.pbDisplay.text = this.PbText();
                    this.pbDeltaDisplay.text = this.PbDeltaText();
                    this.pbDeltaDisplayObject.SetActive(true);
                    // fuck you unity :>
                    // apparently setting an object to active
                    // makes it need to have `DontDestroyOnLoad` called again
                    // except apparently only on some versions???
                    // idk man this worked so its gonna stay
                    UnityEngine.Object.DontDestroyOnLoad(this.pbDeltaDisplayObject);
                } else {
                    this.pbDelta = time - this.pb;
                    this.pbDeltaDisplay.text = this.PbDeltaText();
                    this.pbDeltaDisplayObject.SetActive(true);
                    UnityEngine.Object.DontDestroyOnLoad(this.pbDeltaDisplayObject);
                }
            }
        }

        public void SpawnTriggers() {
            this.start?.Spawn(this);
            this.end?.Spawn(this);
            this.triggers?.ForEach(x => x.Spawn(this));
        }

        public void Awake() {
            triggerTypes.Add(CollisionTrigger.name, typeof(CollisionTrigger));
            triggerTypes.Add(MovementTrigger.name, typeof(MovementTrigger));
            OnLogicPreset += (string p, ref bool s) => {
                var o = true;
                switch(p) {
                    case "segment_start":
                        if(!this.timer.timerActive) {
                            this.runningSegment = true;
                            this.timer.time = TimeSpan.Zero;
                            this.timer.timerActive = true;
                        }
                        break;
                    case "segment_end":
                        if(this.runningSegment) {
                            this.UpdatePB();
                            this.timer.timerActive = false;
                            this.runningSegment = false;
                        }
                        break;
                    case "timer_reset":
                        this.timer.time = TimeSpan.Zero;
                        break;
                    case "timer_pause":
                        this.timer.timerActive = false;
                        break;
                    case "timer_resume":
                        this.timer.timerActive = true;
                        break;
                    default:
                        o = false;
                        break;
                }
                if(o) s = true;
            };
        }
        public void Start() {
            Modding.Logger.Log("[HKTimer] Started target manager");
            LoadTriggers();
        }
        public void Update() {
            if(StringInputManager.GetKeyDown(HKTimer.settings.set_start)) {
                this.start?.Destroy(this);
                this.start = new CollisionTrigger() {
                    scene = GameManager.instance.sceneName,
                    logic = new JValue("segment_start"),
                    color = "green",
                    start = HeroController.instance.transform.position - new Vector3(0.1f, 0.1f),
                    end = HeroController.instance.transform.position + new Vector3(0.1f, 0.1f),
                };
                this.start.Spawn(this);
                this.pb = TimeSpan.Zero;
                this.pbDisplay.text = this.PbText();
            }
            if(StringInputManager.GetKeyDown(HKTimer.settings.set_end)) {
                this.end?.Destroy(this);
                this.end = new CollisionTrigger() {
                    scene = GameManager.instance.sceneName,
                    logic = new JValue("segment_end"),
                    color = "red",
                    start = HeroController.instance.transform.position - new Vector3(0.1f, 0.1f),
                    end = HeroController.instance.transform.position + new Vector3(0.1f, 0.1f),
                };
                this.end.Spawn(this);
                this.pb = TimeSpan.Zero;
                this.pbDisplay.text = this.PbText();
            }
            if(StringInputManager.GetKeyDown(HKTimer.settings.load_triggers)) {
                LoadTriggers();
            }
            if(StringInputManager.GetKeyDown(HKTimer.settings.save_triggers)) {
                SaveTriggers();
            }
        }
        public void OnDestroy() {
            this.start?.Destroy(this);
            this.end?.Destroy(this);
            this.triggers?.ForEach(x => x.Destroy(this));
            GameObject.Destroy(this.pbDeltaDisplayObject);
            GameObject.Destroy(this.pbDisplayObject);
            if(this.timer != null) this.timer.OnTimerReset -= TimerReset;
        }

        private void LoadTriggers() {
            try {
                Modding.Logger.Log("[HKTimer] Loading triggers");
                if(File.Exists(Application.persistentDataPath + "/hktimer_triggers.json")) {
                    var triggers = JsonConvert.DeserializeObject<TriggerSaveFile>(File.ReadAllText(
                        Application.persistentDataPath + "/hktimer_triggers.json"
                    ));
                    // Destroy all triggers
                    this.triggers?.ForEach(x => x.Destroy(this));
                    this.start?.Destroy(this);
                    this.end?.Destroy(this);

                    this.triggers = triggers.other?.ConvertAll<Trigger>(x => x.ToTrigger(this)) ?? new List<Trigger>();
                    this.start = triggers.start.ToTrigger(this);
                    this.end = triggers.end.ToTrigger(this);
                    this.pbDisplay.text = this.PbText();
                    this.SpawnTriggers();
                }
            } catch(Exception e) {
                Modding.Logger.LogError(e);
            }
        }
        private void SaveTriggers() {
            try {
                Modding.Logger.Log("[HKTimer] Saving triggers");
                File.WriteAllText(
                    Application.persistentDataPath + "/hktimer_triggers.json",
                    JsonConvert.SerializeObject(
                        new TriggerSaveFile() {
                            pb_ticks = this.pb.Ticks,
                            start = TriggerSave.FromTrigger(this.start),
                            end = TriggerSave.FromTrigger(this.end),
                            other = this.triggers?.ConvertAll(x => TriggerSave.FromTrigger(x)) ?? new List<TriggerSave>(),
                        },
                        Formatting.Indented
                    )
                );
            } catch(Exception e) {
                Modding.Logger.LogError(e);
            }
        }

        public delegate void LogicPresetDelegate(string preset, ref bool successful);
        public event LogicPresetDelegate OnLogicPreset;

        public void ExecLogic(JToken logic) {
            switch(logic) {
                case JArray v:
                    // Array of logic items, handle each in order
                    foreach(var c in v) {
                        ExecLogic(c);
                    }
                    break;
                case JValue v:
                    // Specific logic preset
                    if(v.Value is string s) {
                        var successful = false;
                        OnLogicPreset.Invoke(s, ref successful);
                        if(!successful) {
                            Modding.Logger.LogError("[HKTimer] Invalid logic preset `" + s + "`");
                        }
                    } else {
                        Modding.Logger.LogError("[HKTimer] Invalid logic `" + v.ToString() + "`");
                    }
                    break;
                case JObject v:
                    if(v["type"] is JValue { Value: "command" }) {
                        try {
                            var lcmd = v.ToObject<LogicCommand>();
                            Trigger trigger;
                            if(lcmd.trigger is JValue { Value: "start" }) {
                                trigger = this.start;
                            } else if(lcmd.trigger is JValue { Value: "end" }) {
                                trigger = this.end;
                            } else {
                                trigger = this.triggers[lcmd.trigger.ToObject<int>()];
                            }
                            trigger.TriggerCommand(lcmd.command, lcmd.data);
                        } catch(Exception e) {
                            Modding.Logger.LogError(e);
                        }
                    }
                    break;
                default:
                    Modding.Logger.LogError("[HKTimer] Invalid logic `" + logic.ToString() + "`");
                    break;
            }
        }

        private class LogicCommand {
            public string command;
            public JValue trigger;
            public JObject data;
        }
    }

    public class TriggerSaveFile {
        public long pb_ticks;
        public List<TriggerSave> other;
        public TriggerSave start;
        public TriggerSave end;
    }

    public class TriggerSave {
        public string trigger_type;
        public JObject trigger_data;


        public Trigger ToTrigger(TriggerManager tm) {
            Type ty;
            if(tm.triggerTypes.TryGetValue(this.trigger_type, out ty)) {
                return (Trigger) trigger_data.ToObject(ty);
            } else {
                throw new Exception("Unknown trigger type '" + this.trigger_type + "'");
            }
        }

        public static TriggerSave FromTrigger(Trigger x) {
            return new TriggerSave() {
                trigger_data = JObject.FromObject(x),
                trigger_type = x.Name
            };
        }
    }

    public abstract class Trigger {
        public JToken logic;
        public string scene;

        [JsonIgnore]
        public abstract string Name {
            get;
        }
        public abstract void Spawn(TriggerManager tm);
        public abstract void Destroy(TriggerManager tm);

        public virtual void TriggerCommand(string command, JObject data) { }
    }


    public class JsonVec2Converter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var vec = (Vector2) value;
            JObject j = new JObject { { "x", vec.x }, { "y", vec.y } };
            j.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            return existingValue;
        }
        public override bool CanWrite => true;
        public override bool CanRead => false;
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Vector2);
        }
    }
}