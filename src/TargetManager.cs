using System;
using System.Runtime.CompilerServices;
using Modding;
using UnityEngine;

namespace FrameDisplay
{
    public class TargetManager : MonoBehaviour
    {
        private GameObject start;
        private GameObject end;

        public void Test()
        {
            Modding.Logger.Log("[FrameDisplay] create goal with " + FrameDisplay.Instance.settings.SetGoal);
        }

        public void Update()
        {
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.SetGoal))
            {
                if (this.end != null)
                {
                    GameObject.Destroy(this.end);
                }
                this.end = this.CreateTrigger(
                    HeroController.instance.transform.position,
                    "tp",
                    () => FrameDisplay.Instance.frameCount.timerActive = false,
                    () => {}
                );
                Modding.Logger.Log("[FrameDisplay] Created goal at " + HeroController.instance.transform.position.ToString());
            }
            if (Input.GetKeyDown(FrameDisplay.Instance.settings.SetStart))
            {
                if (this.start != null)
                {
                    GameObject.Destroy(this.start);
                }
                this.start = this.CreateTrigger(
                    HeroController.instance.transform.position,
                    "tp",
                    () => FrameDisplay.Instance.frameCount.timerActive = true,
                    () => {}
                );
                Modding.Logger.Log("[FrameDisplay] Created start at " + HeroController.instance.transform.position.ToString());
            }
        }

        private GameObject CreateTrigger(Vector3 pos, string name, Action onEnter, Action onExit)
        {
            GameObject gameObject = TargetManager.CreatePlane(new Vector3[]
            {
                new Vector3(pos.x - 0.1f, pos.y - 0.1f),
                new Vector3(pos.x - 0.1f, pos.y + 0.1f),
                new Vector3(pos.x + 0.1f, pos.y - 0.1f),
                new Vector3(pos.x + 0.1f, pos.y + 0.1f)
            }, name, new Color?(Color.black));
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

        private class PlayerPosTrigger : MonoBehaviour
        {
            public Action onEnter;
            public Action onExit;

            private void OnTriggerEnter2D(Collider2D other)
            {
                if(other.gameObject == HeroController.instance.gameObject) {
                    onEnter();
                }
            }

            private void OnTriggerExit2D(Collider2D other) {
                if(other.gameObject == HeroController.instance.gameObject) {
                    onExit();
                }
            }
        }
    }
}