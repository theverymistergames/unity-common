#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    internal static class PortValidator2 {

        public static bool ArePortsCompatible(Port a, Port b) {
            // External ports cannot have connections
            if (a.IsExternal() || b.IsExternal()) return false;

            // Hidden ports cannot have connections
            if (a.IsHidden() || b.IsHidden()) return false;

            // In a blueprint graph port views are able to hold connections
            // only if they have different directions, which is defined by layout
            if (a.IsLeftLayout() == b.IsLeftLayout()) return false;

            // Ports with same PortMode (Input/Output) are not compatible
            if (a.IsInput() == b.IsInput()) return false;

            if (!a.IsData()) return !b.IsData();
            if (!b.IsData()) return false;

            var aDataType = a.DataType;
            var bDataType = b.DataType;

            // Dynamic data ports are not compatible with each other
            if (aDataType == null) return bDataType != null;
            if (bDataType == null) return true;

            if (aDataType.IsValueType) return aDataType == bDataType;
            if (bDataType.IsValueType) return false;

            if (a.IsInput()) {
                if (a.IsMultiple() && bDataType.IsArray && bDataType.GetElementType() == aDataType) return true;

                return b.AcceptSubclass() ? bDataType.IsAssignableFrom(aDataType) : aDataType.IsAssignableFrom(bDataType);
            }

            if (b.IsMultiple() && aDataType.IsArray && aDataType.GetElementType() == bDataType) return true;

            return a.AcceptSubclass() ? aDataType.IsAssignableFrom(bDataType) : bDataType.IsAssignableFrom(aDataType);
        }

        public static bool ValidateExternalPortWithExistingSignature(BlueprintAsset2 asset, Port port) {
            if (!port.IsInput() && port.IsData()) {
                Debug.LogWarning($"Subgraph node has blueprint `{asset.name}` " +
                                 $"which contains multiple external output ports with same name `{port.Name}`: " +
                                 $"this can cause incorrect results. " +
                                 $"Blueprint can have multiple external output ports with same name only for enter, exit or input ports." +
                                 $"Blueprint `{asset.name}` has to be refactored in order to be used as subgraph.");
                return false;
            }

            return true;
        }

        public static bool ValidatePort(BlueprintAsset2 asset, NodeId id, int index) {
            if (index < 0 || index > asset.BlueprintMeta.GetPortCount(id) - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for port [{index}] {index} of node {id}: " +
                               $"node {id} has no port with index {index}.");
                return false;
            }

            var port = asset.BlueprintMeta.GetPort(id, index);

            if (port.IsData()) return port.IsInput() || (
                port.DataType == null
                    ? ValidateDynamicOutputPort(asset, id, port, index)
                    : ValidateOutputPort(asset, id, port, index)
            );

            return !port.IsInput() || ValidateEnterPort(asset, id, port, index);
        }

        private static bool ValidateEnterPort(BlueprintAsset2 asset, NodeId id, Port port, int index) {
            var source = asset.BlueprintMeta.GetNodeSource(id);

            if (source is not (IBlueprintEnter2 or IBlueprintInternalLink)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port [{index}] {port} of node {id}: " +
                               $"node source class {source.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintEnter2)} or " +
                               $"{nameof(IBlueprintInternalLink)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicOutputPort(BlueprintAsset2 asset, NodeId id, Port port, int index) {
            var source = asset.BlueprintMeta.GetNodeSource(id);

            if (source is not (IBlueprintOutput2 or IBlueprintInternalLink)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for dynamic output port [{index}] {port} of node {id}: " +
                               $"node source class {source.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput2)} or " +
                               $"{nameof(IBlueprintInternalLink)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintAsset2 asset, NodeId id, Port port, int index) {
            var source = asset.BlueprintMeta.GetNodeSource(id);

            if (source is not (IBlueprintOutput2 or IBlueprintInternalLink) &&
                ValidationUtils2.GetGenericInterface(source.GetType(), typeof(IBlueprintOutput<>), port.DataType) == null
            ) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for func output port [{index}] {port} of node {id}: " +
                               $"node source class {source.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput2)} or " +
                               $"{typeof(IBlueprintOutput2<>).Name} or " +
                               $"{nameof(IBlueprintInternalLink)}.");
                return false;
            }

            return true;
        }
    }

}

#endif
