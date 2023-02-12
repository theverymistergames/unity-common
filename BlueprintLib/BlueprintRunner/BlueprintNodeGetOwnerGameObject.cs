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

        private MonoBehaviour _runner;

        public override void OnInitialize(IBlueprintHost host) {
            _runner = host.Runner;
        }

        public override void OnDeInitialize() {
            _runner = null;
        }

        public GameObject GetOutputPortValue(int port) => port switch {
            0 => _runner.gameObject,
            _ => null,
        };
    }

}
