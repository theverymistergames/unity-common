#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class LinkValidator {

        public static bool ValidateLink(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            if (fromPortIndex < 0 || fromPortIndex > fromNodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{fromNodeMeta} has no port with index {fromPortIndex}.");
                return false;
            }

            var fromPort = fromNodeMeta.Ports[fromPortIndex];

            if (fromPort.IsData) {
                if (fromPort.DataType == null) {
                    return fromPort.IsInput
                        ? ValidateLinkFromDynamicInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                        : ValidateLinkFromDynamicOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
                }

                return fromPort.IsInput
                    ? ValidateLinkFromInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                    : ValidateLinkFromOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
            }

            return fromPort.IsInput
                ? ValidateLinkFromEnterPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                : ValidateLinkFromExitPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
        }

        private static bool ValidateLinkFromEnterPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is enter port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromExitPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            if (toPortIndex < 0 || toPortIndex > toNodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{toNodeMeta} has no port with index {toPortIndex}.");
                return false;
            }

            var toPort = toNodeMeta.Ports[toPortIndex];

            if (!toPort.IsInput) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPort.IsData) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source exit port cannot have link to the data-based input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicInputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            if (toPortIndex < 0 || toPortIndex > toNodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{toNodeMeta} has no port with index {toPortIndex}.");
                return false;
            }

            var toPort = toNodeMeta.Ports[toPortIndex];

            if (toPort.IsInput) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPort.IsData) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic input port cannot have link to the non-data output port.");
                return false;
            }

            if (toPort.DataType == null) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic input port cannot have link to the dynamic output port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is dynamic output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromInputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            if (toPortIndex < 0 || toPortIndex > toNodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{toNodeMeta} has no port with index {toPortIndex}.");
                return false;
            }

            var fromPort = fromNodeMeta.Ports[fromPortIndex];
            var toPort = toNodeMeta.Ports[toPortIndex];

            if (toPort.IsInput) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPort.IsData) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source input port cannot have link to the non-data output port.");
                return false;
            }

            var fromPortDataType = fromPort.DataType;
            var toPortDataType = toPort.DataType;

            if (toPortDataType == null) return true;

            if (fromPort.IsMultiple && toPortDataType.IsArray) {
                if (toPortDataType.GetElementType() != fromPortDataType) {
                    Debug.LogError($"Blueprint `{blueprint.name}`: " +
                                   $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                                   $"ports have different signature.");
                    return false;
                }

                return true;
            }

            if (fromPortDataType != toPortDataType) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is func output port and it cannot have links.");
            return false;
        }
    }

}

#endif
