using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Editor.VirtualInspector;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintNodeView : Node {

        public readonly BlueprintNodeMeta nodeMeta;

        public Action<BlueprintNodeMeta, Vector2> OnPositionChanged = delegate {  };
        public Action<BlueprintNodeMeta, BlueprintNode> OnValidate = delegate {  };

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta, IEdgeConnectorListener connectorListener) : base(GetUxmlPath()) {
            this.nodeMeta = nodeMeta;

            viewDataKey = nodeMeta.NodeId.ToString();
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var inspector = this.Q<InspectorView>("inspector");

            titleLabel.text = nodeMeta.NodeName;
            container.style.backgroundColor = nodeMeta.NodeColor;
            inspector.UpdateSelection(VirtualInspector.Create(nodeMeta.Node, OnNodeGUI, OnNodeValidate));

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;

            InitPorts(connectorListener);
        }

        private void OnNodeValidate(object obj) {
            if (obj is BlueprintNode node) OnValidate.Invoke(nodeMeta, node);
        }

        private static void OnNodeGUI(SerializedProperty serializedProperty) {
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;

            bool enterChildren = true;
            while (serializedProperty.NextVisible(enterChildren)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(serializedProperty, true);
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeMeta, new Vector2(newPos.xMin, newPos.yMin));
        }
        
        private void InitPorts(IEdgeConnectorListener connectorListener) {
            var ports = nodeMeta.Ports;
            for (int i = 0; i < ports.Count; i++) {
                CreatePort(ports[i], connectorListener);
            }
            RefreshPorts();
        }

        private void CreatePort(Port port, IEdgeConnectorListener connectorListener) {
            if (port.isExternalPort) return;
            
            var direction = port.isExitPort ? Direction.Output : Direction.Input;
            var capacity = !port.isExitPort && port.isDataPort ? PortView.Capacity.Single : PortView.Capacity.Multi;
            
            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            portView.AddManipulator(new EdgeConnector<Edge>(connectorListener));

            portView.portName = FormatPortName(port);
            portView.portColor = GetPortColor(port);

            var container = port.isExitPort ? outputContainer : inputContainer;
            container.Add(portView);
        }

        private static Color GetPortColor(Port port) {
            return port.isDataPort
                ? port.hasDataType
                    ? BlueprintColors.Port.Connection.GetColorForType(port.DataType)
                    : BlueprintColors.Port.Connection.Data
                : BlueprintColors.Port.Connection.Flow;
        }

        private static string FormatPortName(Port port) {
            string nameColor = port.isDataPort
                ? port.hasDataType
                    ? BlueprintColors.Port.Header.GetColorForType(port.DataType)
                    : BlueprintColors.Port.Header.Data
                : BlueprintColors.Port.Header.Flow;

            string name = string.IsNullOrEmpty(port.name) ? string.Empty : port.name.Trim();

            return $"<color={nameColor}>{name}</color>";
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
