using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Editor.SerializedProperties;
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

            viewDataKey = nodeMeta.NodeId.ToString();

            _inspector = this.Q<InspectorView>("inspector");
            _inspector.Inject(OnNodeGUI);

            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");

            titleLabel.text = BlueprintNodeMetaUtils.GetFormattedNodeName(nodeMeta);
            container.style.backgroundColor = BlueprintNodeMetaUtils.GetNodeColor(nodeMeta);

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;

            capabilities &= ~Capabilities.Snappable;
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
                .Where(data => !data.port.IsHidden)
                .OrderBy(d => d.portIndex)
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
            var direction = data.port.IsLeftLayout ? Direction.Input : Direction.Output;
            var capacity = data.port.IsMultiple ? PortView.Capacity.Multi : PortView.Capacity.Single;
            var container = direction == Direction.Input ? inputContainer : outputContainer;

            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            portView.AddManipulator(new EdgeConnector<Edge>(connectorListener));

            portView.portName = BlueprintNodeMetaUtils.GetFormattedPortName(data.portIndex, data.port, richText: true);
            portView.portColor = BlueprintNodeMetaUtils.GetPortColor(data.port);

            container.Add(portView);

            _portViewToPortIndexMap[portView] = data.portIndex;
            _portIndexToPortViewMap[data.portIndex] = portView;
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

                floorLabelWidth = Mathf.Max(floorLabelWidth, Mathf.Max(labelTextWidth + 6f, 50f));
                floorFieldWidth = Mathf.Max(floorFieldWidth, Mathf.Max(totalWidth - floorLabelWidth, 150f));

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

                if (nodePropertyCopy.propertyType == SerializedPropertyType.ObjectReference &&
                    nodePropertyCopy.objectReferenceValue is BlueprintAsset blueprint &&
                    GUILayout.Button("Edit")
                ) {
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
