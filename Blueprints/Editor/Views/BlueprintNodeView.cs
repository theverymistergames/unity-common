using System;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Editor.Blueprints.Editor.Utils;
using MisterGames.Common.Editor.Coroutines;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Port = MisterGames.Blueprints.Core.Port;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Views {

    internal sealed class BlueprintNodeView : Node {

        private const float BorderUpdatePeriod = 0.03f;
        private const float BorderUpdateDuration = 1.5f;
        
        private static readonly Color BorderColorCalled = new Color(0.99f, 0.8f, 0.5f);
        private static readonly Color BorderColorHasFlow = new Color(0.6f, 0.43f, 0.0f);
        private static readonly Color BorderColorHasNoFlow = new Color(0f, 0f, 0f, 0f);
        
        internal Action<BlueprintNode, Vector2> OnPositionChanged = delegate {  };
        internal BlueprintNode Node { get; }

        private readonly VisualElement _border;
        private EditorCoroutineTask _borderUpdateTask;
        private float _borderUpdateProcess;
        
        public BlueprintNodeView(BlueprintNode node) : base(GetUxmlPath()) {
            Node = node;
            viewDataKey = node.Guid;
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var inspector = this.Q<InspectorView>("inspector");

            var meta = NodeMeta.From(node.GetType());
            
            titleLabel.text = meta.name;
            container.style.backgroundColor = meta.color;
            inspector.UpdateSelection(node);

            var iNode = node.AsIBlueprintNode();
            style.left = iNode.Position.x;
            style.top = iNode.Position.y;

            _border = this.Q<VisualElement>("node-border");
            
            InitPorts();
            SetBorderColor(GetStartColor());
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        internal void Dispose() {
            _borderUpdateTask?.Cancel();
        }
        
        internal void VisualizeFlow() {
            if (Node.FlowCount <= 0) return;
            
            _borderUpdateProcess = 0f;
            var startColor = GetStartColor();
            var finishColor = BorderColorCalled;
            
            _borderUpdateTask?.Cancel();
            var routine = EditorCoroutines.ScheduleWhile(
                BorderUpdatePeriod,
                 BorderUpdatePeriod,
                () => OnBorderUpdate(startColor, finishColor)
            );
            _borderUpdateTask = EditorCoroutines.StartCoroutine(this, routine);
        }

        private void SetBorderColor(Color color) {
            _border.style.backgroundColor = color;
        }

        private Color GetStartColor() {
            return Node.FlowCount <= 0 ? BorderColorHasNoFlow : BorderColorHasFlow;
        }
        
        private bool OnBorderUpdate(Color startColor, Color finishColor) {
            _borderUpdateProcess += BorderUpdatePeriod / BorderUpdateDuration;
            var current = Color.Lerp(startColor, finishColor, ProcessToX(_borderUpdateProcess));
            SetBorderColor(current);
            return _borderUpdateProcess < 1f;
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(Node, new Vector2(newPos.xMin, newPos.yMin));
        }
        
        private void InitPorts() {
            var ports = Node.AsIBlueprintNode().Ports;
            for (int i = 0; i < ports.Length; i++) {
                CreatePort(ports[i]);
            }
            RefreshPorts();
        }

        private void CreatePort(Port port) {
            if (port.IsExposed) return;
            
            var direction = port.IsExit ? Direction.Output : Direction.Input;
            var capacity = port.IsMultiple ? PortView.Capacity.Multi : PortView.Capacity.Single;
            
            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            
            portView.portName = port.Name;
            portView.portColor = port.Color;

            var container = port.IsExit ? outputContainer : inputContainer;
            container.Add(portView);
        }

        public override string ToString() {
            return $"BlueprintNodeView({Node})";
        }
        
        private static float ProcessToX(float x) {
            return  -4f * (x - 0.5f) * (x - 0.5f) + 1f;
        }
        
        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
        
    }

}