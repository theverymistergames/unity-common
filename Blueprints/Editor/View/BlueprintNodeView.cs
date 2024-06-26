﻿using System;
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

        private const float NODE_VIEW_MIN_WIDTH = 200f;
        private const float NODE_VIEW_WIDTH_INCREMENT_MANAGED_REFERENCE = 70f;
        private const float NODE_VIEW_WIDTH_INCREMENT_ARRAY = 70f;
        private const float NODE_VIEW_WIDTH_INCREMENT_DEPTH = 70f;

        public Action<NodeId> OnPositionChanged = delegate {  };
        public Action<NodeId> OnValidate = delegate {  };
        public Action<NodeId, int> OnRemoveFromGroup = delegate {  };
        
        public readonly NodeId nodeId;
        public readonly VisualElement _inspector;
        public readonly SerializedProperty _property;

        private readonly Dictionary<PortView, int> _portViewToPortIndexMap = new Dictionary<PortView, int>();
        private readonly Dictionary<int, PortView> _portIndexToPortViewMap = new Dictionary<int, PortView>();
        private readonly BlueprintMeta _blueprintMeta;
        private readonly Button _expandButton;

        private struct PortViewCreationData {
            public int portIndex;
            public Port port;
        }

        public BlueprintNodeView(
            BlueprintMeta meta,
            IEdgeConnectorListener connectorListener,
            NodeId nodeId,
            Vector2 position,
            SerializedProperty property
        ) : base(GetUxmlPath()) {
            _blueprintMeta = meta;
            
            this.nodeId = nodeId;
            viewDataKey = nodeId.ToString();

            _inspector = this.Q<VisualElement>("inspector");
            _property = property;
            
            RecreateNodeGUI();

            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            _expandButton = this.Q<Button>("expand");

            SetupExpandButtonTransform(_blueprintMeta.GetNodeExpandState(nodeId));
            
            _expandButton.clicked -= OnExpandButtonClicked;
            _expandButton.clicked += OnExpandButtonClicked;

            var source = meta.GetNodeSource(nodeId);
            var nodeType = source?.GetNodeType(nodeId.node);

            titleLabel.text = BlueprintNodeUtils.GetFormattedNodeName(nodeId, nodeType);
            container.style.backgroundColor = BlueprintNodeUtils.GetNodeColor(nodeType);

            style.left = position.x;
            style.top = position.y;

            CreatePortViews(meta, connectorListener);
        }

        public void DeInitialize() {
            _expandButton.clicked -= OnExpandButtonClicked;
        }
        
        private void OnExpandButtonClicked() {
            bool expanded = !_blueprintMeta.GetNodeExpandState(nodeId);
            
            _blueprintMeta.SetNodeExpandState(nodeId, expanded);
            SetupExpandButtonTransform(expanded);
            RecreateNodeGUI();

            OnValidate?.Invoke(nodeId);
        }

        private void SetupExpandButtonTransform(bool expanded) {
            _expandButton.transform.rotation = expanded ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
            _expandButton.transform.position = expanded ? new Vector3(0f, 1f, 0f) : new Vector3();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_blueprintMeta == null) return;
            
            if (_blueprintMeta.GroupStorage.TryGetGroupOfNode(nodeId, out int groupId)) {
                evt.menu.AppendAction("Ungroup", action => {
                    OnRemoveFromGroup?.Invoke(nodeId, groupId);
                });
            }
            
            base.BuildContextualMenu(evt);   
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeId);
        }

        private void RecreateNodeGUI() {
            _inspector.Clear();
            
            if (_property == null) return;
            if (!_blueprintMeta.GetNodeExpandState(nodeId)) return;
            
            float width = CalculateNodeViewMinWidth(_property);

            var property = _property.Copy();
            int depth = property.depth;
            var enumerator = property.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (enumerator.Current is not SerializedProperty p) continue;
                if (p.depth > depth + 1) continue;

                var propertyField = new PropertyField { style = { minWidth = width } };
                propertyField.BindProperty(p);

                // Track managed reference properties separately
                // to recreate node GUI with changed serialized properties max depth
                // if managed reference value was changed.
                if (p.propertyType == SerializedPropertyType.ManagedReference) {
                    propertyField.TrackPropertyValue(p, OnSerializedPropertyManagedReferenceValueChanged);
                }
                else {
                    propertyField.TrackPropertyValue(p, OnSerializedPropertyValueChanged);
                }

                _inspector.Add(propertyField);

                // Edit button for BlueprintAsset fields.
                if (p.propertyType == SerializedPropertyType.ObjectReference &&
                    p.objectReferenceValue is BlueprintAsset asset
                ) {
                    _inspector.Add(new Button(() => BlueprintEditorWindow.Open(asset)) { text = "Edit" });
                }

                // Need to track changes inside managed reference property manually,
                // because method propertyField.TrackPropertyValue(property, OnValueChanged) can only spot
                // the reference change and not the changes inside the class that is serialized by reference.
                if (p.propertyType == SerializedPropertyType.ManagedReference &&
                    p.managedReferenceValue != null
                ) {
                    int d = p.depth;
                    var e = p.Copy().GetEnumerator();

                    while (e.MoveNext()) {
                        if (e.Current is not SerializedProperty s || s.depth <= d) continue;

                        // Track managed reference properties separately
                        // to recreate node GUI with changed serialized properties max depth
                        // if managed reference value was changed.
                        if (s.propertyType == SerializedPropertyType.ManagedReference) {
                            propertyField.TrackPropertyValue(s, OnSerializedPropertyManagedReferenceValueChanged);
                        }
                        else {
                            propertyField.TrackPropertyValue(s, OnSerializedPropertyValueChanged);
                        }
                    }
                }
            }
        }

        private void OnSerializedPropertyValueChanged(SerializedProperty property) {
            if (property.propertyType == SerializedPropertyType.ObjectReference) {
                RecreateNodeGUI();
            }
            
            OnValidate?.Invoke(nodeId);
        }

        private void OnSerializedPropertyManagedReferenceValueChanged(SerializedProperty property) {
            RecreateNodeGUI();
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

            portView.portName = BlueprintNodeUtils.GetFormattedPortName(data.portIndex, data.port, richText: true);
            portView.portColor = BlueprintNodeUtils.GetPortColor(data.port);

            container.Add(portView);

            _portViewToPortIndexMap[portView] = data.portIndex;
            _portIndexToPortViewMap[data.portIndex] = portView;
        }

        private static float CalculateNodeViewMinWidth(SerializedProperty property) {
            float width = NODE_VIEW_MIN_WIDTH;
            int maxDepth = property.depth;
            var e = property.Copy().GetEnumerator();

            while (e.MoveNext()) {
                if (e.Current is not SerializedProperty p) continue;
                if (p.depth <= maxDepth) continue;

                if (p.propertyType == SerializedPropertyType.ArraySize) {
                    width += NODE_VIEW_WIDTH_INCREMENT_ARRAY;
                }

                if (p.propertyType == SerializedPropertyType.ManagedReference) {
                    width += NODE_VIEW_WIDTH_INCREMENT_MANAGED_REFERENCE;
                }

                width += NODE_VIEW_WIDTH_INCREMENT_DEPTH * (p.depth - maxDepth);
                maxDepth = p.depth;
            }

            return width;
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
