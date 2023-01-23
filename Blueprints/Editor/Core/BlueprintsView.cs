using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using MisterGames.Fsm.Editor.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action OnBlueprintAssetSetDirty = delegate {  };

        private BlueprintAsset _blueprintAsset;
        private BlueprintNodeSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;

        private Action _editBlackboardPropertyAction;
        private BlackboardView _blackboardView;
        private MiniMap _miniMap;
        private Vector2 _mousePosition;

        [Serializable]
        public struct CopyPasteData {

            public List<NodeData> nodes;
            public List<LinkData> links;
            public Vector2 position;

            [Serializable]
            public struct NodeData {
                public int nodeId;
                public Vector2 position;
                public string serializedNodeType;
            }

            [Serializable]
            public struct LinkData {
                public int fromNodeId;
                public int fromPortIndex;
                public int toNodeId;
                public int toPortIndex;
            }
        }

        // ---------------- ---------------- Initialization ---------------- ----------------

        public BlueprintsView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(Resources.Load<StyleSheet>("BlueprintsEditorViewStyle"));

            InitNodeSearchWindow();
            InitBlackboard();
            InitMiniMap();
            InitUndoRedo();
            InitCopyPaste();
            InitMouse();
        }

        // ---------------- ---------------- Node Search Window ---------------- ----------------

        private static void OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            SearchWindow.Open(new SearchWindowContext(position), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();

            _nodeSearchWindow.onNodeCreationRequest = data => {
                if (_blueprintAsset == null) return;

                var nodeMeta = CreateNode(data.node, ConvertScreenPositionToLocal(data.position));
                CreateNodeView(nodeMeta);
            };

            _nodeSearchWindow.onNodeAndLinkCreationRequest = data => {
                if (_blueprintAsset == null) return;

                var nodeMeta = CreateNode(data.node, ConvertScreenPositionToLocal(data.position));
                CreateConnection(data.fromNodeId, data.fromPortIndex, nodeMeta.NodeId, data.toPortIndex);

                RepopulateView();
            };

            nodeCreationRequest = ctx => {
                if (_blueprintAsset == null) return;

                _nodeSearchWindow.SwitchToNodeSearch();
                OpenSearchWindow(_nodeSearchWindow, ctx.screenMousePosition);
            };
        }

        // ---------------- ---------------- Blackboard ---------------- ----------------

        public void ToggleBlackboard(bool show) {
            _blackboardView.visible = show;

            if (show) FillBlackboardView();
            else _blackboardView.Clear();
        }

        private void InitBlackboard() {
            _blackboardSearchWindow = ScriptableObject.CreateInstance<BlackboardSearchWindow>();
            _blackboardSearchWindow.onSelectType = CreateBlackboardProperty;

            _blackboardView = new BlackboardView(this) {
                windowed = false,
                addItemRequested = _ => { OnAddBlackboardPropertyRequest(); },
                moveItemRequested = (_, i, element) => OnBlackboardPropertyPositionChanged((BlackboardField) element, i),
                editTextRequested = (_, element, newName) => OnBlackboardPropertyNameChanged((BlackboardField) element, newName)
            };

            _blackboardView.SetPosition(new Rect(0, 0, 300, 300));

            Add(_blackboardView);
            _blackboardView.visible = false;
        }

        private void FillBlackboardView() {
            if (_blueprintAsset == null) return;

            _blackboardView.Clear();

            var properties = new List<BlackboardProperty>(_blueprintAsset.Blackboard.PropertiesMap.Values)
                .OrderBy(p => p.index)
                .ToList();

            for (int i = 0; i < properties.Count; i++) {
                var property = properties[i];
                var view = BlackboardUtils.CreateBlackboardPropertyView(property, OnBlackboardPropertyValueChanged);
                _blackboardView.Add(view);
            }
        }

        private void OnAddBlackboardPropertyRequest() {
            if (_blueprintAsset == null) return;

            OpenSearchWindow(_blackboardSearchWindow, GetCurrentScreenMousePosition());
        }

        private void CreateBlackboardProperty(Type type) {
            if (_blueprintAsset == null) return;

            if (!Blackboard.ValidateType(type)) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Blackboard Property");

            string typeName = Blackboard.GetTypeName(type);
            if (!_blueprintAsset.Blackboard.TryAddProperty($"New {typeName}", type, default, out var property)) return;

            var view = BlackboardUtils.CreateBlackboardPropertyView(property, OnBlackboardPropertyValueChanged);
            _blackboardView.Add(view);
        }

        private void RemoveBlackboardProperty(string propertyName) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Blackboard Property");

            _blueprintAsset.Blackboard.RemoveProperty(propertyName);

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyPositionChanged(BlackboardField field, int newIndex) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Position Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyIndex(field.text, newIndex)) return;

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Name Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyName(field.text, newName)) return;

            field.text = newName;
            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyValueChanged(string property, object value) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Value Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyValue(property, value)) return;

            SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- Minimap ---------------- ----------------

        public void ToggleMiniMap(bool show) {
            _miniMap.visible = show;
        }

        private void InitMiniMap() {
            _miniMap = new MiniMap { windowed = false };
            _miniMap.SetPosition(new Rect(600, 0, 400, 250));
            Add(_miniMap);
            _miniMap.visible = false;
        }

        // ---------------- ---------------- Population and views ---------------- ----------------

        public void PopulateViewFromAsset(BlueprintAsset blueprintAsset) {
            if (blueprintAsset == _blueprintAsset) return;

            _blueprintAsset = blueprintAsset;
            if (_blueprintAsset.BlueprintMeta.Invalidate()) SetBlueprintAssetDirtyAndNotify();

            RepopulateView();

            _blueprintAsset.BlueprintMeta.OnInvalidate = () => {
                SetBlueprintAssetDirtyAndNotify();
                RepopulateView();
            };

            _blueprintAsset.BlueprintMeta.OnInvalidateNode = _ => {
                SetBlueprintAssetDirtyAndNotify();
                RepopulateView();
            };
        }

        public void RepopulateView() {
            if (_blueprintAsset == null) return;

            // ReSharper disable once DelegateSubtraction
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            var nodesMeta = _blueprintAsset.BlueprintMeta.NodesMap.Values;
            foreach (var nodeMeta in nodesMeta) {
                CreateNodeView(nodeMeta);
            }
            foreach (var nodeMeta in nodesMeta) {
                CreateNodeConnectionViews(nodeMeta);
            }

            FillBlackboardView();
            ClearSelection();
        }

        public void ClearView() {
            foreach (var element in graphElements) {
                if (element is BlueprintNodeView nodeView) nodeView.DeInitialize();
            }

            _blueprintAsset = null;
            DeleteElements(graphElements);
            ClearSelection();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_blueprintAsset == null) return;

            base.BuildContextualMenu(evt);
        }

        private void SetBlueprintAssetDirtyAndNotify() {
            if (_blueprintAsset == null) return;

            EditorUtility.SetDirty(_blueprintAsset);
            OnBlueprintAssetSetDirty?.Invoke();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private BlueprintNodeMeta CreateNode(BlueprintNode node, Vector2 position) {
            var nodeMeta = BlueprintNodeMeta.Create(node);
            nodeMeta.Position = position;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Node");

            _blueprintAsset.BlueprintMeta.AddNode(nodeMeta);

            SetBlueprintAssetDirtyAndNotify();

            return nodeMeta;
        }

        private void RemoveNode(BlueprintNodeMeta nodeMeta) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Node");

            _blueprintAsset.BlueprintMeta.RemoveNode(nodeMeta.NodeId);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void CreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Create Connection");

            _blueprintAsset.BlueprintMeta.TryCreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Connection");

            _blueprintAsset.BlueprintMeta.RemoveConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprintAsset == null) return change;

            change.edgesToCreate?.ForEach(edge => {
                if (edge.input.node is BlueprintNodeView toView && edge.output.node is BlueprintNodeView fromView) {
                    int fromNodeId = fromView.nodeMeta.NodeId;
                    int toNodeId = toView.nodeMeta.NodeId;

                    int fromPort = GetPortIndex(edge.output);
                    int toPort = GetPortIndex(edge.input);

                    CreateConnection(fromNodeId, fromPort, toNodeId, toPort);
                }
            });

            change.elementsToRemove?.ForEach(e => {
                switch (e) {
                    case BlueprintNodeView view:
                        RemoveNode(view.nodeMeta);
                        break;

                    case BlackboardField field:
                        RemoveBlackboardProperty(field.text);
                        break;

                    case Edge edge:
                        if (edge.input.node is BlueprintNodeView toView && edge.output.node is BlueprintNodeView fromView) {
                            int fromNodeId = fromView.nodeMeta.NodeId;
                            int toNodeId = toView.nodeMeta.NodeId;

                            int fromPort = GetPortIndex(edge.output);
                            int toPort = GetPortIndex(edge.input);

                            RemoveConnection(fromNodeId, fromPort, toNodeId, toPort);
                        }
                        break;
                }
            });

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };

            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) {
                SetBlueprintAssetDirtyAndNotify();
                RepopulateView();
            }

            return change;
        }

        private BlueprintNodeView CreateNodeView(BlueprintNodeMeta nodeMeta) {
            var nodeView = new BlueprintNodeView(nodeMeta, this) {
                OnPositionChanged = OnNodePositionChanged,
                OnValidate = OnValidateNode,
            };

            AddElement(nodeView);

            return nodeView;
        }

        private void OnValidateNode(BlueprintNodeMeta nodeMeta, BlueprintNode node) {
            if (node is IBlueprintValidatedNode validatedNode) {
                validatedNode.OnValidate(nodeMeta.NodeId, _blueprintAsset);
            }
            node.OnValidate();
            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnNodePositionChanged(BlueprintNodeMeta nodeMeta, Vector2 position) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Node Position Changed");
            nodeMeta.Position = position;
            SetBlueprintAssetDirtyAndNotify();
        }

        private void CreateNodeConnectionViews(BlueprintNodeMeta nodeMeta) {
            var blueprintMeta = _blueprintAsset.BlueprintMeta;

            var fromNodeView = FindNodeViewByNodeId(nodeMeta.NodeId);
            var fromNodePorts = nodeMeta.Ports;

            for (int p = 0; p < fromNodePorts.Count; p++) {
                var port = fromNodePorts[p];
                if (port.isExternalPort) continue;

                bool isPortInExitContainer = port.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
                var fromPortView = GetPortView(fromNodeView, p, isPortInExitContainer);
                var links = blueprintMeta.GetLinksFromNodePort(nodeMeta.NodeId, p);

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    var toNodeView = FindNodeViewByNodeId(link.nodeId);
                    var toPort = toNodeView.nodeMeta.Ports[link.portIndex];

                    bool isToPortInExitContainer = toPort.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
                    var toPortView = GetPortView(toNodeView, link.portIndex, isToPortInExitContainer);

                    AddElement(fromPortView.ConnectTo(toPortView));
                }
            }
        }

        private Edge CreateConnectionView(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            var fromNodeView = FindNodeViewByNodeId(fromNodeId);
            var fromPort = fromNodeView.nodeMeta.Ports[fromPortIndex];

            bool isFromPortInExitContainer = fromPort.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
            var fromPortView = GetPortView(fromNodeView, fromPortIndex, isFromPortInExitContainer);

            var toNodeView = FindNodeViewByNodeId(toNodeId);
            var toPort = toNodeView.nodeMeta.Ports[toPortIndex];

            bool isToPortInExitContainer = toPort.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
            var toPortView = GetPortView(toNodeView, toPortIndex, isToPortInExitContainer);

            var edge = fromPortView.ConnectTo(toPortView);
            AddElement(edge);

            return edge;
        }

        private BlueprintNodeView FindNodeViewByNodeId(int nodeId) {
            return GetNodeByGuid(nodeId.ToString()) as BlueprintNodeView;
        }

        public override List<PortView> GetCompatiblePorts(PortView startPortView, NodeAdapter nodeAdapter) {
            int startNodeId = GetNode(startPortView).nodeMeta.NodeId;
            var startPort = GetPort(startPortView);

            return ports
                .Where(portView => GetNode(portView).nodeMeta.NodeId != startNodeId &&
                                   BlueprintValidation.ArePortsCompatible(startPort, GetPort(portView)))
                .ToList();
        }

        private static BlueprintNodeView GetNode(PortView portView) {
            return (BlueprintNodeView) portView.node;
        }

        private static Port GetPort(PortView view) {
            return ((BlueprintNodeView) view.node).nodeMeta.Ports[GetPortIndex(view)];
        }

        private static PortView GetPortView(BlueprintNodeView view, int index, bool isExit) {
            var ports = view.nodeMeta.Ports;
            int portIndex = -1;
            
            for (int i = 0; i < ports.Count; i++) {
                var port = ports[i];
                bool isPortInExitContainer = port.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
                if (isPortInExitContainer != isExit) continue;
                
                portIndex++;
                
                if (i == index) break;
            }

            var container = isExit ? view.outputContainer : view.inputContainer;
            return container[portIndex] as PortView;
        }
        
        private static int GetPortIndex(PortView portView) {
            bool isExit = portView.direction == Direction.Output;
            
            var node = portView.node;
            var portContainer = isExit ? node.outputContainer : node.inputContainer;
            
            int portIndex = portContainer.IndexOf(portView);
            var ports = ((BlueprintNodeView) node).nodeMeta.Ports;

            int count = -1;
            for (int i = 0; i < ports.Count; i++) {
                var port = ports[i];
                bool isPortInExitContainer = port.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput;
                if (isPortInExitContainer != isExit) continue;

                if (++count == portIndex) {
                    portIndex = i;
                    break;
                }
            }

            return portIndex;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            var portView = edge.input ?? edge.output;
            var nodeMeta = GetNode(portView).nodeMeta;
            var port = GetPort(portView);
            int portIndex = GetPortIndex(portView);

            _nodeSearchWindow.SwitchToNodePortSearch(new BlueprintNodeSearchWindow.PortSearchData {
                fromNodeId = nodeMeta.NodeId,
                fromPort = port,
                fromPortIndex = portIndex,
            });

            OpenSearchWindow(_nodeSearchWindow, GetCurrentScreenMousePosition());
        }

        public void OnDrop(GraphView graphView, Edge edge) { }

        // ---------------- ---------------- Undo Redo ---------------- ----------------

        private void InitUndoRedo() {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            if (_blueprintAsset == null) return;

            SetBlueprintAssetDirtyAndNotify();
            RepopulateView();
        }
        
        // ---------------- ---------------- Copy paste ---------------- ----------------

        private void InitCopyPaste() {
            canPasteSerializedData = CanPaste;
            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
        }

        private static string OnSerializeGraphElements(IEnumerable<GraphElement> elements) {
            var copyData = new CopyPasteData {
                nodes = new List<CopyPasteData.NodeData>(),
                links = new List<CopyPasteData.LinkData>(),
                position = Vector2.zero,
            };

            var elementArray = elements.ToArray();
            for (int i = 0; i < elementArray.Length; i++) {
                var element = elementArray[i];

                if (element is BlueprintNodeView nodeView) {
                    var nodeMeta = nodeView.nodeMeta;

                    copyData.position += nodeMeta.Position;

                    copyData.nodes.Add(new CopyPasteData.NodeData {
                        nodeId = nodeMeta.NodeId,
                        position = nodeMeta.Position,
                        serializedNodeType = SerializedType.ToString(nodeMeta.Node.GetType())
                    });
                    continue;
                }

                if (element is Edge edge &&
                    edge.input.node is BlueprintNodeView toNodeView &&
                    edge.output.node is BlueprintNodeView fromNodeView) {

                    copyData.links.Add(new CopyPasteData.LinkData {
                        fromNodeId = fromNodeView.nodeMeta.NodeId,
                        fromPortIndex = GetPortIndex(edge.output),
                        toNodeId = toNodeView.nodeMeta.NodeId,
                        toPortIndex = GetPortIndex(edge.input),
                    });
                }
            }

            if (elementArray.Length > 0) copyData.position /= elementArray.Length;

            return JsonUtility.ToJson(copyData);
        }

        private void OnUnserializeAndPaste(string operationName, string data) {
            CopyPasteData pasteData;
            try {
                pasteData = JsonUtility.FromJson<CopyPasteData>(data);
            }
            catch (ArgumentException) {
                return;
            }

            if (pasteData.nodes == null || pasteData.nodes.Count == 0) return;

            ClearSelection();

            var positionDiff = _mousePosition - pasteData.position;
            var nodeIdMap = new Dictionary<int, int>();

            for (int i = 0; i < pasteData.nodes.Count; i++) {
                var nodeData = pasteData.nodes[i];

                var node = (BlueprintNode) Activator.CreateInstance(SerializedType.FromString(nodeData.serializedNodeType));
                var position = nodeData.position + positionDiff;

                var nodeMeta = CreateNode(node, position);
                var nodeView = CreateNodeView(nodeMeta);

                nodeIdMap[nodeData.nodeId] = nodeMeta.NodeId;

                AddToSelection(nodeView);
            }

            if (pasteData.links != null) {
                for (int i = 0; i < pasteData.links.Count; i++) {
                    var link = pasteData.links[i];
                    if (!nodeIdMap.TryGetValue(link.fromNodeId, out int fromNodeId) ||
                        !nodeIdMap.TryGetValue(link.toNodeId, out int toNodeId)
                    ) {
                        continue;
                    }

                    CreateConnection(fromNodeId, link.fromPortIndex, toNodeId, link.toPortIndex);

                    var edge = CreateConnectionView(fromNodeId, link.fromPortIndex, toNodeId, link.toPortIndex);
                    AddToSelection(edge);
                }
            }

            nodeIdMap.Clear();
        }

        private bool CanPaste(string data) {
            return _blueprintAsset != null;
        }

        // ---------------- ---------------- Mouse position ---------------- ----------------

        private void InitMouse() {
            RegisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        private void HandleMouseMove(MouseMoveEvent evt) {
            _mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
        }

        private static Vector2 GetCurrentScreenMousePosition() {
            return GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        }

        private Vector2 ConvertScreenPositionToLocal(Vector2 screenPosition) {
            var worldPosition = OnRequestWorldPosition.Invoke(screenPosition);
            return contentViewContainer.WorldToLocal(worldPosition);
        }

        // ---------------- ---------------- Nested classes ---------------- ----------------
        
        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
