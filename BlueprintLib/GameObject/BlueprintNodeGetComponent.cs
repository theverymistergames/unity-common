using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetComponent :
        BlueprintSource<BlueprintNodeGetComponent>,
        BlueprintSources.IOutput<BlueprintNodeGetComponent>,
        BlueprintSources.IConnectionCallback<BlueprintNodeGetComponent>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Component", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeGetComponent : IBlueprintNode, IBlueprintOutput, IBlueprintConnectionCallback {

        [SerializeField] private InputType _input;

        private enum InputType {
            GameObject,
            Component,
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            var outputDataType = typeof(Component);

            if (meta.TryGetLinksTo(id, 1, out int l)) {
                var link = meta.GetLink(l);
                var linkedPortDataType = meta.GetPort(link.id, link.port).DataType;
                if (linkedPortDataType != null) outputDataType = linkedPortDataType;
            }

            meta.AddPort(id, _input == InputType.GameObject ? Port.Input<GameObject>() : Port.Input<Component>());
            meta.AddPort(id, Port.DynamicOutput(type: outputDataType, acceptSubclass: true));
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 1) return default;

            return _input == InputType.GameObject
                ? blueprint.Read<GameObject>(token, 0).GetComponent<T>()
                : blueprint.Read<Component>(token, 0).GetComponent<T>();
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port == 1) meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

}
