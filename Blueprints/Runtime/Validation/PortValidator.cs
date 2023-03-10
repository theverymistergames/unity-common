#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class PortValidator {

        public static bool ArePortsCompatible(Port a, Port b) {
            // External ports are hidden and cannot have connections
            if (a.IsExternal || b.IsExternal) return false;

            // In a blueprint graph port views are compatible to create connections
            // only if they have different directions, which is defined by layout
            if (a.IsLeftLayout == b.IsLeftLayout) return false;

            // Ports with same PortMode (Input/Output) are not compatible
            if (a.IsInput == b.IsInput) return false;

            // Port with null signature is compatible with any other port that has concrete signature
            if (a.Signature == null) return b.Signature != null && !b.IsAnyAction && !b.IsAnyFunc && !b.IsDynamicFunc;
            if (b.Signature == null) return a.Signature != null && !a.IsAnyAction && !a.IsAnyFunc && !a.IsDynamicFunc;

            if (a.IsAction) {
                if (!b.IsAction) return false;

                // Any-action port is compatible with any other action port with concrete signature
                if (a.IsAnyAction) return !b.IsAnyAction;
                if (b.IsAnyAction) return true;

                // Port a signature: System.Action, compatible with System.Action or any-action port
                if (!a.Signature.IsGenericType) return !b.Signature.IsGenericType;

                // Port a signature: System.Action<...>, not compatible with System.Action
                if (!b.Signature.IsGenericType) return false;

                var aArgs = a.Signature.GetGenericArguments();
                var bArgs = b.Signature.GetGenericArguments();

                if (aArgs.Length != bArgs.Length) return false;

                for (int i = 0; i < aArgs.Length; i++) {
                    if (aArgs[i] != bArgs[i]) return false;
                }

                return true;
            }

            if (a.IsFunc) {
                if (!b.IsFunc) return false;

                // Any-func port is compatible with any other func port with concrete signature
                if (a.IsAnyFunc) return !b.IsAnyFunc && !b.IsDynamicFunc;
                if (b.IsAnyFunc) return !a.IsDynamicFunc;

                var aArgs = a.Signature.GetGenericArguments();
                var bArgs = b.Signature.GetGenericArguments();

                if (aArgs.Length != bArgs.Length) return false;

                // Port a signature: System.Func<T> or System.Func<>, compatible with Func<T> and Func<> ports
                if (aArgs.Length == 1) {
                    // Func<> is not compatible with Func<>
                    if (a.IsDynamicFunc) return !b.IsDynamicFunc;
                    if (b.IsDynamicFunc) return true;
                }

                // Port a signature: System.Func<...>, compatible with same signature Func<...> ports
                for (int i = 0; i < aArgs.Length; i++) {
                    if (aArgs[i] != bArgs[i]) return false;
                }

                return true;
            }

            return a.Signature == b.Signature;
        }

        public static bool ValidateExternalPortWithExistingSignature(BlueprintAsset asset, Port port) {
            if (!port.IsInput && !port.IsAction) {
                Debug.LogWarning($"Subgraph node has blueprint `{asset.name}` " +
                                 $"which contains multiple external output ports with same name `{port.Name}`: " +
                                 $"this can cause incorrect results. " +
                                 $"Blueprint can have multiple external output ports with same name only for action-based output ports." +
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

            if (port.Signature == null) {
                return port.IsInput
                    ? ValidateAnyInputPort(blueprint, nodeMeta, portIndex)
                    : ValidateAnyOutputPort(blueprint, nodeMeta, portIndex);
            }

            if (port.IsAction) {
                if (port.IsAnyAction) {
                    return port.IsInput
                        ? ValidateAnyActionInputPort(blueprint, nodeMeta, portIndex)
                        : ValidateAnyActionOutputPort(blueprint, nodeMeta, portIndex);
                }

                return port.IsInput
                    ? ValidateActionInputPort(blueprint, nodeMeta, portIndex)
                    : ValidateActionOutputPort(blueprint, nodeMeta, portIndex);
            }

            if (port.IsFunc) {
                if (port.IsAnyFunc) {
                    return port.IsInput
                        ? ValidateAnyFuncInputPort(blueprint, nodeMeta, portIndex)
                        : ValidateAnyFuncOutputPort(blueprint, nodeMeta, portIndex);
                }

                if (port.IsDynamicFunc) {
                    return port.IsInput
                        ? ValidateDynamicFuncInputPort(blueprint, nodeMeta, portIndex)
                        : ValidateDynamicFuncOutputPort(blueprint, nodeMeta, portIndex);
                }

                return port.IsInput
                    ? ValidateFuncInputPort(blueprint, nodeMeta, portIndex)
                    : ValidateFuncOutputPort(blueprint, nodeMeta, portIndex);
            }

            return port.IsInput
                ? ValidateCustomInputPort(blueprint, nodeMeta, portIndex)
                : ValidateCustomOutputPort(blueprint, nodeMeta, portIndex);
        }

        private static bool ValidateAnyInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-input port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateAnyOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateAnyActionInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-action input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-action input port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateAnyActionOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-action output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-action output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateActionInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for action input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            if (!port.Signature.IsGenericType) {
                if (node is not (IBlueprintEnter or IBlueprintPortLinker)) {
                    Debug.LogError($"Blueprint `{blueprint.name}`: " +
                                   $"Validation failed for action input port {portIndex} of node {nodeMeta}: " +
                                   $"node class {node.GetType().Name} does not implement interface " +
                                   $"{nameof(IBlueprintEnter)} or " +
                                   $"{nameof(IBlueprintPortLinker)}.");
                    return false;
                }

                return true;
            }

            var genericArguments = port.Signature.GetGenericArguments();
            var iBlueprintEnterType = genericArguments.Length switch {
                1 => typeof(IBlueprintEnter<>),
                2 => typeof(IBlueprintEnter<,>),
                3 => typeof(IBlueprintEnter<,,>),
                4 => typeof(IBlueprintEnter<,,,>),
                5 => typeof(IBlueprintEnter<,,,,>),
                6 => typeof(IBlueprintEnter<,,,,,>),
                7 => typeof(IBlueprintEnter<,,,,,,>),
                8 => typeof(IBlueprintEnter<,,,,,,,>),
                9 => typeof(IBlueprintEnter<,,,,,,,,>),
                10 => typeof(IBlueprintEnter<,,,,,,,,,>),
                11 => typeof(IBlueprintEnter<,,,,,,,,,,>),
                12 => typeof(IBlueprintEnter<,,,,,,,,,,,>),
                13 => typeof(IBlueprintEnter<,,,,,,,,,,,,>),
                14 => typeof(IBlueprintEnter<,,,,,,,,,,,,,>),
                15 => typeof(IBlueprintEnter<,,,,,,,,,,,,,,>),
                _ => typeof(IBlueprintEnter<,,,,,,,,,,,,,,,>),
            };

            bool implementsEnter = ValidationUtils.GetGenericInterface(node.GetType(), iBlueprintEnterType, genericArguments) != null;
            if (!implementsEnter && node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for action input port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{iBlueprintEnterType.Name} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateActionOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for action output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateAnyFuncInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-func input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateAnyFuncOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-func output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not IBlueprintPortLinker) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for any-func output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicFuncInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic-func input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateDynamicFuncOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic-func output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;

            if (node is not (IBlueprintOutput or IBlueprintPortLinker)) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for dynamic-func output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{nameof(IBlueprintOutput)} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateFuncInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for func input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateFuncOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for func output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var node = nodeMeta.Node;
            var port = nodeMeta.Ports[portIndex];

            var genericArguments = port.Signature.GetGenericArguments();
            if (genericArguments.Length == 1) {
                if (node is not (IBlueprintOutput or IBlueprintPortLinker) &&
                    ValidationUtils.GetGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), genericArguments) == null
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

            var iBlueprintOutputType = genericArguments.Length switch {
                2 => typeof(IBlueprintOutput<,>),
                3 => typeof(IBlueprintOutput<,,>),
                4 => typeof(IBlueprintOutput<,,,>),
                5 => typeof(IBlueprintOutput<,,,,>),
                6 => typeof(IBlueprintOutput<,,,,,>),
                7 => typeof(IBlueprintOutput<,,,,,,>),
                8 => typeof(IBlueprintOutput<,,,,,,,>),
                9 => typeof(IBlueprintOutput<,,,,,,,,>),
                10 => typeof(IBlueprintOutput<,,,,,,,,,>),
                11 => typeof(IBlueprintOutput<,,,,,,,,,,>),
                12 => typeof(IBlueprintOutput<,,,,,,,,,,,>),
                13 => typeof(IBlueprintOutput<,,,,,,,,,,,,>),
                14 => typeof(IBlueprintOutput<,,,,,,,,,,,,,>),
                15 => typeof(IBlueprintOutput<,,,,,,,,,,,,,,>),
                16 => typeof(IBlueprintOutput<,,,,,,,,,,,,,,,>),
                _ => typeof(IBlueprintOutput<,,,,,,,,,,,,,,,,>),
            };

            if (node is not IBlueprintPortLinker &&
                ValidationUtils.GetGenericInterface(node.GetType(), iBlueprintOutputType, genericArguments) == null
            ) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for func output port {portIndex} of node {nodeMeta}: " +
                               $"node class {node.GetType().Name} does not implement interface " +
                               $"{iBlueprintOutputType.Name} or " +
                               $"{nameof(IBlueprintPortLinker)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateCustomInputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for custom input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }

        private static bool ValidateCustomOutputPort(BlueprintAsset blueprint, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Length - 1) {
                Debug.LogError($"Blueprint `{blueprint.name}`: " +
                               $"Validation failed for custom output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            return true;
        }
    }

}

#endif
