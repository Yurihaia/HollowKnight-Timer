using System;
using Newtonsoft.Json;
using UnityEngine;
using Modding.Converters;

namespace HKTimer {
    namespace Triggers {
        public class CollisionTrigger : Trigger {
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 start;
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 end;

            public string color;

            [JsonIgnore]
            private GameObject go;

            public const string name = "collision";
            public override string Name => name;

            public override void Spawn(TriggerManager tm) {
                if(this.scene == GameManager.instance.sceneName) {
                    this.Destroy(tm);
                    Color color;
                    if(!ColorUtility.TryParseHtmlString(this.color, out color)) {
                        HKTimer.instance.LogError("Invalid color `" + color + "`.");
                        color = Color.black;
                    }
                    this.go = CreateTrigger(
                        this.start,
                        this.end,
                        "hktimer/trigger/collision",
                        () => {
                            if(this.logic != null) HKTimer.instance.triggerManager.ExecLogic(this.logic);
                        },
                        color
                    );
                }
            }
            public override void Destroy(TriggerManager tm) {
                if(this.go != null) GameObject.Destroy(this.go);
            }
            private static GameObject CreateTrigger(Vector2 start, Vector2 end, string name, Action onEnter, Color c) {
                GameObject gameObject = CollisionTrigger.CreatePlane(new Vector3[]
                {
                new Vector3(start.x, start.y),
                new Vector3(start.x, end.y),
                new Vector3(end.x, start.y),
                new Vector3(end.x, end.y)
                }, name, c);
                PlayerCollisionHandler playerPosTrigger = gameObject.AddComponent<PlayerCollisionHandler>();
                playerPosTrigger.onEnter = onEnter;
                gameObject.SetActive(true);
                return gameObject;
            }
            private static GameObject CreatePlane(Vector3[] vert, string name = "Plane", Color? c = null) {
                GameObject gameObject = new GameObject(name);
                gameObject.AddComponent<MeshFilter>().mesh = CreateMesh(vert);
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, c ?? Color.white);
                tex.Apply();
                meshRenderer.material.mainTexture = tex;
                meshRenderer.material.color = c ?? Color.white;
                gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                gameObject.SetActive(true);
                return gameObject;
            }
            private static Mesh CreateMesh(Vector3[] vertices) {
                Mesh mesh = new Mesh {
                    name = "ScriptedMesh",
                    vertices = vertices,
                    uv = new Vector2[] {
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

            private class PlayerCollisionHandler : MonoBehaviour {
                public Action onEnter;
                private void OnTriggerEnter2D(Collider2D other) {
                    if(other.gameObject == HeroController.instance.gameObject) {
                        onEnter();
                    }
                }
            }
        }
    }
}