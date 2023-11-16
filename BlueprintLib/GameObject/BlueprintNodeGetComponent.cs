using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetComponent :
        BlueprintSource<BlueprintNodeGetComponent2>,
        BlueprintSources.IOutput<BlueprintNodeGetComponent2>,
        BlueprintSources.IConnectionCallback<BlueprintNodeGetComponent2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Component", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeGetComponent2 : IBlueprintNode, IBlueprintOutput2, IBlueprintConnectionCallback {

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

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Component", Category = "GameObject", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeGetComponent : BlueprintNode, IBlueprintOutput

#if UNITY_EDITOR
        , IBlueprintPortLinksListener
        , IBlueprintPortDecorator
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private InputType _input;

        private enum InputType {
            GameObject,
            Component,
        }

        public override Port[] CreatePorts() => new[] {
            _input == InputType.GameObject ? Port.Input<GameObject>() : Port.Input<Component>(),
            Port.DynamicOutput(type: typeof(Component), acceptSubclass: true),
        };

        public T GetOutputPortValue<T>(int port) {
            if (port != 1) return default;

            return _input == InputType.GameObject
                ? Ports[0].Get<GameObject>().GetComponent<T>()
                : Ports[0].Get<Component>().GetComponent<T>();
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var linksToOutput = blueprint.BlueprintMeta.GetLinksToNodePort(nodeId, 1);
            if (linksToOutput.Count == 0) return;

            var link = linksToOutput[0];
            var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            var dataType = linkedPort.DataType;
            if (dataType == null) return;

            ports[1] = Port.DynamicOutput(type: linkedPort.DataType);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }

        public void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex) {
            if (portIndex == 1) blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
