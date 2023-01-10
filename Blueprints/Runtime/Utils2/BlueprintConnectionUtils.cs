using MisterGames.Blueprints.Core2;
using MisterGames.Blueprints.Ports;

namespace MisterGames.Blueprints.Utils2 {

    public static class BlueprintConnectionUtils {

        public static bool TryConnectNodes(IBlueprintRouter router, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            var fromNode = router.GetNode(fromNodeId);
            var toNode = router.GetNode(toNodeId);

            if (!fromNode.HasPort(fromPortIndex)) return false;
            if (!toNode.HasPort(toPortIndex)) return false;

            var fromPort = fromNode.Ports[fromPortIndex];
            var toPort = toNode.Ports[toPortIndex];
            if (!ArePortsCompatible(fromPort.Meta, toPort.Meta)) return false;

            // Ports are in data mode
            if (fromPort.Meta.isDataPort) {
                // fromPort is an output port, so adding link to the input port toPort
                if (fromPort.Meta.isExitPort) {
                    toPort.ClearLinks();
                    toPort.AddLink(fromNodeId, fromPortIndex);
                    toNode.SetPort(toPortIndex, toPort);
                    return true;
                }

                // fromPort is an input port, so adding link to fromPort
                fromPort.ClearLinks();
                fromPort.AddLink(toNodeId, toPortIndex);
                fromNode.SetPort(fromPortIndex, fromPort);
                return true;
            }

            // Ports are in flow mode
            // fromPort is an exit port, so adding link to fromPort
            if (fromPort.Meta.isExitPort) {
                fromPort.AddLink(toNodeId, toPortIndex);
                fromNode.SetPort(fromPortIndex, fromPort);
                return true;
            }

            // fromPort is an enter port, so adding link to the exit port toPort
            toPort.AddLink(fromNodeId, fromPortIndex);
            toNode.SetPort(toPortIndex, toPort);
            return true;
        }

        public static bool ArePortsCompatible(PortMeta a, PortMeta b) {
            // Must have different port direction
            if (a.isExitPort == b.isExitPort) return false;

            // Must be same mode (flow/data)
            if (a.isDataPort != b.isDataPort) return false;

            // Both ports are flow ports
            if (!a.isDataPort) return true;

            // Both ports are data ports, at least one port has no serialized type
            if (!a.hasDataType || !b.hasDataType) return true;

            // Must have same data type
            if (a.dataType != b.dataType) return false;

            return true;
        }

    }

}
