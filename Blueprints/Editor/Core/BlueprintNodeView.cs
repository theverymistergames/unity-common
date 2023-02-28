using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Color;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintNodeView : Node {

        public readonly BlueprintNodeMeta nodeMeta;

        public Action<BlueprintNodeMeta, Vector2> OnPositionChanged = delegate {  };
        public Action<BlueprintNodeMeta> OnValidate = delegate {  };

        private readonly SerializedProperty _nodeProperty;

        private readonly Dictionary<PortView, int> _portViewToPortIndexMap = new Dictionary<PortView, int>();
        private readonly Dictionary<int, PortView> _portIndexToPortViewMap = new Dictionary<int, PortView>();

        private readonly InspectorView _inspector;

        private struct PortViewCreationData {
            public int portIndex;
            public Port port;
        }

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta, SerializedProperty nodeProperty) : base(GetUxmlPath()) {
            this.nodeMeta = nodeMeta;
            _nodeProperty = nodeProperty;
            var node = nodeMeta.Node;

            viewDataKey = nodeMeta.NodeId.ToString();

            _inspector = this.Q<InspectorView>("inspector");
            _inspector.Inject(OnNodeGUI);

            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var nodeMetaAttr = node.GetType().GetCustomAttribute<BlueprintNodeMetaAttribute>(false);

            titleLabel.text = GetFormattedNodeName(nodeMeta, nodeMetaAttr);
            container.style.backgroundColor = GetNodeColor(nodeMetaAttr);

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;
        }

        public void DeInitialize() {
            _inspector.Clear();

            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();

            Clear();
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeMeta, new Vector2(newPos.xMin, newPos.yMin));
        }

        public void CreatePortViews(IEdgeConnectorListener connectorListener) {
            var portViewsCreationData = nodeMeta.Ports
                .Select((port, index) => new PortViewCreationData { portIndex = index, port = port })
                .Where(data => !data.port.isExternalPort)
                .OrderBy(d => d.port.mode)
                .ThenBy(d => d.portIndex)
                .ToArray();

            for (int i = 0; i < portViewsCreationData.Length; i++) {
                CreatePortView(portViewsCreationData[i], connectorListener);
            }

            RefreshPorts();
        }

        public void ClearPortViews() {
            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();

            inputContainer.Clear();
            outputContainer.Clear();
        }

        public PortView GetPortView(int portIndex) {
            return _portIndexToPortViewMap[portIndex];
        }

        public int GetPortIndex(PortView portView) {
            return _portViewToPortIndexMap[portView];
        }

        private void CreatePortView(PortViewCreationData data, IEdgeConnectorListener connectorListener) {
            if (data.port.isExternalPort) return;

            Direction direction;
            PortView.Capacity capacity;
            VisualElement container;

            switch (data.port.mode) {
                case Port.Mode.Enter:
                    direction = Direction.Input;
                    capacity = PortView.Capacity.Multi;
                    container = inputContainer;
                    break;

                case Port.Mode.Exit:
                    direction = Direction.Output;
                    capacity = PortView.Capacity.Multi;
                    container = outputContainer;
                    break;

                case Port.Mode.Input:
                    direction = Direction.Input;
                    capacity = PortView.Capacity.Single;
                    container = inputContainer;
                    break;

                case Port.Mode.InputArray:
                    direction = Direction.Input;
                    capacity = PortView.Capacity.Multi;
                    container = inputContainer;
                    break;

                case Port.Mode.Output:
                    direction = Direction.Output;
                    capacity = PortView.Capacity.Multi;
                    container = outputContainer;
                    break;

                case Port.Mode.NonTypedInput:
                    direction = Direction.Input;
                    capacity = PortView.Capacity.Single;
                    container = inputContainer;
                    break;

                case Port.Mode.NonTypedOutput:
                    direction = Direction.Output;
                    capacity = PortView.Capacity.Multi;
                    container = outputContainer;
                    break;

                default:
                    throw new NotSupportedException($"Port mode {data.port.mode} is not supported");
            }

            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            portView.AddManipulator(new EdgeConnector<Edge>(connectorListener));

            portView.portName = GetFormattedPortName(data.portIndex, data.port);
            portView.portColor = GetPortColor(data.port);

            container.Add(portView);

            _portViewToPortIndexMap[portView] = data.portIndex;
            _portIndexToPortViewMap[data.portIndex] = portView;
        }

        private static string GetFormattedNodeName(BlueprintNodeMeta nodeMeta, BlueprintNodeMetaAttribute nodeMetaAttr) {
            string nodeName = string.IsNullOrWhiteSpace(nodeMetaAttr.Name) ? nodeMeta.Node.GetType().Name : nodeMetaAttr.Name.Trim();
            return $"#{nodeMeta.NodeId} {nodeName}";
        }

        private static Color GetNodeColor(BlueprintNodeMetaAttribute nodeMetaAttr) {
            string nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;
            return ColorUtils.HexToColor(nodeColor);
        }

        private static string GetFormattedPortName(int index, Port port) {
            return string.IsNullOrEmpty(port.name)
                ? port.mode switch {
                    Port.Mode.Enter => $"<color={BlueprintColors.Port.Header.Flow}>[{index}]</color>",
                    Port.Mode.Exit => $"<color={BlueprintColors.Port.Header.Flow}>[{index}]</color>",
                    Port.Mode.Input => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>[{index}] {TypeNameFormatter.GetTypeName(port.dataType)}</color>",
                    Port.Mode.Output => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>{TypeNameFormatter.GetTypeName(port.dataType)} [{index}]</color>",
                    Port.Mode.InputArray => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>[{index}] {TypeNameFormatter.GetTypeName(port.dataType)}[]</color>",
                    Port.Mode.NonTypedInput => $"<color={BlueprintColors.Port.Header.Data}>[{index}]</color>",
                    Port.Mode.NonTypedOutput => $"<color={BlueprintColors.Port.Header.Data}>[{index}]</color>",
                    _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
                }
                : port.mode switch {
                    Port.Mode.Enter => $"<color={BlueprintColors.Port.Header.Flow}>[{index}] {port.name.Trim()}</color>",
                    Port.Mode.Exit => $"<color={BlueprintColors.Port.Header.Flow}>{port.name.Trim()} [{index}]</color>",
                    Port.Mode.Input => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>[{index}] {port.name.Trim()}</color>",
                    Port.Mode.Output => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>{port.name.Trim()} [{index}]</color>",
                    Port.Mode.InputArray => $"<color={BlueprintColors.Port.Header.GetColorForType(port.dataType)}>[{index}] {port.name.Trim()}</color>",
                    Port.Mode.NonTypedInput => $"<color={BlueprintColors.Port.Header.Data}>[{index}] {port.name.Trim()}</color>",
                    Port.Mode.NonTypedOutput => $"<color={BlueprintColors.Port.Header.Data}>{port.name.Trim()} [{index}]</color>",
                    _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
                };
        }

        private static Color GetPortColor(Port port) {
            return port.mode switch {
                Port.Mode.Enter => BlueprintColors.Port.Connection.Flow,
                Port.Mode.Exit => BlueprintColors.Port.Connection.Flow,
                Port.Mode.Input => BlueprintColors.Port.Connection.GetColorForType(port.dataType),
                Port.Mode.Output => BlueprintColors.Port.Connection.GetColorForType(port.dataType),
                Port.Mode.InputArray => BlueprintColors.Port.Connection.GetColorForType(port.dataType),
                Port.Mode.NonTypedInput => BlueprintColors.Port.Connection.Data,
                Port.Mode.NonTypedOutput => BlueprintColors.Port.Connection.Data,
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported"),
            };
        }

        private void OnNodeGUI() {
            float labelWidthCache = EditorGUIUtility.labelWidth;
            float fieldWidthCache = EditorGUIUtility.fieldWidth;

            float totalWidth = labelWidthCache + fieldWidthCache;
            float floorLabelWidth = 0;
            float floorFieldWidth = 0;

            var nodePropertyCopy = _nodeProperty.Copy();
            var endProperty = nodePropertyCopy.GetEndProperty();
            bool enterChildren = true;
            while (nodePropertyCopy.NextVisible(enterChildren) && !SerializedProperty.DataEquals(nodePropertyCopy, endProperty)) {
                float labelTextWidth = EditorStyles.label.CalcSize(new GUIContent(nodePropertyCopy.displayName)).x;

                floorLabelWidth = Mathf.Max(floorLabelWidth, Mathf.Max(labelTextWidth + 6f, 40f));
                floorFieldWidth = Mathf.Max(floorFieldWidth, Mathf.Max(totalWidth - floorLabelWidth, 140f));

                enterChildren = false;
            }

            EditorGUIUtility.labelWidth = floorLabelWidth;
            EditorGUIUtility.fieldWidth = floorFieldWidth;

            nodePropertyCopy = _nodeProperty.Copy();

            endProperty = nodePropertyCopy.GetEndProperty();
            enterChildren = true;

            EditorGUI.BeginChangeCheck();

            while (nodePropertyCopy.NextVisible(enterChildren) && !SerializedProperty.DataEquals(nodePropertyCopy, endProperty)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(nodePropertyCopy, true);

                if (nodePropertyCopy.GetValue() is BlueprintAsset blueprint && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.OpenAsset(blueprint);
                }
            }

            if (EditorGUI.EndChangeCheck()) OnValidate?.Invoke(nodeMeta);

            EditorGUIUtility.labelWidth = labelWidthCache;
            EditorGUIUtility.fieldWidth = fieldWidthCache;
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
