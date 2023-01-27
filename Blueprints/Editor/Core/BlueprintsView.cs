using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Editor.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using Object = UnityEngine.Object;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action OnBlueprintAssetSetDirty = delegate {  };

        private BlueprintAsset _blueprintAsset;
        private BlueprintAssetEditor _blueprintAssetEditor;

        private BlueprintNodeSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;

        private BlackboardView _blackboardView;
        private MiniMap _miniMap;
        private Vector2 _mousePosition;

        private DropEdgeData _lastDropEdgeData;

        private struct DropEdgeData {
            public int nodeId;
            public int portIndex;
        }

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
                public string nodeJson;
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
            SearchWindow.Open(new SearchWindowContext(position, 280f), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();

            _nodeSearchWindow.onNodeCreationRequest = (node, position) => {
                if (_blueprintAsset == null) return;

                var nodeMeta = CreateNode(node, ConvertScreenPositionToLocal(position));
                CreateNodeView(nodeMeta);
            };

            _nodeSearchWindow.onNodeAndLinkCreationRequest = (node, position, portIndex) => {
                if (_blueprintAsset == null) return;

                var nodeMeta = CreateNode(node, ConvertScreenPositionToLocal(position));
                CreateConnection(_lastDropEdgeData.nodeId, _lastDropEdgeData.portIndex, nodeMeta.NodeId, portIndex);

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

            var blackboard = _blueprintAsset.Blackboard;
            _blackboardView.Clear();

            var properties = new List<BlackboardProperty>(blackboard.PropertiesMap.Values)
                .OrderBy(p => p.index)
                .ToList();

            for (int i = 0; i < properties.Count; i++) {
                var property = properties[i];
                var view = BlackboardUtils.CreateBlackboardPropertyView(blackboard, property, OnBlackboardPropertyValueChanged);
                _blackboardView.Add(view);
            }
        }

        private void OnAddBlackboardPropertyRequest() {
            if (_blueprintAsset == null) return;

            OpenSearchWindow(_blackboardSearchWindow, GetCurrentScreenMousePosition());
        }

        private void CreateBlackboardProperty(Type type) {
            if (_blueprintAsset == null) return;

            var blackboard = _blueprintAsset.Blackboard;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Blackboard Property");

            string typeName = Blackboard.GetTypeName(type);
            if (!blackboard.TryAddProperty($"New {typeName}", type, out var property)) return;

            var view = BlackboardUtils.CreateBlackboardPropertyView(blackboard, property, OnBlackboardPropertyValueChanged);
            _blackboardView.Add(view);

            SetBlueprintAssetDirtyAndNotify();
        }

        private void RemoveBlackboardProperty(string propertyName) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Blackboard Property");

            _blueprintAsset.Blackboard.RemoveProperty(Blackboard.StringToHash(propertyName));

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyPositionChanged(BlackboardField field, int newIndex) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Position Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyIndex(Blackboard.StringToHash(field.text), newIndex)) return;

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Name Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyName(Blackboard.StringToHash(field.text), newName)) return;

            field.text = newName;
            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyValueChanged(string property, object value) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Value Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyValue(Blackboard.StringToHash(property), value)) return;

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

            ClearView();

            _blueprintAsset = blueprintAsset;
            InvalidateBlueprintAsset(_blueprintAsset);

            _blueprintAssetEditor = (BlueprintAssetEditor) UnityEditor.Editor.CreateEditor(_blueprintAsset, typeof(BlueprintAssetEditor));
            RepopulateView();

            _blueprintAsset.BlueprintMeta.OnInvalidateNodePortsAndLinks = RepaintNodePortsAndLinks;
        }

        private void RepaintNodePortsAndLinks(int nodeId) {
            var nodeView = FindNodeViewByNodeId(nodeId);

            RemoveNodeConnectionViews(nodeView);

            nodeView.ClearPortViews();
            nodeView.CreatePortViews(this);

            CreateFromNodeConnectionViews(nodeView.nodeMeta);
            CreateToNodeConnectionViews(nodeView.nodeMeta);
        }

        private void RepopulateView() {
            if (_blueprintAsset == null) return;

            graphViewChanged -= OnGraphViewChanged;
            foreach (var element in graphElements) {
                if (element is BlueprintNodeView nodeView) nodeView.DeInitialize();
            }
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            var nodesMeta = _blueprintAsset.BlueprintMeta.NodesMap.Values;
            foreach (var nodeMeta in nodesMeta) {
                CreateNodeView(nodeMeta);
            }
            foreach (var nodeMeta in nodesMeta) {
                CreateFromNodeConnectionViews(nodeMeta);
            }

            FillBlackboardView();
            ClearSelection();
        }

        public void ClearView() {
            graphViewChanged -= OnGraphViewChanged;

            foreach (var element in graphElements) {
                if (element is BlueprintNodeView nodeView) nodeView.DeInitialize();
            }
            DeleteElements(graphElements);

            if (_blueprintAsset != null) {
                _blueprintAsset.BlueprintMeta.OnInvalidateNodePortsAndLinks = null;
                _blueprintAsset = null;
            }

            if (_blueprintAssetEditor != null) {
                Object.DestroyImmediate(_blueprintAssetEditor);
                _blueprintAssetEditor = null;
            }
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

        private void InvalidateBlueprintAsset(BlueprintAsset blueprintAsset) {
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(blueprintAsset);

            var blueprintMeta = blueprintAsset.BlueprintMeta;
            var nodesMap = blueprintMeta.NodesMap;

            int[] nodeIds = new int[nodesMap.Count];
            nodesMap.Keys.CopyTo(nodeIds, 0);

            bool isNodesDataChanged = false;

            for (int n = 0; n < nodeIds.Length; n++) {
                int nodeId = nodeIds[n];
                var nodeMeta = nodesMap[nodeId];

                if (nodeMeta.Node == null) {
                    blueprintMeta.RemoveNode(nodeId);
                    isNodesDataChanged = true;
                    continue;
                }

                long refId = SerializationUtility.GetManagedReferenceIdForObject(blueprintAsset, nodeMeta.Node);
                if (refId is SerializationUtility.RefIdNull or SerializationUtility.RefIdUnknown) {
                    blueprintMeta.RemoveNode(nodeId);
                    isNodesDataChanged = true;
                    continue;
                }

                isNodesDataChanged |= blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true, notify: false);
            }

            if (isNodesDataChanged) SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private BlueprintNodeMeta CreateNode(BlueprintNode node, Vector2 position) {
            var nodeMeta = new BlueprintNodeMeta(node) { Position = position };
            nodeMeta.RecreatePorts();

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
                if (edge.input.node is BlueprintNodeView toNodeView && edge.output.node is BlueprintNodeView fromNodeView) {
                    int fromNodeId = fromNodeView.nodeMeta.NodeId;
                    int toNodeId = toNodeView.nodeMeta.NodeId;

                    int fromPortIndex = fromNodeView.GetPortIndex(edge.output);
                    int toPortIndex = toNodeView.GetPortIndex(edge.input);

                    CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
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
                        if (edge.input?.node is BlueprintNodeView toNodeView && edge.output?.node is BlueprintNodeView fromNodeView) {
                            int fromNodeId = fromNodeView.nodeMeta.NodeId;
                            int toNodeId = toNodeView.nodeMeta.NodeId;

                            int fromPortIndex = fromNodeView.GetPortIndex(edge.output);
                            int toPortIndex = toNodeView.GetPortIndex(edge.input);

                            RemoveConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                        }
                        break;
                }
            });

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };

            // Edge views are to be created in the next RepopulateView() call
            if (hasEdgesToCreate) change.edgesToCreate.Clear();

            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) SetBlueprintAssetDirtyAndNotify();
            if (hasElementsToRemove || hasEdgesToCreate) RepopulateView();

            return change;
        }

        private BlueprintNodeView CreateNodeView(BlueprintNodeMeta nodeMeta) {
            var nodeView = new BlueprintNodeView(nodeMeta) { OnPositionChanged = OnNodePositionChanged };
            nodeView.CreatePortViews(this);

            nodeView.InitializeNodeInspector(() => {
                _blueprintAsset.editedNodeId = nodeMeta.NodeId;
                _blueprintAsset.editedNode = nodeMeta.Node;
                _blueprintAssetEditor.DoOnInspectorGUI();
            });

            AddElement(nodeView);

            return nodeView;
        }

        private void OnNodePositionChanged(BlueprintNodeMeta nodeMeta, Vector2 position) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Node Position Changed");
            nodeMeta.Position = position;
            SetBlueprintAssetDirtyAndNotify();
        }

        private void CreateFromNodeConnectionViews(BlueprintNodeMeta nodeMeta) {
            var blueprintMeta = _blueprintAsset.BlueprintMeta;

            var fromNodeView = FindNodeViewByNodeId(nodeMeta.NodeId);
            var fromNodePorts = nodeMeta.Ports;

            for (int p = 0; p < fromNodePorts.Length; p++) {
                var fromPort = fromNodePorts[p];
                if (fromPort.isExternalPort) continue;

                var fromPortView = fromNodeView.GetPortView(p);
                var links = blueprintMeta.GetLinksFromNodePort(nodeMeta.NodeId, p);

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    var toPortView = FindNodeViewByNodeId(link.nodeId).GetPortView(link.portIndex);
                    var edge = fromPortView.ConnectTo(toPortView);

                    AddElement(edge);
                }
            }
        }

        private void CreateToNodeConnectionViews(BlueprintNodeMeta nodeMeta) {
            var blueprintMeta = _blueprintAsset.BlueprintMeta;

            var toNodeView = FindNodeViewByNodeId(nodeMeta.NodeId);
            var toNodePorts = nodeMeta.Ports;

            for (int p = 0; p < toNodePorts.Length; p++) {
                var toPort = toNodePorts[p];
                if (toPort.isExternalPort) continue;

                var toPortView = toNodeView.GetPortView(p);
                var links = blueprintMeta.GetLinksToNodePort(nodeMeta.NodeId, p);

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    var fromPortView = FindNodeViewByNodeId(link.nodeId).GetPortView(link.portIndex);
                    var edge = fromPortView.ConnectTo(toPortView);

                    AddElement(edge);
                }
            }
        }

        private Edge CreateConnectionView(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            var fromPortView = FindNodeViewByNodeId(fromNodeId).GetPortView(fromPortIndex);
            var toPortView = FindNodeViewByNodeId(toNodeId).GetPortView(toPortIndex);

            var edge = fromPortView.ConnectTo(toPortView);
            AddElement(edge);

            return edge;
        }

        private void RemoveNodeConnectionViews(BlueprintNodeView nodeView) {
            graphViewChanged -= OnGraphViewChanged;

            int inputPortViewsCount = nodeView.inputContainer.hierarchy.childCount;
            for (int i = 0; i < inputPortViewsCount; i++) {
                var portView = nodeView.inputContainer[i] as PortView;
                if (portView == null) continue;

                DeleteElements(portView.connections);
            }

            int outputPortViewsCount = nodeView.outputContainer.hierarchy.childCount;
            for (int i = 0; i < outputPortViewsCount; i++) {
                var portView = nodeView.outputContainer[i] as PortView;
                if (portView == null) continue;

                DeleteElements(portView.connections);
            }

            graphViewChanged += OnGraphViewChanged;
        }

        private BlueprintNodeView FindNodeViewByNodeId(int nodeId) {
            return GetNodeByGuid(nodeId.ToString()) as BlueprintNodeView;
        }

        public override List<PortView> GetCompatiblePorts(PortView startPortView, NodeAdapter nodeAdapter) {
            var startNodeView = (BlueprintNodeView) startPortView.node;
            var startNodeMeta = startNodeView.nodeMeta;
            var startPort = startNodeMeta.Ports[startNodeView.GetPortIndex(startPortView)];

            return ports
                .Where(portView => {
                    var nodeView = (BlueprintNodeView) portView.node;
                    if (nodeView.nodeMeta.NodeId == startNodeMeta.NodeId) return false;

                    var port = nodeView.nodeMeta.Ports[nodeView.GetPortIndex(portView)];
                    return BlueprintValidation.ArePortsCompatible(startPort, port);
                })
                .ToList();
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            var portView = edge.input ?? edge.output;
            var nodeView = (BlueprintNodeView) portView.node;
            var nodeMeta = nodeView.nodeMeta;

            int portIndex = nodeView.GetPortIndex(portView);
            var port = nodeMeta.Ports[portIndex];

            _lastDropEdgeData.nodeId = nodeMeta.NodeId;
            _lastDropEdgeData.portIndex = portIndex;

            _nodeSearchWindow.SwitchToNodePortSearch(port);
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
                        serializedNodeType = SerializedType.ToString(nodeMeta.Node.GetType()),
                        nodeJson = JsonUtility.ToJson(nodeMeta.Node),
                    });
                    continue;
                }

                if (element is Edge { input: { node: BlueprintNodeView toNodeView }, output: { node: BlueprintNodeView fromNodeView } } edge) {
                    copyData.links.Add(new CopyPasteData.LinkData {
                        fromNodeId = fromNodeView.nodeMeta.NodeId,
                        fromPortIndex = fromNodeView.GetPortIndex(edge.output),
                        toNodeId = toNodeView.nodeMeta.NodeId,
                        toPortIndex = toNodeView.GetPortIndex(edge.input),
                    });
                }
            }

            if (elementArray.Length > 0) copyData.position /= elementArray.Length;

            return JsonUtility.ToJson(copyData);
        }

        private void OnUnserializeAndPaste(string operationName, string data) {
            if (data == null) return;

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

                var nodeType = SerializedType.FromString(nodeData.serializedNodeType);
                var node = JsonUtility.FromJson(nodeData.nodeJson, nodeType) as BlueprintNode;
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
