#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class LinkValidator {

        public static bool ValidateLink(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex, BlueprintNodeMeta toNodeMeta, int toPortIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            return port.mode switch {
                Port.Mode.Enter => ValidateLinkToEnterPort(asset, toNodeMeta, toPortIndex),
                Port.Mode.Exit => ValidateLinkToExitPort(asset, toNodeMeta, toPortIndex),
                Port.Mode.Input => ValidateLinkToInputPort(asset, port.DataType, toNodeMeta, toPortIndex),
                Port.Mode.Output => ValidateLinkToOutputPort(asset, port.DataType, toNodeMeta, toPortIndex),
                Port.Mode.InputArray => ValidateLinkToInputArrayPort(asset, port.DataType, toNodeMeta, toPortIndex),
                Port.Mode.NonTypedInput => ValidateLinkToNonTypedInputPort(asset, toNodeMeta, toPortIndex),
                Port.Mode.NonTypedOutput => ValidateLinkToNonTypedOutputPort(asset, toNodeMeta, toPortIndex),
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
            };
        }

        private static bool ValidateLinkToEnterPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {nodeMeta}, port {portIndex}]: " +
                           $"port {portIndex} of node {nodeMeta} is enter port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkToExitPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.mode != Port.Mode.Enter) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an enter port.");
                return false;
            }

            if (node is not (IBlueprintEnter or IBlueprintPortLinker)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} " +
                               $"does not implement interface {nameof(IBlueprintEnter)} or {nameof(IBlueprintPortLinker)}.");
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

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (port.mode == Port.Mode.NonTypedOutput) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for non-typed output port {portIndex} of node {nodeMeta}: " +
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

            bool implementsIBlueprintOutputInterface = ValidationUtils.HasGenericInterface(
                node.GetType(),
                typeof(IBlueprintOutput<>),
                port.DataType
            );

            if (!implementsIBlueprintOutputInterface && node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}> or {typeof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkToOutputPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {nodeMeta}, port {portIndex}]: " +
                           $"port {portIndex} of node {nodeMeta} is output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkToInputArrayPort(BlueprintAsset asset, Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (port.mode == Port.Mode.NonTypedOutput) {
                if (node is not IBlueprintPortLinker) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for non-typed output port {portIndex} of node {nodeMeta}: " +
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

            bool implementsIBlueprintOutputInterface = ValidationUtils.HasGenericInterface(
                node.GetType(),
                typeof(IBlueprintOutput<>),
                port.DataType
            );

            if (!implementsIBlueprintOutputInterface && node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}> or {typeof(IBlueprintPortLinker)}.");
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

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (port.mode is not Port.Mode.Output) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            bool implementsIBlueprintOutputInterface = ValidationUtils.HasGenericInterface(
                node.GetType(),
                typeof(IBlueprintOutput<>),
                port.DataType
            );

            if (!implementsIBlueprintOutputInterface && node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}> or {typeof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkToNonTypedOutputPort(BlueprintAsset asset, BlueprintNodeMeta nodeMeta, int portIndex) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {nodeMeta}, port {portIndex}]: " +
                           $"port {portIndex} of node {nodeMeta} is non-typed output port and it cannot have links.");
            return false;
        }
    }

}

#endif
