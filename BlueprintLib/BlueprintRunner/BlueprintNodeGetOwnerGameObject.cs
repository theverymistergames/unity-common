using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Owner GameObject", Category = "Blueprint Runner", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeGetOwnerGameObject : BlueprintNode, IBlueprintOutput<GameObject> {
        
        public override Port[] CreatePorts() => new[] {
            Port.Output<GameObject>("GameObject"),
        };

        private GameObject _runnerGameObject;

        public override void OnInitialize(IBlueprintHost host) {
            _runnerGameObject = host.Runner.gameObject;
        }

        public override void OnDeInitialize() {
            _runnerGameObject = null;
        }

        public GameObject GetOutputPortValue(int port) => port switch {
            0 => _runnerGameObject,
            _ => null,
        };
    }

}
