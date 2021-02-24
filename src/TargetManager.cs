using System;
using UnityEngine;
using System.IO;

namespace HKTimer
{
    public class TargetManager : MonoBehaviour
    {

        private PlayerPosTriggerSave start;
        private PlayerPosTriggerSave end;

        public void SpawnTriggers(string scene)
        {
            if (start.scene.Equals(scene))
            {
                this.CreateTrigger(
                    new Vector3(start.x, start.y),
                    "hktimer/start",
                    () => HKTimer.instance.frameCount.timerActive = true,
                    () => { },
                    Color.green
                );
            }
            if (end.scene.Equals(scene))
            {
                this.CreateTrigger(
                    new Vector3(end.x, end.y),
                    "hktimer/end",
                    () => HKTimer.instance.frameCount.timerActive = false,
                    () => { },
                    Color.red
                );
            }
        }

        public void Start()
        {
            Modding.Logger.Log("[HKTimer] Started target manager");
            LoadTriggers();
        }

        public void Update()
        {
            // there was a log here
            if (Input.GetKeyDown(HKTimer.instance.settings.set_start))
            {
                Modding.Logger.Log("[HKTimer] Created start at " + HeroController.instance.transform.position.ToString());
                {
                    var x = GameObject.Find("hktimer/start");
                    if (x != null) GameObject.Destroy(x);
                }
                this.CreateTrigger(
                    HeroController.instance.transform.position,
                    "hktimer/start",
                    () => HKTimer.instance.frameCount.timerActive = true,
                    () => { },
                    Color.green
                );
                this.start = new PlayerPosTriggerSave()
                {
                    scene = GameManager.instance.sceneName,
                    x = HeroController.instance.transform.position.x,
                    y = HeroController.instance.transform.position.y,
                };
            }
            if (Input.GetKeyDown(HKTimer.instance.settings.set_end))
            {
                Modding.Logger.Log("[HKTimer] Created end at " + HeroController.instance.transform.position.ToString());
                {
                    var x = GameObject.Find("hktimer/end");
                    if (x != null) GameObject.Destroy(x);
                }
                this.CreateTrigger(
                    HeroController.instance.transform.position,
                    "hktimer/end",
                    () => HKTimer.instance.frameCount.timerActive = false,
                    () => { },
                    Color.red
                );
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
            {
                var x = GameObject.Find("hktimer/start");
                if (x != null) GameObject.Destroy(x);
            }
            {
                var x = GameObject.Find("hktimer/end");
                if (x != null) GameObject.Destroy(x);
            }
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
            this.SpawnTriggers(GameManager.instance.sceneName);
        }

        private void SaveTriggers()
        {
            Modding.Logger.Log("[HKTimer] saving start " + start);
            Modding.Logger.Log("[HKTimer] saving end " + end);
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