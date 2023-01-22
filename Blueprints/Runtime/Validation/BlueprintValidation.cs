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
            return IsValidBlueprintAssetForSubgraph(ownerAsset, subgraphAsset, 0, ownerAsset.name)
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

            path += $" -> {subgraphAsset.name}";
            level++;

            if (level >= MAX_SUBGRAPH_LEVELS) {
                Debug.LogWarning($"Subgraph node of blueprint {ownerAsset.name} " +
                                 $"cannot accept blueprint {subgraphAsset.name} as parameter " +
                                 $"because subgraph depth is reached max level {MAX_SUBGRAPH_LEVELS}. " +
                                 $"Path: [{path}]");
                return false;
            }

            if (subgraphAsset == ownerAsset) {
                Debug.LogWarning($"Subgraph node of blueprint {ownerAsset.name} " +
                                 $"cannot accept blueprint {subgraphAsset.name} as parameter " +
                                 $"because this will produce cyclic references. " +
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

            // Must have different port direction
            if (a.isExitPort == b.isExitPort) return false;

            // Must be same mode (flow/data)
            if (a.isDataPort != b.isDataPort) return false;

            // Both ports are flow ports
            if (!a.isDataPort) return true;

            // Both ports are data ports, at least one port has no serialized type
            if (!a.hasDataType || !b.hasDataType) return true;

            // Must have same data type
            if (a.DataType != b.DataType) return false;

            return true;
        }

        public static bool ValidatePort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            // Enter port
            if (!port.isDataPort && !port.isExitPort) {
                return ValidateEnterPort(asset, nodeMeta, portIndex);
            }

            // Exit port
            if (!port.isDataPort) {
                return ValidateExitPort(asset, nodeMeta, portIndex);
            }

            // Input port
            if (!port.isExitPort) {
                return port.hasDataType
                    ? ValidateInputPort(asset, port.DataType, nodeMeta, portIndex)
                    : ValidateInputPort(asset, nodeMeta, portIndex);
            }

            // Output port
            return port.hasDataType
                ? ValidateOutputPort(asset, port.DataType, nodeMeta, portIndex)
                : ValidateOutputPort(asset, nodeMeta, portIndex);
        }

        public static bool ValidateLink(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex, BlueprintNodeMeta toNodeMeta, int toPortIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            // Enter port
            if (!port.isDataPort && !port.isExitPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"port {portIndex} of node {nodeMeta} is enter port and it cannot have links.");
                return false;
            }

            // Exit port
            if (!port.isDataPort) {
                return ValidateEnterPort(asset, toNodeMeta, toPortIndex);
            }

            // Input port
            if (!port.isExitPort) {
                return port.hasDataType
                    ? ValidateOutputPort(asset, port.DataType, toNodeMeta, toPortIndex)
                    : ValidateOutputPort(asset, toNodeMeta, toPortIndex);
            }

            // Output port
            Debug.LogError($"Blueprint asset {asset.name}: " +
                           $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {portIndex} of node {nodeMeta} is output port and it cannot have links.");
            return false;
        }

        private static bool ValidateEnterPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an enter port.");
                return false;
            }

            if (nodeMeta.Node is not IBlueprintEnter) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement interface {nameof(IBlueprintEnter)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateExitPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an exit port.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an input port.");
                return false;
            }

            if (port.hasDataType && port.DataType != dataType) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this input port type is not {dataType.Name}.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (!port.hasDataType && nodeMeta.Node is not IBlueprintOutput) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for non-typed output port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement interface {nameof(IBlueprintOutput)}.");
                return false;
            }
            
            if (port.hasDataType &&
                !HasGenericInterface(nodeMeta.Node.GetType(), typeof(IBlueprintOutput<>), port.DataType)
            ) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}>.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (!port.hasDataType) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port has no type, but needs to have type {dataType.Name}.");
                return false;
            }
            
            if (port.DataType != dataType) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            if (!HasGenericInterface(nodeMeta.Node.GetType(), typeof(IBlueprintOutput<>), dataType)) {
                Debug.LogError($"Blueprint asset {asset.name}: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{dataType.Name}>.");
                return false;
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
