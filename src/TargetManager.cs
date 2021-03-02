using System;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Modding;

namespace HKTimer
{
    public class TargetManager : MonoBehaviour
    {

        private TimeSpan pb;
        private PlayerPosTriggerSave start;
        private PlayerPosTriggerSave end;

        private Text pbDisplay;
        private GameObject pbDisplayObject;

        public void ShowDisplay()
        {
            if(pbDisplayObject != null) {
                GameObject.Destroy(pbDisplayObject);
            }
            Modding.Logger.Log("[HKTimer] Creating display");
            pbDisplayObject = CanvasUtil.CreateCanvas(UnityEngine.RenderMode.ScreenSpaceOverlay, 100);
            CanvasUtil.CreateFonts();
            CanvasUtil.RectData timerRd = new CanvasUtil.RectData(
                new Vector2(300, 75),
                new Vector2(0.5f, 0.5f),
                // offset x = 0.15f - 0.115f
                // offset y = 0.1f - 0.05f
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.035f, HKTimer.instance.settings.timerAnchorY - 0.05f),
                new Vector2(HKTimer.instance.settings.timerAnchorX - 0.035f, HKTimer.instance.settings.timerAnchorY - 0.05f)
            );
            this.pb = TimeSpan.Zero;
            pbDisplay = CanvasUtil.CreateTextPanel(pbDisplayObject, this.PbText(), 30, TextAnchor.LowerLeft, timerRd).GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(pbDisplayObject);
        }

        private string PbText() {
            return string.Format(
                "PB {0,3}:{1:D2}.{2:D3}",
                Math.Floor(this.pb.TotalMinutes),
                this.pb.Seconds,
                this.pb.Milliseconds
            );
        }

        private void CreateEnd(Vector3 pos)
        {
            {
                var x = GameObject.Find("hktimer/end");
                if (x != null) GameObject.Destroy(x);
            }
            this.CreateTrigger(
                pos,
                "hktimer/end",
                () => {
                    HKTimer.instance.frameCount.timerActive = false;
                    var time = HKTimer.instance.frameCount.time;
                    if(this.pb == null || this.pb == TimeSpan.Zero || this.pb > time) {
                        this.pb = time;
                        this.pbDisplay.text = this.PbText();
                    }
                },
                () => { },
                Color.red
            );
        }

        private void CreateStart(Vector3 pos) {
            {
                var x = GameObject.Find("hktimer/start");
                if (x != null) GameObject.Destroy(x);
            }
            this.CreateTrigger(
                pos,
                "hktimer/start",
                () => HKTimer.instance.frameCount.timerActive = true,
                () => { },
                Color.green
            );
        }

        public void SpawnTriggers(string scene)
        {
            
            {
                var x = GameObject.Find("hktimer/end");
                if (x != null) GameObject.Destroy(x);
            }
            {
                var x = GameObject.Find("hktimer/start");
                if (x != null) GameObject.Destroy(x);
            }
            if (start?.scene != null && start.scene.Equals(scene))
            {
                CreateStart(new Vector3(start.x, start.y));
            }
            if (end?.scene != null && end.scene.Equals(scene))
            {
                CreateEnd(new Vector3(end.x, end.y));
            }
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
                CreateStart(HeroController.instance.transform.position);
                this.start = new PlayerPosTriggerSave()
                {
                    scene = GameManager.instance.sceneName,
                    x = HeroController.instance.transform.position.x,
                    y = HeroController.instance.transform.position.y,
                };
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.set_end))
            {
                CreateEnd(HeroController.instance.transform.position);
                this.end = new PlayerPosTriggerSave()
                {
                    scene = GameManager.instance.sceneName,
                    x = HeroController.instance.transform.position.x,
                    y = HeroController.instance.transform.position.y,
                };
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
            var triggers = JsonUtility.FromJson<Triggers>(File.ReadAllText(
                Application.persistentDataPath + "/hktimer_triggers.json"
            ));
            this.start = new PlayerPosTriggerSave()
            {
                scene = triggers.startScene,
                x = triggers.startX,
                y = triggers.startY,
            };
            this.end = new PlayerPosTriggerSave()
            {
                scene = triggers.endScene,
                x = triggers.endX,
                y = triggers.endY,

            };
            this.pb = new TimeSpan(triggers.pbTicks);
            this.pbDisplay.text = this.PbText();
            this.SpawnTriggers(GameManager.instance.sceneName);
        }

        private void SaveTriggers()
        {
            try
            {
                File.WriteAllText(
                    Application.persistentDataPath + "/hktimer_triggers.json",
                    JsonUtility.ToJson(new Triggers()
                    {
                        startScene = this.start.scene,
                        startX = this.start.x,
                        startY = this.start.y,
                        endScene = this.end.scene,
                        endX = this.end.x,
                        endY = this.end.y,
                        pbTicks = this.pb.Ticks,
                    }, true)
                );
            }
            catch (Exception) { }
        }

        private GameObject CreateTrigger(Vector3 pos, string name, Action onEnter, Action onExit, Color c)
        {
            GameObject gameObject = TargetManager.CreatePlane(new Vector3[]
            {
                new Vector3(pos.x - 0.1f, pos.y - 0.1f),
                new Vector3(pos.x - 0.1f, pos.y + 0.1f),
                new Vector3(pos.x + 0.1f, pos.y - 0.1f),
                new Vector3(pos.x + 0.1f, pos.y + 0.1f)
            }, name, c);
            TargetManager.PlayerPosTrigger playerPosTrigger = gameObject.AddComponent<TargetManager.PlayerPosTrigger>();
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
            gameObject.AddComponent<MeshFilter>().mesh = TargetManager.CreateMesh(vert);
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

        [Serializable]
        public class PlayerPosTriggerSave
        {
            public string scene;
            public float x;
            public float y;
        }

        private class PlayerPosTrigger : MonoBehaviour
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
}