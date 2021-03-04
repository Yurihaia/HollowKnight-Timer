using System;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace HKTimer
{
    public class TargetManager : MonoBehaviour
    {

        private TimeSpan pb = TimeSpan.Zero;
        private TimeSpan pbDelta = TimeSpan.Zero;
        private bool runningSegment = false;

        private List<Trigger> triggers = new List<Trigger>();
        // Special start and end triggers you can place
        // only these ones will get changed using the keybinds
        private Trigger start;
        private Trigger end;

        private Text pbDisplay;
        private GameObject pbDisplayObject;

        private Text pbDeltaDisplay;
        private GameObject pbDeltaDisplayObject;

        public void OnTimerPauseManual()
        {

        }

        public void OnTimerResetManual()
        {
            this.runningSegment = false;
            pbDeltaDisplayObject.SetActive(false);
        }

        public void OnDestroy()
        {
            this.start?.Destroy();
            this.end?.Destroy();
            this.triggers?.ForEach(x => x.Destroy());
            GameObject.Destroy(pbDeltaDisplayObject);
            GameObject.Destroy(pbDisplayObject);
        }

        public void ShowDisplay()
        {
            if (pbDisplayObject != null)
            {
                GameObject.DestroyImmediate(pbDisplayObject);
            }
            if (pbDeltaDisplayObject != null)
            {
                GameObject.DestroyImmediate(pbDeltaDisplayObject);
            }
            pbDisplayObject = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            pbDeltaDisplayObject = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData pbRd = new CanvasUtil.RectData(
                new Vector2(300, 75),
                new Vector2(0.5f, 0.5f),
                // offset x = 0.15f - 0.115f
                // offset y = 0.1f - 0.05f
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.1f, HKTimer.instance.settings.timerAnchorY - 0.05f),
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.1f, HKTimer.instance.settings.timerAnchorY - 0.05f)
            );
            CanvasUtil.RectData pbDeltaRd = new CanvasUtil.RectData(
                new Vector2(200, 50),
                new Vector2(0.5f, 0.5f),
                // offset x = 0.15f - 0.115f
                // offset y = 0.1f - 0.1f
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.1f, HKTimer.instance.settings.timerAnchorY - 0.1f),
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.1f, HKTimer.instance.settings.timerAnchorY - 0.1f)
            );
            this.pb = TimeSpan.Zero;
            pbDisplay = CanvasUtil.CreateTextPanel(
                pbDisplayObject,
                this.PbText(),
                30,
                TextAnchor.LowerRight,
                pbRd
            ).GetComponent<Text>();
            pbDeltaDisplay = CanvasUtil.CreateTextPanel(
                pbDeltaDisplayObject,
                this.PbDeltaText(),
                20,
                TextAnchor.LowerRight,
                pbDeltaRd
            ).GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(pbDisplayObject);
            UnityEngine.Object.DontDestroyOnLoad(pbDeltaDisplayObject);
            pbDeltaDisplayObject.SetActive(false);
            Modding.Logger.Log("[HKTimer] Created display");
        }

        private string PbText()
        {
            return string.Format(
                "PB {0,3}:{1:D2}.{2:D3}",
                Math.Floor(this.pb.TotalMinutes),
                this.pb.Seconds,
                this.pb.Milliseconds
            );
        }

        private string PbDeltaText()
        {
            var dur = this.pbDelta.Duration();
            return string.Format(
                "{0}{1}:{2:D2}.{3:D3}",
                this.pbDelta < TimeSpan.Zero ? "-": "+",
                Math.Floor(dur.TotalMinutes),
                dur.Seconds,
                dur.Milliseconds
            );
        }

        public void UpdatePB()
        {
            var time = HKTimer.instance.frameCount.time;
            if(HKTimer.instance.frameCount.timerActive)
            {
                if(this.pb == null || this.pb == TimeSpan.Zero) // No PB yet
                {
                    this.pb = time;
                    this.pbDisplay.text = this.PbText();
                    this.pbDelta = TimeSpan.Zero;
                    this.pbDeltaDisplayObject.SetActive(false);
                }
                else if(this.pb > time) // Beat PB
                {
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
                    UnityEngine.Object.DontDestroyOnLoad(pbDeltaDisplayObject);
                }
                else // Lost PB
                {
                    this.pbDelta = time - this.pb;
                    this.pbDeltaDisplay.text = this.PbDeltaText();
                    this.pbDeltaDisplayObject.SetActive(true);
                    UnityEngine.Object.DontDestroyOnLoad(pbDeltaDisplayObject);
                }
            }
        }

        public void SpawnTriggers(string scene)
        {
            this.start?.Spawn(scene, this);
            this.end?.Spawn(scene, this);
            this.triggers?.ForEach(x => x.Spawn(scene, this));
        }

        public void Start()
        {
            Modding.Logger.Log("[HKTimer] Started target manager");
            LoadTriggers();
        }

        public void Update()
        {
            if (Input.GetKeyDown(HKTimer.instance.settings.set_start))
            {
                this.start?.Destroy();
                this.start = new CollisionTrigger()
                {
                    scene = GameManager.instance.sceneName,
                    logic = new JValue("segment_start"),
                    color = "green",
                    start = HeroController.instance.transform.position - new Vector3(0.1f, 0.1f),
                    end = HeroController.instance.transform.position + new Vector3(0.1f, 0.1f),
                };
                this.start.Spawn(GameManager.instance.sceneName, this);
                this.pb = TimeSpan.Zero;
                this.pbDisplay.text = this.PbText();
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.set_end))
            {
                this.end?.Destroy();
                this.end = new CollisionTrigger()
                {
                    scene = GameManager.instance.sceneName,
                    logic = new JValue("segment_end"),
                    color = "red",
                    start = HeroController.instance.transform.position - new Vector3(0.1f, 0.1f),
                    end = HeroController.instance.transform.position + new Vector3(0.1f, 0.1f),
                };
                this.end.Spawn(GameManager.instance.sceneName, this);
                this.pb = TimeSpan.Zero;
                this.pbDisplay.text = this.PbText();
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.load_triggers))
            {
                LoadTriggers();
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.save_triggers))
            {
                SaveTriggers();
            }
        }

        private void LoadTriggers()
        {
            try
            {
                Modding.Logger.Log("[HKTimer] Loading triggers");
                if (File.Exists(Application.persistentDataPath + "/hktimer_triggers.json"))
                {
                    var triggers = JsonConvert.DeserializeObject<TriggerSaveFile>(File.ReadAllText(
                        Application.persistentDataPath + "/hktimer_triggers.json"
                    ));
                    // Destroy all triggers
                    this.triggers?.ForEach(x => x.Destroy());
                    this.start?.Destroy();
                    this.end?.Destroy();

                    this.triggers = triggers.other?.ConvertAll<Trigger>(x => x.ToTrigger())  ?? new List<Trigger>();
                    this.start = triggers.start.ToTrigger();
                    this.end = triggers.end.ToTrigger();
                    this.pbDisplay.text = this.PbText();
                    this.SpawnTriggers(GameManager.instance.sceneName);
                }
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }

        private void SaveTriggers()
        {
            try
            {
                Modding.Logger.Log("[HKTimer] Saving triggers");
                File.WriteAllText(
                    Application.persistentDataPath + "/hktimer_triggers.json",
                    JsonConvert.SerializeObject(
                        new TriggerSaveFile()
                        {
                            pb_ticks = this.pb.Ticks,
                            start = TriggerSave.FromTrigger(this.start),
                            end = TriggerSave.FromTrigger(this.end),
                            other = this.triggers?.ConvertAll(x => TriggerSave.FromTrigger(x)) ?? new List<TriggerSave>(),
                        },
                        Formatting.Indented
                    )
                );
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }

        public void TriggerLogic(JToken logic)
        {
            switch(logic) {
                case JArray arr:
                    // Array of logic items, handle each in order
                    foreach(var c in arr) {
                        this.TriggerLogic(c);
                    }
                    break;
                case JValue val:
                    // Specific logic preset
                    if(val.Value is string s) {
                        switch(s) {
                            case "segment_start":
                                if(!HKTimer.instance.frameCount.timerActive) {
                                    this.runningSegment = true;
                                    HKTimer.instance.frameCount.time = TimeSpan.Zero;
                                    HKTimer.instance.frameCount.timerActive = true;
                                }
                                break;
                            case "segment_end":
                                if(this.runningSegment) {
                                    this.UpdatePB();
                                    HKTimer.instance.frameCount.timerActive = false;
                                    this.runningSegment = false;
                                }
                                break;
                            case "timer_reset":
                                HKTimer.instance.frameCount.time = TimeSpan.Zero;
                                break;
                            case "timer_pause":
                                HKTimer.instance.frameCount.timerActive = false;
                                break;
                            case "timer_resume":
                                HKTimer.instance.frameCount.timerActive = true;
                                break;
                            case "timer_toggle":
                                HKTimer.instance.frameCount.timerActive ^= true;
                                break;
                            default:
                                Modding.Logger.LogError("[HKTimer] Invalid logic preset '" + s + "'");
                                break;
                        }
                    } else {
                        Modding.Logger.LogError("[HKTimer] Invalid logic `" + val.ToString() + "`");
                    }
                    break;
                default:
                    Modding.Logger.LogError("[HKTimer] Invalid logic `" + logic.ToString() + "`");
                    break;
            }
        }
    }

    public class TriggerSaveFile
    {
        public long pb_ticks;
        public List<TriggerSave> other;
        public TriggerSave start;
        public TriggerSave end;
    }

    public class TriggerSave
    {
        public string trigger_type;
        public JObject trigger_data;

        public Trigger ToTrigger()
        {
            switch (this.trigger_type)
            {
                case "collision":
                    return this.trigger_data.ToObject<CollisionTrigger>();
                case "movement":
                    return this.trigger_data.ToObject<MovementTrigger>();
                default:
                    throw new Exception("Invalid trigger_type '" + this.trigger_type + "'");
            }
        }

        public static TriggerSave FromTrigger(Trigger x)
        {
            var obj = JObject.FromObject(x);
            var tag = x switch
            {
                CollisionTrigger _ => "collision",
                MovementTrigger _ => "movement",
                _ => throw new Exception("Unknown trigger " + x.GetType().Name)
            };
            return new TriggerSave()
            {
                trigger_data = obj,
                trigger_type = tag
            };
        }
    }

    public abstract class Trigger
    {
        public JToken logic;
        public string scene;

        public abstract void Spawn(string currentScene, TargetManager tm);
        public abstract void Destroy();
    }

    public class CollisionTrigger : Trigger
    {
        [JsonConverter(typeof(JsonVec2Converter))]
        public Vector2 start;
        [JsonConverter(typeof(JsonVec2Converter))]
        public Vector2 end;

        public string color;

        [JsonIgnore]
        private GameObject go;

        public override void Spawn(string currentScene, TargetManager tm)
        {
            if (this.scene == currentScene)
            {
                this.Destroy();
                Color color;
                if(!ColorUtility.TryParseHtmlString(this.color, out color))
                {
                    Modding.Logger.LogError("Invalid color `" + color + "`.");
                    color = Color.black;
                }
                this.go = CreateTrigger(
                    this.start,
                    this.end,
                    "hktimer/trigger/collision",
                    () => {
                        if(this.logic != null) tm.TriggerLogic(this.logic);
                    },
                    () => { },
                    color
                );
            }
        }

        public override void Destroy()
        {
            if(this.go != null) GameObject.Destroy(this.go);
        }

        public static GameObject CreateTrigger(Vector2 start, Vector2 end, string name, Action onEnter, Action onExit, Color c)
        {
            GameObject gameObject = CollisionTrigger.CreatePlane(new Vector3[]
            {
                new Vector3(start.x, start.y),
                new Vector3(start.x, end.y),
                new Vector3(end.x, start.y),
                new Vector3(end.x, end.y)
            }, name, c);
            PlayerCollisionHandler playerPosTrigger = gameObject.AddComponent<PlayerCollisionHandler>();
            playerPosTrigger.onEnter = onEnter;
            playerPosTrigger.onExit = onExit;
            gameObject.SetActive(true);
            return gameObject;
        }
        // Thank you 56
        // but what the fuck
        private static GameObject CreatePlane(Vector3[] vert, string name = "Plane", Color? c = null)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.AddComponent<MeshFilter>().mesh = CreateMesh(vert);
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material.shader = Shader.Find("Particles/Multiply");
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c ?? Color.white);
            tex.Apply();
            meshRenderer.material.mainTexture = tex;
            meshRenderer.material.color = Color.white;
            gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
            gameObject.SetActive(true);
            return gameObject;
        }

        private static Mesh CreateMesh(Vector3[] vertices)
        {
            Mesh mesh = new Mesh
            {
                name = "ScriptedMesh",
                vertices = vertices,
                uv = new Vector2[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f)
                },
                triangles = new int[] { 0, 1, 2, 1, 2, 3 }
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private class PlayerCollisionHandler : MonoBehaviour
        {
            public Action onEnter;
            public Action onExit;

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (other.gameObject == HeroController.instance.gameObject)
                {
                    onEnter();
                }
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                if (other.gameObject == HeroController.instance.gameObject)
                {
                    onExit();
                }
            }
        }
    }

    public class MovementTrigger : Trigger
    {
        [JsonIgnore]
        private GameObject go;

        public override void Spawn(string currentScene, TargetManager tm)
        {

        }

        public override void Destroy()
        {
        }

        private class MovementHandler : MonoBehaviour
        {
            public void Update()
            {
                // idk i'll work on this later :)
            }
        }
    }

    public class JsonVec2Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (Vector2) value;
            JObject j = new JObject {{"x", vec.x}, {"y", vec.y}};
            j.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return existingValue;
        }
        public override bool CanWrite => true;
        public override bool CanRead => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }
    }
}