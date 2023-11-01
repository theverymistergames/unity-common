using System;
using System.Reflection;
using MisterGames.Common.Colors;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Utils {

    public static class BlueprintNodeUtils {

        public static string GetFormattedPortName(int index, Port port, bool richText) {
            string portName = port.Name;
            string colorHex;

            if (port.IsData()) {
                if (port.DataType == null) {
                    colorHex = BlueprintColors.Port.Header.Data;
                    if (string.IsNullOrEmpty(portName)) portName = "?";
                }
                else {
                    var dataType = port.DataType;

                    colorHex = BlueprintColors.Port.Header.GetColorForType(dataType);
                    if (string.IsNullOrEmpty(portName)) portName = $"{TypeNameFormatter.GetTypeName(dataType)}{(port.IsInput() && port.IsMultiple() ? " (multi)" : string.Empty)}";
                }
            }
            else {
                colorHex = BlueprintColors.Port.Header.Flow;
            }

            string fullPortName = string.IsNullOrEmpty(portName)
                ? $"[{index}]"
                : port.IsLeftLayout() ? $"[{index}] {portName}" : $"{portName} [{index}]";

            return richText ? $"<color={colorHex}>{fullPortName}</color>" : fullPortName;
        }

        public static string GetFormattedNodeName(NodeId id, Type type) {
            if (type == null) return $"#{id.source}.{id.node}";

            var attr = type.GetCustomAttribute<BlueprintNodeAttribute>(false);
            return $"#{id.source}.{id.node} {(string.IsNullOrWhiteSpace(attr?.Name) ? type.Name : attr.Name.Trim())}";
        }

        public static Color GetNodeColor(Type type) {
            if (type == null) return ColorUtils.HexToColor(BlueprintColors.Node.Default);

            var nodeMetaAttr = type.GetCustomAttribute<BlueprintNodeAttribute>(false);
            string nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;
            return ColorUtils.HexToColor(nodeColor);
        }

        public static Color GetPortColor(Port port) {
            return port.IsData()
                ? BlueprintColors.Port.Connection.GetColorForType(port.DataType)
                : BlueprintColors.Port.Connection.Flow;
        }
    }

}
