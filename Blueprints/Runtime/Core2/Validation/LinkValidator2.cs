#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    internal static class LinkValidator2 {

        public static bool ValidateInternalLink(BlueprintMeta2 meta, NodeId id, int port, IBlueprintInternalLink link) {
            int portCount = meta.GetPortCount(id);
            if (port < 0 || port >= portCount) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for internal links [node {id}, port {port}]: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = meta.GetPort(id, port);
            if (portData.IsData() == portData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for internal links of node {id} port {port}: " +
                               $"port {port} must be enter or output to have internal links.");
                return false;
            }

            link.GetLinkedPorts(id, port, out int index, out int count);

            for (; index < count; index++) {
                if (index < 0 || index >= portCount) {
                    Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                                   $"Validation failed for internal links [node {id}, port {port} :: port {index}]: " +
                                   $"node {id} has no linked port {index}.");
                    return false;
                }

                portData = meta.GetPort(id, index);
                if (portData.IsData() != portData.IsInput()) {
                    Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                                   $"Validation failed for internal link [node {id}, port {port} :: port {index}]: " +
                                   $"linked port {index} must be exit or input.");
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateHashLink(IBlueprintHashLink link, BlueprintMeta2 meta, NodeId id) {
            link.GetLinkedPort(id, out int hash, out int port);

            if (port < 0 || port >= meta.GetPortCount(id)) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for hash link of node {id} port {port}: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = meta.GetPort(id, port);
            if (portData.IsData() != portData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for hash link of node {id} port {port}: " +
                               $"port {port} must be exit or input to have hash links.");
                return false;
            }

            return true;
        }

        public static bool ValidateNodeLink(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (port < 0 || port > meta.GetPortCount(id) - 1) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = meta.GetPort(id, port);

            if (portData.IsData()) {
                if (portData.DataType == null) {
                    return portData.IsInput()
                        ? ValidateLinkFromDynamicInputPort(meta, id, port, toId, toPort)
                        : ValidateLinkFromDynamicOutputPort(meta, id, port, toId, toPort);
                }

                return portData.IsInput()
                    ? ValidateLinkFromInputPort(meta, id, port, toId, toPort)
                    : ValidateLinkFromOutputPort(meta, id, port, toId, toPort);
            }

            return portData.IsInput()
                ? ValidateLinkFromEnterPort(meta, id, port, toId, toPort)
                : ValidateLinkFromExitPort(meta, id, port, toId, toPort);
        }

        private static bool ValidateLinkFromEnterPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is enter port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromExitPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = meta.GetPort(toId, toPort);

            if (!toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source exit port cannot have link to the data-based input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicInputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = meta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the non-data output port.");
                return false;
            }

            if (toPortData.DataType == null) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the dynamic output port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicOutputPort(BlueprintMeta2 asset, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{asset.Asset}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is dynamic output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromInputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var fromPortData = meta.GetPort(id, port);
            var toPortData = meta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source input port cannot have link to the non-data output port.");
                return false;
            }

            var fromPortDataType = fromPortData.DataType;
            var toPortDataType = toPortData.DataType;

            if (toPortDataType == null) return true;

            if (fromPortData.IsMultiple() && toPortDataType.IsArray) {
                if (toPortDataType.GetElementType() != fromPortDataType) {
                    Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                                   $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                                   $"ports have different signature.");
                    return false;
                }

                return true;
            }

            if (toPortData.AcceptSubclass() && !toPortDataType.IsAssignableFrom(fromPortDataType)) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            if (!fromPortDataType.IsAssignableFrom(toPortDataType)) {
                Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromOutputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{meta.Asset.name}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is func output port and it cannot have links.");
            return false;
        }
    }

}

#endif
