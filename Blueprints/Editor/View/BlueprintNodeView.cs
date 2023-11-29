using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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

        public Action<NodeId> OnPositionChanged = delegate {  };
        public Action<NodeId> OnValidate = delegate {  };

        public readonly NodeId nodeId;

        private readonly Dictionary<PortView, int> _portViewToPortIndexMap = new Dictionary<PortView, int>();
        private readonly Dictionary<int, PortView> _portIndexToPortViewMap = new Dictionary<int, PortView>();

        private struct PortViewCreationData {
            public int portIndex;
            public Port port;
        }

        public BlueprintNodeView(
            BlueprintMeta2 meta,
            IEdgeConnectorListener connectorListener,
            NodeId nodeId,
            Vector2 position,
            SerializedProperty property
        ) : base(GetUxmlPath())
        {
            this.nodeId = nodeId;
            viewDataKey = nodeId.ToString();

            var inspector = this.Q<VisualElement>("inspector");
            CreateNodeGUI(inspector, property);

            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");

            var source = meta.GetNodeSource(nodeId);
            var nodeType = source?.GetNodeType(nodeId.node);

            titleLabel.text = BlueprintNodeUtils.GetFormattedNodeName(nodeId, nodeType);
            container.style.backgroundColor = BlueprintNodeUtils.GetNodeColor(nodeType);

            style.left = position.x;
            style.top = position.y;

            CreatePortViews(meta, connectorListener);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeId);
        }

        private void CreateNodeGUI(VisualElement container, SerializedProperty property) {
            if (property == null) return;

            float minWidth = CalculateMinWidth(property);

            int depth = property.depth;
            var enumerator = property.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (enumerator.Current is not SerializedProperty childProperty) continue;
                if (childProperty.depth > depth + 1) continue;

                var propertyField = new PropertyField {
                    style = { minWidth = minWidth }
                };

                propertyField.BindProperty(property);
                propertyField.TrackPropertyValue(property, OnValueChanged);

                container.Add(propertyField);

                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue is BlueprintAsset2 asset
                ) {
                    var button = new Button(() => BlueprintEditorWindow.Open(asset)) { text = "Edit" };
                    container.Add(button);
                }
            }
        }

        private void OnValueChanged(SerializedProperty property) {
            OnValidate?.Invoke(nodeId);
        }

        public bool TryGetPortView(int portIndex, out PortView portView) {
            return _portIndexToPortViewMap.TryGetValue(portIndex, out portView);
        }

        public int GetPortIndex(PortView portView) {
            return _portViewToPortIndexMap[portView];
        }

        private void CreatePortViews(IBlueprintMeta meta, IEdgeConnectorListener connectorListener) {
            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();

            inputContainer.Clear();
            outputContainer.Clear();

            var portViewsCreationData = new List<PortViewCreationData>();
            int portCount = meta.GetPortCount(nodeId);

            for (int i = 0; i < portCount; i++) {
                var port = meta.GetPort(nodeId, i);
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

        private static float CalculateMinWidth(SerializedProperty property) {
            float labelWidth = NODE_VIEW_MIN_LABEL_WIDTH;
            float fieldWidth = NODE_VIEW_MIN_FIELD_WIDTH;

            property = property.Copy();
            bool hasArrayFields = false;

            int basePropertyDepth = property.depth;
            int maxPropertyDepth = basePropertyDepth;
            var enumerator = property.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (enumerator.Current is not SerializedProperty childProperty) continue;
                if (childProperty.depth > basePropertyDepth + 1) continue;

                hasArrayFields |= childProperty.propertyType == SerializedPropertyType.ArraySize;

                if (childProperty.depth > maxPropertyDepth) maxPropertyDepth = childProperty.depth;
            }

            if (hasArrayFields) {
                labelWidth = Mathf.Max(labelWidth, NODE_VIEW_MIN_ARRAY_LABEL_WIDTH);
                fieldWidth = Mathf.Max(fieldWidth, NODE_VIEW_MIN_ARRAY_FIELD_WIDTH);
            }

            int maxDepthDiff = maxPropertyDepth - basePropertyDepth;
            if (maxDepthDiff > 1) labelWidth += maxDepthDiff * NODE_VIEW_LABEL_WIDTH_INCREMENT_BY_DEPTH;

            return labelWidth + fieldWidth;
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
