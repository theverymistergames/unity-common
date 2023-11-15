#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class LinkValidator2 {

        public static bool ValidateRootLink(
            BlueprintMeta2 meta,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            int port,
            NodeId rootId,
            int rootPort
        ) {
            int portCount = meta.GetPortCount(id);
            if (port < 0 || port >= portCount) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for external link of node {id} port {port}: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = meta.GetPort(id, port);

            if (portData.IsData() && !portData.IsInput() &&
                linkStorage.GetFirstLink(rootId.source, rootId.node, rootPort) >= 0
            ) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for external link of node {id} port {port}: " +
                               $"output external port with same signature was already added.");
                return false;
            }

            return true;
        }

        public static bool ValidateInternalLink(BlueprintMeta2 meta, NodeId id, int port, IBlueprintInternalLink link) {
            int portCount = meta.GetPortCount(id);
            if (port < 0 || port >= portCount) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for internal links of node {id} port {port}: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            var portData = meta.GetPort(id, port);
            if (portData.IsData() == portData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for internal links of node {id} port {port}: " +
                               $"port {port} must be enter or output to have internal links.");
                return false;
            }

            link.GetLinkedPorts(id, port, out int p, out int count);

            for (int end = p + count; p < end; p++) {
                if (p < 0 || p >= portCount) {
                    Debug.LogError($"Blueprint `{meta.Owner}`: " +
                                   $"Validation failed for internal link [node {id}, port {port} :: port {p}]: " +
                                   $"node {id} has no linked port {p}.");
                    return false;
                }

                portData = meta.GetPort(id, p);
                if (portData.IsData() != portData.IsInput()) {
                    Debug.LogError($"Blueprint `{meta.Owner}`: " +
                                   $"Validation failed for internal link [node {id}, port {port} :: port {p}]: " +
                                   $"linked port {p} must be exit or input.");
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateHashLink(IBlueprintHashLink link, BlueprintMeta2 meta, NodeId id) {
            if (!link.TryGetLinkedPort(id, out int hash, out int port)) return true;

            if (port < 0 || port >= meta.GetPortCount(id)) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for hash link of node {id} port {port}: " +
                               $"node {id} has no port with index {port}.");
                return false;
            }

            return true;
        }

        public static bool ValidateNodeLink(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (port < 0 || port > meta.GetPortCount(id) - 1) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
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
            Debug.LogError($"Blueprint `{meta.Owner}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is enter port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromExitPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = meta.GetPort(toId, toPort);

            if (!toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source exit port cannot have link to the data-based input port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicInputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var toPortData = meta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the non-data output port.");
                return false;
            }

            if (toPortData.DataType == null) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source dynamic input port cannot have link to the dynamic output port.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromDynamicOutputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{meta.Owner}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is dynamic output port and it cannot have links.");
            return false;
        }

        private static bool ValidateLinkFromInputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            if (toPort < 0 || toPort > meta.GetPortCount(toId) - 1) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"node {toId} has no port with index {toPort}.");
                return false;
            }

            var fromPortData = meta.GetPort(id, port);
            var toPortData = meta.GetPort(toId, toPort);

            if (toPortData.IsInput()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have same direction.");
                return false;
            }

            if (!toPortData.IsData()) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"source input port cannot have link to the non-data output port.");
                return false;
            }

            var fromPortDataType = fromPortData.DataType;
            var toPortDataType = toPortData.DataType;

            if (toPortDataType == null) return true;

            if (fromPortData.IsMultiple() && toPortDataType.IsArray) {
                if (toPortDataType.GetElementType() != fromPortDataType) {
                    Debug.LogError($"Blueprint `{meta.Owner}`: " +
                                   $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                                   $"ports have different signature.");
                    return false;
                }

                return true;
            }

            if (toPortData.AcceptSubclass() && !toPortDataType.IsAssignableFrom(fromPortDataType)) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            if (!fromPortDataType.IsAssignableFrom(toPortDataType)) {
                Debug.LogError($"Blueprint `{meta.Owner}`: " +
                               $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                               $"ports have different signature.");
                return false;
            }

            return true;
        }

        private static bool ValidateLinkFromOutputPort(BlueprintMeta2 meta, NodeId id, int port, NodeId toId, int toPort) {
            Debug.LogError($"Blueprint `{meta.Owner}`: " +
                           $"Validation failed for link [node {id}, port {port} :: node {toId}, port {toPort}]: " +
                           $"port {port} of node {id} is func output port and it cannot have links.");
            return false;
        }
    }

}

#endif
