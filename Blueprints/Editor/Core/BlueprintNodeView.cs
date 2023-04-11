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

        private float _labelWidth = -1f;
        private float _fieldWidth = -1f;

        private string _lastNodeJson;

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

            _lastNodeJson = JsonUtility.ToJson(nodeMeta.Node);
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
            var nodePropertyCopy = _nodeProperty.Copy();
            var endProperty = nodePropertyCopy.GetEndProperty();
            bool enterChildren = true;

            bool changed = false;
            EditorGUI.BeginChangeCheck();

            float labelWidthCache = EditorGUIUtility.labelWidth;
            float fieldWidthCache = EditorGUIUtility.fieldWidth;

            if (_labelWidth < 0f || _fieldWidth < 0f) {
                (_labelWidth, _fieldWidth) = CalculateLabelAndFieldWidth(_nodeProperty);
            }

            EditorGUIUtility.labelWidth = _labelWidth;
            EditorGUIUtility.fieldWidth = _fieldWidth;

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

            EditorGUIUtility.labelWidth = labelWidthCache;
            EditorGUIUtility.fieldWidth = fieldWidthCache;

            changed |= EditorGUI.EndChangeCheck();

            string nodeJson = JsonUtility.ToJson(nodeMeta.Node);
            changed |= nodeJson != _lastNodeJson;

            _lastNodeJson = nodeJson;

            if (changed) {
                _labelWidth = -1f;
                _fieldWidth = -1f;
                OnValidate?.Invoke(nodeMeta);
            }
        }

        private static (float, float) CalculateLabelAndFieldWidth(SerializedProperty property) {
            float totalWidth = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;
            float labelWidth = 0;
            float fieldWidth = 0;

            property = property.Copy();
            var endProperty = property.GetEndProperty();

            while (property.NextVisible(true) && !SerializedProperty.DataEquals(property, endProperty)) {
                float labelTextWidth = EditorStyles.label.CalcSize(new GUIContent(property.displayName)).x;

                labelWidth = Mathf.Max(labelWidth, Mathf.Max(labelTextWidth + 6f, 50f));
                fieldWidth = Mathf.Max(fieldWidth, Mathf.Max(totalWidth - labelWidth, 150f));
            }

            return (labelWidth, fieldWidth);
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
