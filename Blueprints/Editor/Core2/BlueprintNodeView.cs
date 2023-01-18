using System;
using MisterGames.Blueprints.Core2;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Port = MisterGames.Blueprints.Core2.Port;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core2 {

    public sealed class BlueprintNodeView : Node {

        public Action<BlueprintNodeMeta, Vector2> OnPositionChanged = delegate {  };
        public BlueprintNodeMeta NodeMeta { get; }

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta) : base(GetUxmlPath()) {
            NodeMeta = nodeMeta;
            viewDataKey = nodeMeta.NodeId.ToString();
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var inspector = this.Q<InspectorView>("inspector");

            var meta = Editor.NodeMeta.From(nodeMeta.GetType());
            
            titleLabel.text = meta.name;
            container.style.backgroundColor = meta.color;
            inspector.UpdateSelection(nodeMeta);

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;

            InitPorts();
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(NodeMeta, new Vector2(newPos.xMin, newPos.yMin));
        }
        
        private void InitPorts() {
            var ports = NodeMeta.Ports;
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

            return $"<color={nameColor}>{port.name.Trim()}</color>";
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
        
    }

}
