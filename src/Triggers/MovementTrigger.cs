using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HKTimer {
    namespace Triggers {
        public class MovementTrigger : Trigger {
            [JsonIgnore]
            private GameObject go;
            [JsonIgnore]
            private MovementHandler movementHandler;

            public const string name = "movement";
            public override string Name => name;

            public override void Spawn(TriggerManager tm) {
                if(this.scene == GameManager.instance.sceneName) {
                    this.go = new GameObject();
                    this.movementHandler = go.AddComponent<MovementHandler>().Initialize(this.logic, tm);
                }
            }
            public override void Destroy(TriggerManager tm) {
                if(go != null) GameObject.Destroy(go);
            }

            public override void TriggerCommand(string command, JObject data) {
                if(command == "enable") {
                    this.movementHandler.triggerEnabled = true;
                } else if(command == "disable") {
                    this.movementHandler.triggerEnabled = false;
                }
            }

            private class MovementHandler : MonoBehaviour {
                private TriggerManager tm;
                private JToken logic;
                public bool triggerEnabled = true;

                public MovementHandler Initialize(JToken logic, TriggerManager tm) {
                    this.tm = tm;
                    this.logic = logic;
                    return this;
                }

                public void Update() {
                    if(triggerEnabled) {
                        var hero_ctrl = GameManager.instance.hero_ctrl;
                        var moving = hero_ctrl.current_velocity != new Vector2(0, 0) ||
                            hero_ctrl.cState.casting ||
                            hero_ctrl.cState.attacking ||
                            (hero_ctrl.cState.freezeCharge || hero_ctrl.cState.superDashOnWall) ||
                            hero_ctrl.gameObject.LocateMyFSM("Dream Nail").ActiveStateName == "Charge";
                        if(moving) {
                            this.triggerEnabled = false;
                            HKTimer.instance.triggerManager.ExecLogic(this.logic);
                        }
                    }
                }
            }
        }
    }
}