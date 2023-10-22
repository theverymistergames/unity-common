#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    internal static class LinkValidator2 {

        public static bool ValidateLink(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            if (port < 0 || port > asset.BlueprintMeta.GetPortCount(id) - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = asset.BlueprintMeta.GetPort(id, port);

            if (portData.IsData()) {
                if (portData.DataType == null) {
                    return portData.IsInput()
                        ? ValidateLinkFromDynamicInputPort(asset, id, port, toId, toPort)
                        : ValidateLinkFromDynamicOutputPort(asset, id, port, toId, toPort);
                }

                return portData.IsInput()
                    ? ValidateLinkFromInputPort(asset, id, port, toId, toPort)
                    : ValidateLinkFromOutputPort(asset, id, port, toId, toPort);
            }

            return portData.IsInput()
                ? ValidateLinkFromEnterPort(asset, id, port, toId, toPort)
                : ValidateLinkFromExitPort(asset, id, port, toId, toPort);
        }

        private static bool ValidateLinkFromEnterPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is enter port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromExitPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > asset.BlueprintMeta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = asset.BlueprintMeta.GetPort(toId, toPort);

            if (!toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPortData.IsData()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source exit port cannot have link to the data-based input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicInputPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > asset.BlueprintMeta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = asset.BlueprintMeta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the non-data output port.");
                return false;
            }

            if (toPortData.DataType == null) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the dynamic output port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicOutputPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is dynamic output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromInputPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > asset.BlueprintMeta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var fromPortData = asset.BlueprintMeta.GetPort(id, port);
            var toPortData = asset.BlueprintMeta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source input port cannot have link to the non-data output port.");
                return false;
            }

            var fromPortDataType = fromPortData.DataType;
            var toPortDataType = toPortData.DataType;

            if (toPortDataType == null) return true;

            if (fromPortData.IsMultiple() && toPortDataType.IsArray) {
                if (toPortDataType.GetElementType() != fromPortDataType) {
                    Debug.LogError($"Blueprint `{asset.name}`: " +
                                   $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                                   $"ports have different signature.");
                    return false;
                }

                return true;
            }

            if (toPortData.AcceptSubclass() && !toPortDataType.IsAssignableFrom(fromPortDataType)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            if (!fromPortDataType.IsAssignableFrom(toPortDataType)) {
                Debug.LogError($"Blueprint `{asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromOutputPort(BlueprintAsset2 asset, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{asset.name}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is func output port and it cannot have links.");
            return false;
        }
    }

}

#endif
