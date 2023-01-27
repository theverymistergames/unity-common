using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Color;
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

        private readonly Dictionary<PortView, int> _portViewToPortIndexMap = new Dictionary<PortView, int>();
        private readonly Dictionary<int, PortView> _portIndexToPortViewMap = new Dictionary<int, PortView>();
        private readonly InspectorView _inspector;

        private struct PortViewCreationData {
            public int portIndex;
            public Port port;
        }

        public BlueprintNodeView(BlueprintNodeMeta nodeMeta) : base(GetUxmlPath()) {
            this.nodeMeta = nodeMeta;

            viewDataKey = nodeMeta.NodeId.ToString();
            
            var titleLabel = this.Q<Label>("title");
            var container = this.Q<VisualElement>("title-container");
            _inspector = this.Q<InspectorView>("inspector");

            var node = nodeMeta.Node;
            var nodeType = node.GetType();
            var nodeMetaAttr = nodeType.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);

            string nodeName = string.IsNullOrWhiteSpace(nodeMetaAttr.Name) ? nodeType.Name : nodeMetaAttr.Name.Trim();
            string nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;

            titleLabel.text = nodeName;
            container.style.backgroundColor = ColorUtils.HexToColor(nodeColor);

            style.left = nodeMeta.Position.x;
            style.top = nodeMeta.Position.y;
        }

        public void InitializeNodeInspector(Action onInspectorGUI) {
            _inspector.Inject(onInspectorGUI);
        }

        public void DeInitialize() {
            _portViewToPortIndexMap.Clear();
            _portIndexToPortViewMap.Clear();
            _inspector.ClearInspector();
            Clear();
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(nodeMeta, new Vector2(newPos.xMin, newPos.yMin));
        }

        public void CreatePortViews(IEdgeConnectorListener connectorListener) {
            var portViewsCreationData = nodeMeta.Ports
                .Select((port, index) => new PortViewCreationData { portIndex = index, port = port })
                .Where(data => !data.port.isExternalPort)
                .OrderBy(d => d.port.mode)
                .ThenBy(d => d.port.name)
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
            if (data.port.isExternalPort) return;

            Direction direction;
            PortView.Capacity capacity;
            VisualElement container;

            switch (data.port.mode) {
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
                    throw new NotSupportedException($"Port mode {data.port.mode} is not supported");
            }

            var portView = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            portView.AddManipulator(new EdgeConnector<Edge>(connectorListener));

            portView.portName = FormatPortName(data.port);
            portView.portColor = GetPortColor(data.port);

            container.Add(portView);

            _portViewToPortIndexMap[portView] = data.portIndex;
            _portIndexToPortViewMap[data.portIndex] = portView;
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
            string name = string.IsNullOrEmpty(port.name) ? string.Empty : port.name.Trim();

            return port.mode switch {
                Port.Mode.Enter => $"<color={BlueprintColors.Port.Header.Flow}>{name}</color>",
                Port.Mode.Exit => $"<color={BlueprintColors.Port.Header.Flow}>{name}</color>",
                Port.Mode.Input => $"<color={BlueprintColors.Port.Header.GetColorForType(port.DataType)}>{name}</color>",
                Port.Mode.Output => $"<color={BlueprintColors.Port.Header.GetColorForType(port.DataType)}>{name}</color>",
                Port.Mode.NonTypedInput => $"<color={BlueprintColors.Port.Header.Data}>{name}</color>",
                Port.Mode.NonTypedOutput => $"<color={BlueprintColors.Port.Header.Data}>{name}</color>",
                _ => throw new NotSupportedException($"Port mode {port.mode} is not supported"),
            };
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("BlueprintNodeView");
            return AssetDatabase.GetAssetPath(asset);
        }

        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
