using System;
using MisterGames.Blueprints.Core2;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Editor.VirtualInspector;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Port = MisterGames.Blueprints.Core2.Port;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core2 {

    public sealed class BlueprintNodeView : Node {

        public readonly BlueprintNodeMeta nodeMeta;
        private readonly BlueprintAsset _ownerAsset;

        public Action<BlueprintNodeMeta, Vector2> OnPositionChanged = delegate {  };
        public Action<BlueprintNodeMeta> OnValidate = delegate {  };

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta, BlueprintAsset ownerAsset) : base(GetUxmlPath()) {
            this.nodeMeta = nodeMeta;
            _ownerAsset = ownerAsset;

            viewDataKey = nodeMeta.NodeId.ToString();
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var inspector = this.Q<InspectorView>("inspector");

            titleLabel.text = nodeMeta.NodeName;
            container.style.backgroundColor = nodeMeta.NodeColor;
            inspector.UpdateSelection(VirtualInspector.Create(nodeMeta.Node, OnNodeGUI, OnNodeValidate));

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;

            InitPorts();
        }

        private void OnNodeValidate(object obj) {
            if (obj is not BlueprintNode node) return;

            if (node is BlueprintNodeSubgraph subgraph) subgraph.OnValidate(nodeMeta.NodeId, _ownerAsset);
            node.OnValidate();

            OnValidate.Invoke(nodeMeta);
        }

        private static void OnNodeGUI(SerializedProperty serializedProperty) {
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;

            foreach (object child in serializedProperty) {
                var childProperty = (SerializedProperty) child;
                EditorGUILayout.PropertyField(childProperty, true);

                if (childProperty.GetValue() is BlueprintAsset blueprintAsset && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.GetWindow().PopulateFromAsset(blueprintAsset);
                }
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
        
        private void InitPorts() {
            var ports = nodeMeta.Ports;
            for (int i = 0; i < ports.Count; i++) {
                CreatePort(ports[i]);
            }
            RefreshPorts();
        }

        private void CreatePort(Port port) {
            if (port.isExternalPort) return;
            
            var direction = port.isExitPort ? Direction.Output : Direction.Input;
            var capacity = !port.isExitPort && port.isDataPort ? PortView.Capacity.Single : PortView.Capacity.Multi;
            
            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            
            portView.portName = FormatPortName(port);
            portView.portColor = GetPortColor(port);

            var container = port.isExitPort ? outputContainer : inputContainer;
            container.Add(portView);
        }

        private static Color GetPortColor(Port port) {
            return port.isDataPort
                ? port.hasDataType
                    ? MisterGames.Blueprints.Core2.BlueprintColors.Port.Connection.GetColorForType(port.DataType)
                    : MisterGames.Blueprints.Core2.BlueprintColors.Port.Connection.Data
                : MisterGames.Blueprints.Core2.BlueprintColors.Port.Connection.Flow;
        }

        private static string FormatPortName(Port port) {
            string nameColor = port.isDataPort
                ? port.hasDataType
                    ? MisterGames.Blueprints.Core2.BlueprintColors.Port.Header.GetColorForType(port.DataType)
                    : MisterGames.Blueprints.Core2.BlueprintColors.Port.Header.Data
                : MisterGames.Blueprints.Core2.BlueprintColors.Port.Header.Flow;

            string name = string.IsNullOrEmpty(port.name) ? string.Empty : port.name.Trim();

            return $"<color={nameColor}>{name}</color>";
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
