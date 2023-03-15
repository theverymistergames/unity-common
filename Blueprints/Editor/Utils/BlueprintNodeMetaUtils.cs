using System.Linq;
using System.Reflection;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Color;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Utils {

    public static class BlueprintNodeMetaUtils {

        public static string GetFormattedPortName(int index, Port port, bool richText) {
            string portName = port.Name?.Trim();
            string colorHex;

            if (port.IsData) {
                if (port.DataType == null) {
                    colorHex = BlueprintColors.Port.Header.Data;
                    portName ??= "?";
                }
                else {
                    var dataType = port.DataType;

                    colorHex = BlueprintColors.Port.Header.GetColorForType(dataType);
                    portName ??= TypeNameFormatter.GetTypeName(dataType);
                }
            }
            else {
                colorHex = BlueprintColors.Port.Header.Flow;
            }

            if (port.IsDisabled) colorHex = BlueprintColors.Port.Header.Disabled;

            string fullPortName = string.IsNullOrEmpty(portName)
                ? $"[{index}]"
                : port.IsLeftLayout ? $"[{index}] {portName}" : $"{portName} [{index}]";

            return richText ? $"<color={colorHex}>{fullPortName}</color>" : fullPortName;
        }

        public static string GetFormattedNodeName(BlueprintNodeMeta nodeMeta) {
            var nodeMetaAttr = nodeMeta.Node.GetType().GetCustomAttribute<BlueprintNodeMetaAttribute>(false);
            string nodeName = string.IsNullOrWhiteSpace(nodeMetaAttr.Name) ? nodeMeta.Node.GetType().Name : nodeMetaAttr.Name.Trim();
            return $"#{nodeMeta.NodeId} {nodeName}";
        }

        public static Color GetNodeColor(BlueprintNodeMeta nodeMeta) {
            var nodeMetaAttr = nodeMeta.Node.GetType().GetCustomAttribute<BlueprintNodeMetaAttribute>(false);
            string nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;
            return ColorUtils.HexToColor(nodeColor);
        }

        public static Color GetPortColor(Port port) {
            return port.IsDisabled ? BlueprintColors.Port.Connection.Disabled
                : port.IsData ? BlueprintColors.Port.Connection.GetColorForType(port.DataType)
                : BlueprintColors.Port.Connection.Flow;
        }
    }

}
