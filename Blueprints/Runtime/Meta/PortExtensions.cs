using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;

namespace MisterGames.Blueprints {

    public static class PortExtensions {

        public static Port Layout(this Port port, PortLayout layout) {
            switch (layout) {
                case PortLayout.Default:
                    port.mode &= ~(PortMode.LayoutLeft | PortMode.LayoutRight);
                    break;

                case PortLayout.Left:
                    port.mode &= ~PortMode.LayoutRight;
                    port.mode |= PortMode.LayoutLeft;
                    break;

                case PortLayout.Right:
                    port.mode &= ~PortMode.LayoutLeft;
                    port.mode |= PortMode.LayoutRight;
                    break;
            }

            return port;
        }

        public static Port Capacity(this Port port, PortCapacity capacity) {
            switch (capacity) {
                case PortCapacity.Default:
                    port.mode &= ~(PortMode.CapacitySingle | PortMode.CapacityMultiple);
                    break;

                case PortCapacity.Single:
                    port.mode &= ~PortMode.CapacityMultiple;
                    port.mode |= PortMode.CapacitySingle;
                    break;

                case PortCapacity.Multiple:
                    port.mode &= ~PortMode.CapacitySingle;
                    port.mode |= PortMode.CapacityMultiple;
                    break;
            }

            return port;
        }

        public static Port Hide(this Port port, bool isHidden) {
            if (isHidden) port.options |= PortOptions.Hidden;
            else port.options &= ~PortOptions.Hidden;

            return port;
        }

        public static bool IsInput(this Port port) {
            return (port.mode & PortMode.Input) == PortMode.Input;
        }

        public static bool IsData(this Port port) {
            return (port.mode & PortMode.Data) == PortMode.Data;
        }

        public static bool IsExternal(this Port port) {
            return (port.options & PortOptions.External) == PortOptions.External;
        }

        public static bool IsHidden(this Port port) {
            return (port.options & PortOptions.Hidden) == PortOptions.Hidden;
        }

        public static bool IsMultiple(this Port port) {
            return (port.mode & PortMode.CapacitySingle) != PortMode.CapacitySingle &&
                   (port.mode & PortMode.CapacityMultiple) != PortMode.CapacityMultiple
                ? !port.IsInput() || !port.IsData()
                : (port.mode & PortMode.CapacityMultiple) == PortMode.CapacityMultiple;
        }

        public static bool IsLeftLayout(this Port port) {
            return (port.mode & PortMode.LayoutLeft) != PortMode.LayoutLeft &&
                   (port.mode & PortMode.LayoutRight) != PortMode.LayoutRight
                ? port.IsInput()
                : (port.mode & PortMode.LayoutLeft) == PortMode.LayoutLeft;
        }

        public static int GetSignature(this Port port) {
            return HashCode.Combine(
                port.mode,
                port.dataType,
                string.IsNullOrWhiteSpace(port.name) ? string.Empty : port.name
            );
        }

        public static bool AcceptSubclass(this Port port) {
            return (port.options & PortOptions.AcceptSubclass) == PortOptions.AcceptSubclass;
        }

        internal static Port External(this Port port, bool isExternal) {
            if (isExternal) port.options |= PortOptions.External;
            else port.options &= ~PortOptions.External;

            return port;
        }

        internal static void FetchExternalPorts(IBlueprintMeta meta, NodeId id, BlueprintAsset asset) {
            if (asset == null) return;

            var subgraphMeta = asset.BlueprintMeta;
            var nodes = subgraphMeta.Nodes;
            var portSignatureSet = new HashSet<int>();

            foreach (var subgraphNodeId in nodes) {
                int portCount = subgraphMeta.GetPortCount(subgraphNodeId);

                for (int p = 0; p < portCount; p++) {
                    var port = subgraphMeta.GetPort(subgraphNodeId, p);
                    if (!port.IsExternal()) continue;

                    int sign = port.GetSignature();

                    if (portSignatureSet.Contains(sign)) {
                        PortValidator.ValidateExternalPortWithExistingSignature(subgraphMeta, port);
                        continue;
                    }

                    portSignatureSet.Add(sign);
                    meta.AddPort(id, port.External(false).Hide(false));
                }
            }
        }
    }

}
