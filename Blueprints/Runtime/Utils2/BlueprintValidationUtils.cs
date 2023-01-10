using System;
using MisterGames.Blueprints.Core2;
using UnityEngine;

namespace MisterGames.Blueprints.Utils2 {

    internal static class BlueprintValidationUtils {

        public static bool ValidateEnterPort(BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (port.Meta.isExitPort || port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {node}: " +
                               $"this port is not an enter port.");
                return false;
            }

            if (port.Links.Count > 0) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {node}: " +
                               $"port has {port.Links.Count} links, but enter port must not have links.");
                return false;
            }

            if (node is not IBlueprintEnter) {
                Debug.LogError($"Validation failed for enter port {portIndex} of node {node}: " +
                               $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintEnter)}.");
                return false;
            }

            return true;
        }

        public static bool ValidateExitPort(BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for exit port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (!port.Meta.isExitPort || port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for exit port {portIndex} of node {node}: " +
                               $"this port is not an exit port.");
                return false;
            }

            return true;
        }

        public static bool ValidateInputPort(BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (port.Meta.isExitPort || !port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"this port is not an input port.");
                return false;
            }

            if (port.Links.Count > 1) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"port has {port.Links.Count} links, but must have only 0 or 1 links.");
                return false;
            }

            return true;
        }

        public static bool ValidateInputPort(Type dataType, BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (port.Meta.isExitPort || !port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"this port is not an input port.");
                return false;
            }

            if (port.Meta.hasDataType && port.Meta.dataType != dataType) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"this input port type is not {dataType.Name}.");
                return false;
            }

            if (port.Links.Count > 1) {
                Debug.LogError($"Validation failed for input port {portIndex} of node {node}: " +
                               $"port has {port.Links.Count} links, but must have only 0 or 1 links.");
                return false;
            }

            return true;
        }

        public static bool ValidateInputPort<T>(BlueprintNode node, int portIndex) {
            return ValidateInputPort(typeof(T), node, portIndex);
        }

        public static bool ValidateOutputPort(BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (!port.Meta.isExitPort || !port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (port.Links.Count > 0) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"port has {port.Links.Count} links, but output port must not have links.");
                return false;
            }

            if (!port.Meta.hasDataType && node is not IBlueprintOutput) {
                Debug.LogError($"Validation failed for non-typed output port {portIndex} of node {node}: " +
                               $"node class {node.GetType().Name} does not implement interface {nameof(IBlueprintOutput)}.");
                return false;
            }

            if (port.Meta.hasDataType &&
                !HasGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.Meta.dataType)
            ) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"node class {node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{port.Meta.dataType.Name}>.");
                return false;
            }

            return true;
        }

        public static bool ValidateOutputPort(Type dataType, BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];
            if (!port.Meta.isExitPort || !port.Meta.isDataPort) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"this port is not an output port.");
                return false;
            }

            if (port.Links.Count > 0) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"port has {port.Links.Count} links, but output port must not have links.");
                return false;
            }

            if (!port.Meta.hasDataType) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"port has no type, but needs to have type {dataType.Name}.");
                return false;
            }

            if (port.Meta.dataType != dataType) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"port type is not {dataType.Name}.");
                return false;
            }

            if (!HasGenericInterface(node.GetType(), typeof(IBlueprintOutput<>), port.Meta.dataType)) {
                Debug.LogError($"Validation failed for output port {portIndex} of node {node}: " +
                               $"node class {node.GetType().Name} does not implement " +
                               $"interface {typeof(IBlueprintOutput<>).Name}<{dataType.Name}>.");
                return false;
            }

            return true;
        }

        public static bool ValidateOutputPort<T>(BlueprintNode node, int portIndex) {
            return ValidateOutputPort(typeof(T), node, portIndex);
        }

        public static bool ValidatePort(BlueprintNode node, int portIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for port {portIndex} of node {node}: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];

            // Enter port
            if (!port.Meta.isDataPort && !port.Meta.isExitPort) {
                return ValidateEnterPort(node, portIndex);
            }

            // Exit port
            if (!port.Meta.isDataPort && port.Meta.isExitPort) {
                return ValidateExitPort(node, portIndex);
            }

            // Input port
            if (!port.Meta.isExitPort) {
                return port.Meta.hasDataType
                    ? ValidateInputPort(port.Meta.dataType, node, portIndex)
                    : ValidateInputPort(node, portIndex);
            }

            // Output port
            return port.Meta.hasDataType
                ? ValidateOutputPort(port.Meta.dataType, node, portIndex)
                : ValidateOutputPort(node, portIndex);
        }

        public static bool ValidateLink(BlueprintNode node, int portIndex, BlueprintNode toNode, int toPortIndex) {
            if (!node.HasPort(portIndex)) {
                Debug.LogError($"Validation failed for port link [node {node}, port {portIndex} :: node {toNode}, port {toPortIndex}]: " +
                               $"{node} has no port with index {portIndex}.");
                return false;
            }

            var port = node.Ports[portIndex];

            // Enter port
            if (!port.Meta.isDataPort && !port.Meta.isExitPort) {
                Debug.LogError($"Validation failed for port link [node {node}, port {portIndex} :: node {toNode}, port {toPortIndex}]: " +
                               $"port {portIndex} of node {node} is enter port and it cannot have links.");
                return false;
            }

            // Exit port
            if (!port.Meta.isDataPort && port.Meta.isExitPort) {
                return ValidateEnterPort(toNode, toPortIndex);
            }

            // Input port
            if (!port.Meta.isExitPort) {
                return port.Meta.hasDataType
                    ? ValidateOutputPort(port.Meta.dataType, toNode, toPortIndex)
                    : ValidateOutputPort(toNode, toPortIndex);
            }

            // Output port
            Debug.LogError($"Validation failed for port link [node {node}, port {portIndex} :: node {toNode}, port {toPortIndex}]: " +
                           $"port {portIndex} of node {node} is output port and it cannot have links.");
            return false;
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
