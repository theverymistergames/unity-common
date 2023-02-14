#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class PortValidator {

        public static bool ArePortsCompatible(Port a, Port b) {
            // External ports are not compatible in editor
            if (a.isExternalPort || b.isExternalPort) return false;

            return a.mode switch {
                Port.Mode.Enter => b.mode == Port.Mode.Exit,
                Port.Mode.Exit => b.mode == Port.Mode.Enter,
                Port.Mode.Input => b.mode == Port.Mode.Output && a.dataType == b.dataType || b.mode == Port.Mode.NonTypedOutput,
                Port.Mode.Output => (b.mode is Port.Mode.Input or Port.Mode.InputArray) && a.dataType == b.dataType || b.mode == Port.Mode.NonTypedInput,
                Port.Mode.NonTypedInput => b.mode is Port.Mode.Output,
                Port.Mode.NonTypedOutput => b.mode is Port.Mode.Input or Port.Mode.InputArray,
                _ => throw new NotSupportedException($"Port mode {a.mode} is not supported")
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
                Port.Mode.Input => ValidateInputPort(asset, port.dataType, nodeMeta, portIndex),
                Port.Mode.Output => ValidateOutputPort(asset, port.dataType, nodeMeta, portIndex),
                Port.Mode.InputArray => ValidateInputArrayPort(asset, port.dataType, nodeMeta, portIndex),
                Port.Mode.NonTypedInput => ValidateNonTypedInputPort(asset, nodeMeta, portIndex),
                Port.Mode.NonTypedOutput => ValidateNonTypedOutputPort(asset, nodeMeta, portIndex),
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
            };
        }

        private static bool ValidateEnterPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not (IBlueprintEnter or IBlueprintPortLinker)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} " +
                               $"does not implement interface {nameof(IBlueprintEnter)} or {nameof(IBlueprintPortLinker)}.");
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

            return true;
        }

        private static bool ValidateInputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            if (port.dataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this input port type is not {dataType.Name}.");
                return false;
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

            if (port.dataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            bool implementsIBlueprintOutputInterface = ValidationUtils.GetGenericInterface(
                node.GetType(),
                typeof(IBlueprintOutput<>),
                port.dataType
            ) != null;

            if (!implementsIBlueprintOutputInterface && node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{typeof(IBlueprintOutput<>).Name}<{port.dataType.Name}> or {typeof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputArrayPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input-array port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            if (port.dataType != dataType) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for input-array port {portIndex} of node {nodeMeta}: " +
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

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for non-typed output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }
    }

}

#endif
