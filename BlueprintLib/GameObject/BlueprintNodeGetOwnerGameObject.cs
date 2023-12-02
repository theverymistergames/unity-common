using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetOwnerGameObject :
        BlueprintSource<BlueprintNodeGetOwnerGameObject>,
        BlueprintSources.IOutput<BlueprintNodeGetOwnerGameObject, GameObject>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Owner GameObject", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeGetOwnerGameObject : IBlueprintNode, IBlueprintOutput<GameObject> {

        private MonoBehaviour _runner;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<GameObject>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _runner = blueprint.Host;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _runner = null;
        }

        public GameObject GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => _runner.gameObject,
            _ => null,
        };
    }

}
