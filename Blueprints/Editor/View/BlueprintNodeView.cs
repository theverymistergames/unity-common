using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.View {

    public sealed class BlueprintNodeView : Node {

        private const float NODE_VIEW_MIN_LABEL_WIDTH = 50f;
        private const float NODE_VIEW_MIN_FIELD_WIDTH = 150f;

        private const float NODE_VIEW_MIN_ARRAY_LABEL_WIDTH = 100f;
        private const float NODE_VIEW_MIN_ARRAY_FIELD_WIDTH = 200f;

        private const float NODE_VIEW_LABEL_WIDTH_INCREMENT_BY_DEPTH = 40f;

        public Action<NodeId, Vector2> OnPositionChanged = delegate {  };
        public Action<NodeId> OnValidate = delegate {  };

        public readonly NodeId nodeId;

        private readonly BlueprintMeta2 _meta;
        private readonly SerializedObject _serializedObject;
        private readonly InspectorView _inspector;

        private readonly Dictionary<PortView, int> _portViewToPortIndexMap = new Dictionary<PortView, int>();
        private readonly Dictionary<int, PortView> _portIndexToPortViewMap = new Dictionary<int, PortView>();

        private int _nodePathSourceIndex;
        private int _nodePathNodeIndex;
        private string _nodePath;

        private uint _contentHashCache;
        private float _labelWidth = -1f;
        private float _fieldWidth = -1f;

        private struct PortViewCreationData {
            public int portIndex;
            public Port port;
        }

        public BlueprintNodeView(BlueprintMeta2 meta, NodeId nodeId, Vector2 position, SerializedObject serializedObject) : base(GetUxmlPath()) {
            this.nodeId = nodeId;
            _meta = meta;
            _serializedObject = serializedObject;
            viewDataKey = nodeId.ToString();

            if (FetchNodePath(forceRefresh: true)) {
                _contentHashCache = _serializedObject.FindProperty(_nodePath)?.contentHash ?? 0;
            }

            _inspector = this.Q<InspectorView>("inspector");
            _inspector.Inject(OnNodeGUI);

            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");

            var source = meta.GetNodeSource(nodeId);
            var nodeType = source?.NodeType;

            titleLabel.text = BlueprintNodeUtils.GetFormattedNodeName(nodeId, nodeType);
            container.style.backgroundColor = BlueprintNodeUtils.GetNodeColor(nodeType);

            style.left = position.x;
            style.top = position.y;

            capabilities &= ~Capabilities.Snappable;
        }

        private bool FetchNodePath(bool forceRefresh = false) {
            if (!_meta.TryGetNodePath(nodeId, out int sourceIndex, out int nodeIndex)) return false;

            if (_nodePathSourceIndex != sourceIndex || _nodePathNodeIndex != nodeIndex || forceRefresh) {
                _nodePath = BlueprintNodeUtils.GetNodePath(sourceIndex, nodeIndex);
            }

            _nodePathSourceIndex = sourceIndex;
            _nodePathNodeIndex = nodeIndex;

            return true;
        }

        public void DeInitialize() {
            _inspector.Clear();

            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();

            Clear();
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeId, new Vector2(newPos.xMin, newPos.yMin));
        }

        public void CreatePortViews(IEdgeConnectorListener connectorListener) {
            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();

            inputContainer.Clear();
            outputContainer.Clear();

            var portViewsCreationData = new List<PortViewCreationData>();
            int portCount = _meta.GetPortCount(nodeId);

            for (int i = 0; i < portCount; i++) {
                var port = _meta.GetPort(nodeId, i);
                portViewsCreationData.Add(new PortViewCreationData { portIndex = i, port = port });
            }

            portViewsCreationData = portViewsCreationData
                .Where(data => !data.port.IsHidden())
                .OrderBy(d => d.portIndex)
                .ToList();

            for (int i = 0; i < portViewsCreationData.Count; i++) {
                CreatePortView(portViewsCreationData[i], connectorListener);
            }

            RefreshPorts();
        }

        public PortView GetPortView(int portIndex) {
            return _portIndexToPortViewMap[portIndex];
        }

        public int GetPortIndex(PortView portView) {
            return _portViewToPortIndexMap[portView];
        }

        private void CreatePortView(PortViewCreationData data, IEdgeConnectorListener connectorListener) {
            var direction = data.port.IsLeftLayout() ? Direction.Input : Direction.Output;
            var capacity = data.port.IsMultiple() ? PortView.Capacity.Multi : PortView.Capacity.Single;
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
            if (!FetchNodePath() || _serializedObject.FindProperty(_nodePath) is not {} serializedProperty) return;

            var endProperty = serializedProperty.GetEndProperty();
            bool enterChildren = true;

            float labelWidthCache = EditorGUIUtility.labelWidth;
            float fieldWidthCache = EditorGUIUtility.fieldWidth;

            if (_labelWidth < 0f || _fieldWidth < 0f) {
                (_labelWidth, _fieldWidth) = CalculateLabelAndFieldWidth(serializedProperty);
            }

            EditorGUIUtility.labelWidth = _labelWidth;
            EditorGUIUtility.fieldWidth = _fieldWidth;

            bool hasProperties = false;

            EditorGUI.BeginChangeCheck();

            while (serializedProperty.NextVisible(enterChildren) && !SerializedProperty.DataEquals(serializedProperty, endProperty)) {
                hasProperties = true;

                enterChildren = false;
                EditorGUILayout.PropertyField(serializedProperty, true);

                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                    serializedProperty.objectReferenceValue is BlueprintAsset2 blueprint &&
                    GUILayout.Button("Edit")
                ) {
                    BlueprintEditorWindow.OpenAsset(blueprint);
                }
            }

            _serializedObject.ApplyModifiedProperties();

            uint contentHash = _serializedObject.FindProperty(_nodePath).contentHash;
            bool changed = EditorGUI.EndChangeCheck() || hasProperties && contentHash != _contentHashCache;
            _contentHashCache = contentHash;

            _serializedObject.Update();

            EditorGUIUtility.labelWidth = labelWidthCache;
            EditorGUIUtility.fieldWidth = fieldWidthCache;

            if (changed) {
                _labelWidth = -1f;
                _fieldWidth = -1f;
                OnValidate?.Invoke(nodeId);
                Debug.Log($"OnValidate {nodeId}, source {_meta.GetNodeSource(nodeId)}");
            }
        }

        private static (float, float) CalculateLabelAndFieldWidth(SerializedProperty property) {
            float totalWidth = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;

            float labelWidth = 0;
            float fieldWidth = 0;

            property = property.Copy();
            var endProperty = property.GetEndProperty();

            bool hasArrayFields = false;

            int basePropertyDepth = property.depth;
            int maxPropertyDepth = basePropertyDepth;

            while (property.NextVisible(true) && !SerializedProperty.DataEquals(property, endProperty)) {
                float labelTextWidth = EditorStyles.label.CalcSize(new GUIContent(property.displayName)).x;

                labelWidth = Mathf.Max(labelWidth, Mathf.Max(labelTextWidth + 20f, NODE_VIEW_MIN_LABEL_WIDTH));
                fieldWidth = Mathf.Max(fieldWidth, Mathf.Max(totalWidth - labelWidth, NODE_VIEW_MIN_FIELD_WIDTH));

                hasArrayFields |= property.propertyType == SerializedPropertyType.ArraySize;

                if (property.depth > maxPropertyDepth) maxPropertyDepth = property.depth;
            }

            if (hasArrayFields) {
                labelWidth = Mathf.Max(labelWidth, NODE_VIEW_MIN_ARRAY_LABEL_WIDTH);
                fieldWidth = Mathf.Max(fieldWidth, NODE_VIEW_MIN_ARRAY_FIELD_WIDTH);
            }

            int maxDepthDiff = maxPropertyDepth - basePropertyDepth;
            if (maxDepthDiff > 1) labelWidth += maxDepthDiff * NODE_VIEW_LABEL_WIDTH_INCREMENT_BY_DEPTH;

            return (labelWidth, fieldWidth);
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
