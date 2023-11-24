using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetOwnerGameObject :
        BlueprintSource<BlueprintNodeGetOwnerGameObject2>,
        BlueprintSources.IOutput<BlueprintNodeGetOwnerGameObject2, GameObject>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Owner GameObject", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeGetOwnerGameObject2 : IBlueprintNode, IBlueprintOutput2<GameObject> {

        private MonoBehaviour _runner;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<GameObject>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _runner = blueprint.Host;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _runner = null;
        }

        public GameObject GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => _runner.gameObject,
            _ => null,
        };
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Owner GameObject", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeGetOwnerGameObject : BlueprintNode, IBlueprintOutput<GameObject> {
        
        public override Port[] CreatePorts() => new[] {
            Port.Output<GameObject>(),
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
