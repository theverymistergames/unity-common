#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

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

        public static bool ValidateExternalPortWithExistingSignature(BlueprintMeta2 meta, Port port) {
            if (!port.IsInput() && port.IsData()) {
                Debug.LogWarning($"Subgraph node has blueprint `{meta.owner}` " +
                                 $"which contains multiple external output ports with same name `{port.Name}`: " +
                                 $"this can cause incorrect results. " +
                                 $"Blueprint can have multiple external output ports with same name only for enter, exit or input ports." +
                                 $"Blueprint `{meta.owner}` has to be refactored in order to be used as subgraph.");
                return false;
            }

            return true;
        }

        public static bool ValidatePort(BlueprintMeta2 meta, NodeId id, int index) {
            if (index < 0 || index > meta.GetPortCount(id) - 1) {
                Debug.LogError($"Blueprint `{meta.owner}`: " +
                               $"Validation failed for port [{index}] {index} of node {id}: " +
                               $"node {id} has no port with index {index}.");
                return false;
            }

            // Any ports allowed for subgraph and external blueprint nodes
            if (meta.GetNodeSource(id) is IBlueprintCompilable) return true;

            var port = meta.GetPort(id, index);
            if (port.IsData()) {
                // Data input port: nothing to validate
                if (port.IsInput()) return true;

                return port.DataType == null
                    ? ValidateDynamicOutputPort(meta, id, port, index)
                    : ValidateOutputPort(meta, id, port, index);
            }

            // Exit port: nothing to validate
            if (!port.IsInput()) return true;

            return ValidateEnterPort(meta, id, port, index);
        }

        private static bool ValidateEnterPort(BlueprintMeta2 meta, NodeId id, Port port, int index) {
            var source = meta.GetNodeSource(id);

            if (source is not (IBlueprintEnter2 or IBlueprintInternalLink)) {
                Debug.LogError($"Blueprint `{meta.owner}`: " +
                               $"Validation failed for enter port [{index}] {port} of node {id}: " +
                               $"node source class {source.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintEnter2)} or " +
                               $"{nameof(IBlueprintInternalLink)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicOutputPort(BlueprintMeta2 meta, NodeId id, Port port, int index) {
            var source = meta.GetNodeSource(id);

            if (source is not (IBlueprintOutput2 or IBlueprintInternalLink)) {
                Debug.LogError($"Blueprint `{meta.owner}`: " +
                               $"Validation failed for dynamic output port [{index}] {port} of node {id}: " +
                               $"node source class {source.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput2)} or " +
                               $"{nameof(IBlueprintInternalLink)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintMeta2 meta, NodeId id, Port port, int index) {
            var source = meta.GetNodeSource(id);

            if (source is not (IBlueprintOutput2 or IBlueprintInternalLink) &&
                ValidationUtils2.GetGenericInterface(source.GetType(), typeof(IBlueprintOutput2<>), port.DataType) == null
            ) {
                Debug.LogError($"Blueprint `{meta.owner}`: " +
                               $"Validation failed for output port [{index}] {port} of node {id}: " +
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
