using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    internal static class BlueprintValidation {

        public static bool ValidatePort(BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            // Enter port
            if (!port.isDataPort && !port.isExitPort) {
                return ValidateEnterPort(nodeMeta, portIndex);
            }

            // Exit port
            if (!port.isDataPort) {
                return ValidateExitPort(nodeMeta, portIndex);
            }

            // Input port
            if (!port.isExitPort) {
                return port.hasDataType
                    ? ValidateInputPort(port.DataType, nodeMeta, portIndex)
                    : ValidateInputPort(nodeMeta, portIndex);
            }

            // Output port
            return port.hasDataType
                ? ValidateOutputPort(port.DataType, nodeMeta, portIndex)
                : ValidateOutputPort(nodeMeta, portIndex);
        }

        public static bool ValidateLink(BlueprintNodeMeta nodeMeta, int portIndex, BlueprintNodeMeta toNodeMeta, int toPortIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];

            // Enter port
            if (!port.isDataPort && !port.isExitPort) {
                Debug.LogError($"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                               $"port {portIndex} of node {nodeMeta} is enter port and it cannot have links.");
                return false;
            }

            // Exit port
            if (!port.isDataPort) {
                return ValidateEnterPort(toNodeMeta, toPortIndex);
            }

            // Input port
            if (!port.isExitPort) {
                return port.hasDataType
                    ? ValidateOutputPort(port.DataType, toNodeMeta, toPortIndex)
                    : ValidateOutputPort(toNodeMeta, toPortIndex);
            }

            // Output port
            Debug.LogError($"Validation failed for port link [node {nodeMeta}, port {portIndex} :: node {toNodeMeta}, port {toPortIndex}]: " +
                           $"port {portIndex} of node {nodeMeta} is output port and it cannot have links.");
            return false;
        }

        private static bool ValidateEnterPort(BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || port.isDataPort) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an enter port.");
                return false;
            }

            if (nodeMeta.Node is not IBlueprintEnter) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement interface {nameof(IBlueprintEnter)}.");
                return false;
            }

            return true;
        }

        private static bool ValidateExitPort(BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || port.isDataPort) {
                Debug.LogError($"Validation failed for exit port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an exit port.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputPort(BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateInputPort(Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an input port.");
                return false;
            }

            if (port.hasDataType && port.DataType != dataType) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {nodeMeta}: " +
                               $"this input port type is not {dataType.Name}.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (!port.hasDataType && nodeMeta.Node is not IBlueprintOutput) {
                Debug.LogError($"Validation failed for non-typed output port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement interface {nameof(IBlueprintOutput)}.");
                return false;
            }
            
            if (port.hasDataType &&
                !HasGenericInterface(nodeMeta.Node.GetType(), typeof(IBlueprintOutput<>), port.DataType)
            ) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"node class {nodeMeta.Node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.DataType.Name}>.");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputPort(Type dataType, BlueprintNodeMeta nodeMeta, int portIndex) {
            if (portIndex < 0 || portIndex > nodeMeta.Ports.Count - 1) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"{nodeMeta} has no port with index {portIndex}.");
                return false;
            }

            var port = nodeMeta.Ports[portIndex];
            if (!port.isExitPort || !port.isDataPort) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (!port.hasDataType) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port has no type, but needs to have type {dataType.Name}.");
                return false;
            }
            
            if (port.DataType != dataType) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            if (!HasGenericInterface(nodeMeta.Node.GetType(), typeof(IBlueprintOutput<>), dataType)) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {nodeMeta}: " +
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
