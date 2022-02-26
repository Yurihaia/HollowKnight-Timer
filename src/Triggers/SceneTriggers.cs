using System;
using Newtonsoft.Json;
using UnityEngine;

namespace HKTimer {
    namespace Triggers {
        public class SceneTrigger : Trigger {
            public const string name = "scene";
            public override string Name => name;

            public override void Destroy(TriggerManager tm) { }

            // Takes advantage of the fact that `Spawn` is called on scene transition
            public override void Spawn(TriggerManager tm) {
                
                if(this.scene == GameManager.instance.sceneName) {
                    tm.ExecLogic(this.logic);
                }
            }
        }
    }
}