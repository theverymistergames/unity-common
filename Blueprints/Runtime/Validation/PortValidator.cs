#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class PortValidator {

        public static bool ArePortsCompatible(Port a, Port b) {
            // External ports cannot have connections
            if (a.IsExternal || b.IsExternal) return false;

            // Hidden ports cannot have connections
            if (a.IsHidden || b.IsHidden) return false;

            // In a blueprint graph port views are able to hold connections
            // only if they have different directions, which is defined by layout
            if (a.IsLeftLayout == b.IsLeftLayout) return false;

            // Ports with same PortMode (Input/Output) are not compatible
            if (a.IsInput == b.IsInput) return false;

            if (!a.IsData) return !b.IsData;
            if (!b.IsData) return false;

            var aDataType = a.DataType;
            var bDataType = b.DataType;

            // Dynamic data ports are not compatible with each other
            if (aDataType == null) return bDataType != null;
            if (bDataType == null) return true;

            if (aDataType.IsValueType) return aDataType == bDataType;
            if (bDataType.IsValueType) return false;

            return a.IsInput ? aDataType.IsAssignableFrom(bDataType) : bDataType.IsAssignableFrom(aDataType);
        }

        public static bool ValidateExternalPortWithExistingSignature(BlueprintAsset asset, Port port) {
            if (!port.IsInput && port.IsData) {
                Debug.LogWarning($"Subgraph node has blueprint `{asset.name}` " +
                                 $"which contains multiple external output ports with same name `{port.Name}`: " +
                                 $"this can cause incorrect results. " +
                                 $"Blueprint can have multiple external output ports with same name only for enter, exit or input ports." +
                                 $"Blueprint `{asset.name}` has to be refactored in order to be used as subgraph.");
                return false;
            }

            return true;
        }

        public static bool ValidatePort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            if (port.IsData) {
                if (port.DataType == null) {
                    return port.IsInput
                        ? ValidateDynamicInputPort(blueprint, nodeMeta, portIndex)
                        : ValidateDynamicOutputPort(blueprint, nodeMeta, portIndex);
                }

                return port.IsInput
                    ? ValidateInputPort(blueprint, nodeMeta, portIndex)
                    : ValidateOutputPort(blueprint, nodeMeta, portIndex);
            }

            return port.IsInput
                ? ValidateEnterPort(blueprint, nodeMeta, portIndex)
                : ValidateExitPort(blueprint, nodeMeta, portIndex);
        }

        private static bool ValidateEnterPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not (IBlueprintEnter or IBlueprintPortLinker)) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintEnter)} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateExitPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not (IBlueprintOutput or IBlueprintPortLinker)) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput)} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (node is not (IBlueprintOutput or IBlueprintPortLinker) &&
                ValidationUtils.GetGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.DataType) == null
            ) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for func output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput)} or " +
                               $"{typeof(IBlueprintOutput<>).Name} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }
    }

}

#endif
