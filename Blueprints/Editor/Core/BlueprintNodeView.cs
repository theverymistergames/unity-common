using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Editor.VirtualInspector;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintNodeView : Node {

        public readonly BlueprintNodeMeta nodeMeta;

        public Action<BlueprintNodeMeta, Vector2> OnPositionChanged = delegate {  };
        public Action<BlueprintNodeMeta, BlueprintNode> OnValidate = delegate {  };

        private readonly VirtualInspector _nodeInspector;

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta, IEdgeConnectorListener connectorListener) : base(GetUxmlPath()) {
            this.nodeMeta = nodeMeta;

            viewDataKey = nodeMeta.NodeId.ToString();
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            var inspector = this.Q<InspectorView>("inspector");

            titleLabel.text = nodeMeta.NodeName;
            container.style.backgroundColor = nodeMeta.NodeColor;

            _nodeInspector = VirtualInspector.Create(nodeMeta.CreateNodeInstance(), OnNodeGUI, OnNodeValidate);

            inspector.UpdateSelection(_nodeInspector);

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;

            InitPorts(connectorListener);
        }

        public void DeInitialize() {
            Object.DestroyImmediate(_nodeInspector);
            Clear();
        }

        private void OnNodeValidate(object obj) {
            if (obj is BlueprintNode node) OnValidate.Invoke(nodeMeta, node);
        }

        private static void OnNodeGUI(SerializedProperty serializedProperty) {
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 240;

            bool enterChildren = true;
            while (serializedProperty.NextVisible(enterChildren)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(serializedProperty, true);

                if (serializedProperty.GetValue() is BlueprintAsset blueprintAsset && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.OpenAsset(blueprintAsset);
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
        
        private void InitPorts(IEdgeConnectorListener connectorListener) {
            var ports = nodeMeta.Ports;
            for (int i = 0; i < ports.Count; i++) {
                CreatePort(ports[i], connectorListener);
            }
            RefreshPorts();
        }

        private void CreatePort(Port port, IEdgeConnectorListener connectorListener) {
            if (port.isExternalPort) return;

            Direction direction;
            PortView.Capacity capacity;
            VisualElement container;

            switch (port.mode) {
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
                    throw new NotSupportedException($"Port mode {port.mode} is not supported");
            }

            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            portView.AddManipulator(new EdgeConnector<Edge>(connectorListener));

            portView.portName = FormatPortName(port);
            portView.portColor = GetPortColor(port);

            container.Add(portView);
        }

        private static Color GetPortColor(Port port) {
            return port.mode switch {
                Port.Mode.Enter => BlueprintColors.Port.Connection.Flow,
                Port.Mode.Exit => BlueprintColors.Port.Connection.Flow,
                Port.Mode.Input => BlueprintColors.Port.Connection.GetColorForType(port.DataType),
                Port.Mode.Output => BlueprintColors.Port.Connection.GetColorForType(port.DataType),
                Port.Mode.NonTypedInput => BlueprintColors.Port.Connection.Data,
                Port.Mode.NonTypedOutput => BlueprintColors.Port.Connection.Data,
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported"),
            };
        }

        private static string FormatPortName(Port port) {
            string nameColor = port.mode switch {
                Port.Mode.Enter => BlueprintColors.Port.Header.Flow,
                Port.Mode.Exit => BlueprintColors.Port.Header.Flow,
                Port.Mode.Input => BlueprintColors.Port.Header.GetColorForType(port.DataType),
                Port.Mode.Output => BlueprintColors.Port.Header.GetColorForType(port.DataType),
                Port.Mode.NonTypedInput => BlueprintColors.Port.Header.Data,
                Port.Mode.NonTypedOutput => BlueprintColors.Port.Header.Data,
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported"),
            };

            string name = string.IsNullOrEmpty(port.name) ? string.Empty : port.name.Trim();

            return $"<color={nameColor}>{name}</color>";
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
