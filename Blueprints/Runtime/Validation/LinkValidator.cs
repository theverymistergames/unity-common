#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Core;
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

            if (fromPort.Signature == null) {
                return fromPort.IsInput
                    ? ValidateLinkFromAnyInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                    : ValidateLinkFromAnyOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
            }

            if (fromPort.IsAction) {
                if (fromPort.IsAnyAction) {
                    return fromPort.IsInput
                        ? ValidateLinkFromAnyActionInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                        : ValidateLinkFromAnyActionOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
                }

                return fromPort.IsInput
                    ? ValidateLinkFromActionInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                    : ValidateLinkFromActionOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
            }

            if (fromPort.IsFunc) {
                if (fromPort.IsAnyFunc) {
                    return fromPort.IsInput
                        ? ValidateLinkFromAnyFuncInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                        : ValidateLinkFromAnyFuncOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
                }

                if (fromPort.IsDynamicFunc) {
                    return fromPort.IsInput
                        ? ValidateLinkFromDynamicFuncInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                        : ValidateLinkFromDynamicFuncOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
                }

                return fromPort.IsInput
                    ? ValidateLinkFromFuncInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                    : ValidateLinkFromFuncOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
            }

            return fromPort.IsInput
                ? ValidateLinkFromCustomInputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex)
                : ValidateLinkFromCustomOutputPort(blueprint, fromNodeMeta, fromPortIndex, toNodeMeta, toPortIndex);
        }

        private static bool ValidateLinkFromAnyInputPort(
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

            if (toPort.Signature == null || toPort.IsAnyAction || toPort.IsAnyFunc || toPort.IsDynamicFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"any-input port cannot have links to " +
                               $"any-output ports, " +
                               $"any-action output ports, " +
                               $"any-func output ports or " +
                               $"dynamic-func output ports.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromAnyOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is any-output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromAnyActionInputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is any-action input port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromAnyActionOutputPort(
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

            if (toPort.Signature == null) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-action output port cannot have link to the any-input port.");
                return false;
            }

            if (!toPort.IsAction) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-action output port cannot have link to the non-action input port.");
                return false;
            }

            if (toPort.IsAnyAction) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-action output port cannot have link to the any-action input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromActionInputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is action input port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromActionOutputPort(
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

            if (!toPort.IsInput) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPort.Signature == null) return true;

            if (!toPort.IsAction) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source action output port cannot have link to the non-action input port.");
                return false;
            }

            if (toPort.IsAnyAction) return true;

            if (fromPort.Signature != toPort.Signature) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"sports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromAnyFuncInputPort(
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

            if (toPort.Signature == null) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-func input port cannot have link to the any-output port.");
                return false;
            }

            if (!toPort.IsFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-func input port cannot have link to the non-func output port.");
                return false;
            }

            if (toPort.IsAnyFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-func input port cannot have link to the any-func output port.");
                return false;
            }

            if (toPort.IsDynamicFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source any-func input port cannot have link to the dynamic-func output port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromAnyFuncOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is any-func output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromDynamicFuncInputPort(
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

            if (toPort.Signature == null) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic-func input port cannot have link to the any-output port.");
                return false;
            }

            if (!toPort.IsFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic-func input port cannot have link to the non-func output port.");
                return false;
            }

            if (toPort.IsAnyFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic-func input port cannot have link to the any-func output port.");
                return false;
            }

            if (toPort.IsDynamicFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic-func input port cannot have link to the dynamic-func output port.");
                return false;
            }

            var genericArguments = toPort.Signature.GetGenericArguments();
            if (genericArguments.Length > 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source dynamic-func input port cannot have link to the func output port with more than 1 generic parameter.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicFuncOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is dynamic-func output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromFuncInputPort(
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

            if (toPort.Signature == null) return true;

            if (!toPort.IsFunc) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"source func input port cannot have link to the non-func output port.");
                return false;
            }

            if (toPort.IsAnyFunc) return true;

            var fromPortGenericArguments = fromPort.Signature.GetGenericArguments();

            if (toPort.IsDynamicFunc) {
                if (fromPortGenericArguments.Length > 1) {
                    Debug.LogError($"Blueprint `{blueprint.name}`: " +
                                   $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                                   $"source func input port with more than 1 generic parameter cannot have link to the dynamic-func output port.");
                    return false;
                }

                return true;
            }

            if (fromPort.Signature != toPort.Signature) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromFuncOutputPort(
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

        private static bool ValidateLinkFromCustomInputPort(
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
            var fromPort = fromNodeMeta.Ports[fromPortIndex];

            if (!toPort.IsInput) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPort.Signature == null) return true;

            if (fromPort.Signature != toPort.Signature) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromCustomOutputPort(
            BlueprintAsset blueprint,
            BlueprintNodeMeta fromNodeMeta,
            int fromPortIndex,
            BlueprintNodeMeta toNodeMeta,
            int toPortIndex
        ) {
            Debug.LogError($"Blueprint `{blueprint.name}`: " +
                           $"Validation failed for port link [node {fromNodeMeta}, port {fromPortIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {fromPortIndex} of node {fromNodeMeta} is custom output port and it cannot have links.");
            return false;
        }
    }

}

#endif
