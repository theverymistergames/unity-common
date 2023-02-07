using System;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class BlueprintValidation {

        private const int MAX_SUBGRAPH_LEVELS = 100;

        public static BlueprintAsset ValidateBlueprintAssetForSubgraph(
            BlueprintAsset ownerAsset,
            BlueprintAsset subgraphAsset
        ) {
            return IsValidBlueprintAssetForSubgraph(ownerAsset, subgraphAsset, 0, $"`{ownerAsset.name}`")
                ? subgraphAsset
                : null;
        }

        private static bool IsValidBlueprintAssetForSubgraph(
            BlueprintAsset ownerAsset,
            BlueprintAsset subgraphAsset,
            int level,
            string path
        ) {
            if (subgraphAsset == null) return true;

            path += $" <- `{subgraphAsset.name}`";
            level++;

            if (level >= MAX_SUBGRAPH_LEVELS) {
                Debug.LogWarning($"Subgraph node of `{ownerAsset.name}` " +
                                 $"cannot accept `{subgraphAsset.name}` as parameter: " +
                                 $"subgraph depth is reached max level {MAX_SUBGRAPH_LEVELS}. " +
                                 $"Path: [{path}]");
                return false;
            }

            if (subgraphAsset == ownerAsset) {
                Debug.LogWarning($"Subgraph node of `{ownerAsset.name}` " +
                                 $"cannot accept `{subgraphAsset.name}` as parameter: " +
                                 $"this will produce cyclic references. " +
                                 $"Path: [{path}]");
                return false;
            }

            var references = subgraphAsset.BlueprintMeta.SubgraphReferencesMap.Values;
            foreach (var asset in references) {
                if (!IsValidBlueprintAssetForSubgraph(ownerAsset, asset, level, path)) return false;
            }

            return true;
        }

        public static bool ArePortsCompatible(Port a, Port b) {
            // External ports are not compatible in editor
            if (a.isExternalPort || b.isExternalPort) return false;

            return a.mode switch {
                Port.Mode.Enter => b.mode == Port.Mode.Exit,
                Port.Mode.Exit => b.mode == Port.Mode.Enter,
                Port.Mode.Input => b.mode == Port.Mode.Output && a.DataType == b.DataType || b.mode == Port.Mode.NonTypedOutput,
                Port.Mode.Output => b.mode is Port.Mode.Input && a.DataType == b.DataType || b.mode == Port.Mode.NonTypedInput,
                Port.Mode.NonTypedInput => b.mode is Port.Mode.Output or Port.Mode.NonTypedOutput,
                Port.Mode.NonTypedOutput => b.mode is Port.Mode.Input or Port.Mode.NonTypedInput,
                _ => throw new NotSupportedException($"Port mode {a.mode} is not supported")
            };
        }

        public static bool ValidatePort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            return port.mode switch {
                Port.Mode.Enter => ValidateEnterPort(asset, nodeMeta, portIndex),
                Port.Mode.Exit => ValidateExitPort(asset, nodeMeta, portIndex),
                Port.Mode.Input => ValidateInputPort(asset, port.DataType, nodeMeta, portIndex),
                Port.Mode.Output => ValidateOutputPort(asset, port.DataType, nodeMeta, portIndex),
                Port.Mode.NonTypedInput => ValidateNonTypedInputPort(asset, nodeMeta, portIndex),
                Port.Mode.NonTypedOutput => ValidateNonTypedOutputPort(asset, nodeMeta, portIndex),
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
            };
        }

        public static bool ValidateExternalPortWithExistingSignature(BlueprintAsset asset, Port port) {
            if (port.mode is Port.Mode.Output or Port.Mode.NonTypedOutput) {
                Debug.LogWarning($"Subgraph node has blueprint `{asset.name}`, " +
                                 $"which contains multiple external outputs with same name `{port.name}`: " +
                                 $"this can cause incorrect output values. " +
                                 $"Blueprint `{asset.name}` has to be refactored in order to be used as subgraph.");
                return false;
            }

            return true;
        }

        public static bool ValidateLink(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex, BlueprintNodeMeta toNodeMeta, int toPortIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            switch (port.mode) {
                case Port.Mode.Enter:
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                                   $"port {portIndex} of node {nodeMeta} is enter port and it cannot have links.");
                    return false;

                case Port.Mode.Exit:
                    return ValidateLinkToExitPort(asset, toNodeMeta, toPortIndex);

                case Port.Mode.Input:
                    return ValidateLinkToInputPort(asset, port.DataType, toNodeMeta, toPortIndex);

                case Port.Mode.NonTypedInput:
                    return ValidateLinkToNonTypedInputPort(asset, toNodeMeta, toPortIndex);

                case Port.Mode.Output:
                case Port.Mode.NonTypedOutput:
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                                   $"port {portIndex} of node {nodeMeta} is output port and it cannot have links.");
                    return false;

                default:
                    throw new NotSupportedException($"Port mode {port.mode} is not supported");
            }
        }

        private static bool ValidateLinkToExitPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.mode != Port.Mode.Enter) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an enter port.");
                return false;
            }

            var node = nodeMeta.Node;

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external enter port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} " +
                                   $"does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (node is not IBlueprintEnter) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} " +
                               $"does not implement interface {nameof(IBlueprintEnter)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkToInputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            var node = nodeMeta.Node;
            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external output port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (port.mode == Port.Mode.Output && port.DataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            if (port.mode == Port.Mode.Output &&
                !HasGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.DataType)
            ) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}>.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkToNonTypedInputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            var node = nodeMeta.Node;

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external output port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (port.mode == Port.Mode.Output &&
                !HasGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.DataType)
            ) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}>.");
                return false;
            }

            return true;
        }

        private static bool ValidateEnterPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external enter port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} " +
                                   $"does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (node is not IBlueprintEnter) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} " +
                               $"does not implement interface {nameof(IBlueprintEnter)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateExitPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external exit port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} " +
                                   $"does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            return true;
        }

        private static bool ValidateInputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external input port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (port.DataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this input port type is not {dataType.Name}.");
                return false;
            }

            return true;
        }

        private static bool ValidateNonTypedInputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external input port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external output port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            if (port.DataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            if (!HasGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.DataType)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}>.");
                return false;
            }

            return true;
        }

        private static bool ValidateNonTypedOutputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.isExternalPort) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for external output port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            return true;
        }

        private static bool HasGenericInterface(Type subjectType, Type interfaceType, Type genericArgumentType) {
            var interfaces = subjectType.GetInterfaces();
            bool hasInterface = false;

            for (int i = 0; i < interfaces.Length; i++) {
                var x = interfaces[i];
                if (x.IsGenericType &&
                    x.GetGenericTypeDefinition() == interfaceType &&
                    x.GenericTypeArguments.Length == 1 &&
                    x.GenericTypeArguments[0] == genericArgumentType
                   ) {
                    hasInterface = true;
                    break;
                }
            }

            return hasInterface;
        }
    }

}
