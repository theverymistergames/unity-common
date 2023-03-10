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

            if (port.IsAction) {
                colorHex = BlueprintColors.Port.Header.Flow;
                if (string.IsNullOrEmpty(portName)) {
                    portName = $"({string.Join(", ", port.Signature.GetGenericArguments().Select(TypeNameFormatter.GetTypeName))})";
                }
            }
            else if (port.IsFunc) {
                var genericArguments = port.Signature.GetGenericArguments();
                var returnType = genericArguments[^1];

                if (returnType.IsGenericTypeParameter) {
                    colorHex = BlueprintColors.Port.Header.Data;
                    if (string.IsNullOrEmpty(portName)) portName = "?";
                }
                else {
                    colorHex = BlueprintColors.Port.Header.GetColorForType(returnType);
                    string returnTypeName = TypeNameFormatter.GetTypeName(returnType);

                    if (string.IsNullOrEmpty(portName)) {
                        portName = genericArguments.Length == 1
                            ? returnTypeName
                            : $"({string.Join(", ", genericArguments[..^2].Select(TypeNameFormatter.GetTypeName))}) => {returnTypeName}";
                    }
                }
            }
            else {
                colorHex = BlueprintColors.Port.Header.Default;
                if (string.IsNullOrEmpty(portName)) portName = TypeNameFormatter.GetTypeName(port.Signature);
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
            if (port.IsDisabled) return BlueprintColors.Port.Connection.Disabled;

            if (port.IsAction) return BlueprintColors.Port.Connection.Flow;

            if (port.IsFunc) {
                var genericArguments = port.Signature.GetGenericArguments();
                if (genericArguments.Length == 0) return BlueprintColors.Port.Connection.Data;

                return BlueprintColors.Port.Connection.GetColorForType(genericArguments[^1]);
            }

            return BlueprintColors.Port.Connection.GetColorForType(port.Signature);
        }
    }

}
