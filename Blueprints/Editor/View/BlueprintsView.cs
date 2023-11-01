using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blackboards.Editor;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.View {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        private const float POPULATE_SCROLL_TO_NODES_CENTER_TOLERANCE_DISTANCE = 70f;

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action OnBlueprintAssetSetDirty = delegate {  };

        private BlueprintAsset2 _blueprintAsset;
        private SerializedObject _blueprintAssetSerializedObject;
        private readonly HashSet<NodeId> _changedNodes = new HashSet<NodeId>();

        private BlueprintNodeSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;

        private CancellationTokenSource _blackboardOpenSearchWindowCts;

        private BlackboardView _blackboardView;
        private Vector2 _mousePosition;

        private DropEdgeData _lastDropEdgeData;

        private struct DropEdgeData {
            public NodeId nodeId;
            public int portIndex;
        }

        [Serializable]
        public struct CopyPasteData {

            public List<NodeData> nodes;
            public List<LinkData> links;
            public Vector2 position;

            [Serializable]
            public struct NodeData {
                public NodeId nodeId;
                public Vector2 position;
                public SerializedType nodeType;
                public string nodeJson;
            }

            [Serializable]
            public struct LinkData {
                public NodeId fromNodeId;
                public int fromPortIndex;
                public NodeId toNodeId;
                public int toPortIndex;
            }
        }

        // ---------------- ---------------- Initialization ---------------- ----------------

        public BlueprintsView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer { minScale = 0.01f, maxScale = 10f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(Resources.Load<StyleSheet>("BlueprintsEditorViewStyle"));

            InitNodeSearchWindow();
            InitBlackboard();
            InitUndoRedo();
            InitCopyPaste();
            InitMouse();
        }

        // ---------------- ---------------- Population and views ---------------- ----------------

        public void PopulateViewFromAsset(BlueprintAsset2 blueprintAsset) {
            if (blueprintAsset == _blueprintAsset) return;

            ClearView();

            _blueprintAsset = blueprintAsset;
            _blueprintAssetSerializedObject = new SerializedObject(_blueprintAsset);

            InvalidateBlueprintAsset(_blueprintAsset);

            RepopulateView();
            RepopulateBlackboardView();

            GetBlueprintNodesCenter(out var position, out var scale);

            _blueprintAsset.BlueprintMeta.Bind(OnNodeChanged);

            var currentPosition = contentViewContainer.transform.position;
            if (Vector3.Distance(position, currentPosition) < POPULATE_SCROLL_TO_NODES_CENTER_TOLERANCE_DISTANCE) {
                return;
            }

            UpdateViewTransform(position, scale);
        }

        private void RepopulateView() {
            if (_blueprintAsset == null) return;

            graphViewChanged -= OnGraphViewChanged;
            foreach (var element in graphElements) {
                if (element is BlueprintNodeView nodeView) nodeView.DeInitialize();
            }
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            var meta = _blueprintAsset.BlueprintMeta;

            foreach (var nodeId in meta.Nodes) {
                CreateNodeView(nodeId, meta.GetNodePosition(nodeId));
            }

            foreach (var nodeId in meta.Nodes) {
                CreateFromNodeConnectionViews(nodeId);
            }

            ClearSelection();
        }

        public void ClearView() {
            graphViewChanged -= OnGraphViewChanged;
            foreach (var element in graphElements) {
                if (element is BlueprintNodeView nodeView) nodeView.DeInitialize();
            }
            DeleteElements(graphElements);

            _blackboardOpenSearchWindowCts?.Cancel();
            _blackboardOpenSearchWindowCts?.Dispose();

            _blackboardView?.Clear();

            if (_blueprintAsset != null) {
                _blueprintAsset.BlueprintMeta.Unbind();
                _blueprintAsset = null;
            }
        }

        private void OnNodeChanged(NodeId id) {
            _changedNodes.Add(id);
        }

        private void FlushChanges() {
            var meta = _blueprintAsset.BlueprintMeta;

            foreach (var nodeId in _changedNodes) {
                RemoveNodeView(nodeId);
                if (meta.ContainsNode(nodeId)) CreateNodeView(nodeId, meta.GetNodePosition(nodeId));
            }

            foreach (var nodeId in _changedNodes) {
                CreateFromNodeConnectionViews(nodeId);
            }

            _changedNodes.Clear();
        }

        private void GetBlueprintNodesCenter(out Vector3 position, out Vector3 scale) {
            position = Vector3.zero;
            scale = Vector3.one;

            if (_blueprintAsset == null) return;

            var positionAccumulator = Vector2.zero;

            var meta = _blueprintAsset.BlueprintMeta;
            var nodes = meta.Nodes;

            foreach (var nodeId in nodes) {
                positionAccumulator += meta.GetNodePosition(nodeId);
            }

            int nodeCount = meta.NodeCount;
            if (nodeCount > 0) positionAccumulator /= nodeCount;

            scale = contentViewContainer.transform.scale;
            position = Vector3.Scale(-positionAccumulator, scale);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_blueprintAsset == null) return;

            base.BuildContextualMenu(evt);
        }

        private void SetBlueprintAssetDirtyAndNotify() {
            if (_blueprintAsset == null) return;

            _blueprintAssetSerializedObject.ApplyModifiedProperties();
            _blueprintAssetSerializedObject.Update();

            EditorUtility.SetDirty(_blueprintAsset);
            OnBlueprintAssetSetDirty?.Invoke();
        }

        private static void InvalidateBlueprintAsset(BlueprintAsset2 blueprintAsset) {
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(blueprintAsset);

            var meta = blueprintAsset.BlueprintMeta;
            var nodes = meta.Nodes;

            foreach (var nodeId in nodes) {
                meta.InvalidateNode(nodeId, invalidateLinks: true);
            }
        }

        // ---------------- ---------------- Node Search Window ---------------- ----------------

        private static bool OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            return SearchWindow.Open(new SearchWindowContext(position, 280f), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();

            _nodeSearchWindow.onNodeCreationRequest = (nodeType, position) => {
                if (_blueprintAsset == null) return;
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out _)) return;

                FlushChanges();
            };

            _nodeSearchWindow.onNodeAndLinkCreationRequest = (nodeType, position, portIndex) => {
                if (_blueprintAsset == null) return;
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out var id)) return;

                CreateConnection(_lastDropEdgeData.nodeId, _lastDropEdgeData.portIndex, id, portIndex);
                FlushChanges();
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

            if (show) RepopulateBlackboardView();
            else _blackboardView.Clear();
        }

        private void InitBlackboard() {
            _blackboardSearchWindow = ScriptableObject.CreateInstance<BlackboardSearchWindow>();
            _blackboardSearchWindow.onSelectType = CreateBlackboardProperty;
            _blackboardSearchWindow.onPendingArrayElementTypeSelection = OnBlackboardSearchWindowSelectedArrayType;

            _blackboardView = new BlackboardView(this) {
                windowed = false,
                addItemRequested = _ => { OnAddBlackboardPropertyRequest(); },
                moveItemRequested = (_, i, element) => OnBlackboardPropertyPositionChanged((BlackboardField) element, i),
                editTextRequested = (_, element, newName) => OnBlackboardPropertyNameChanged((BlackboardField) element, newName)
            };

            _blackboardView.SetPosition(new Rect(0, 0, 300, 300));
            _blackboardView.visible = false;

            Add(_blackboardView);
        }

        private void OnBlackboardSearchWindowSelectedArrayType(SearchWindowContext ctx) {
            _blackboardOpenSearchWindowCts?.Cancel();
            _blackboardOpenSearchWindowCts?.Dispose();
            _blackboardOpenSearchWindowCts = new CancellationTokenSource();

            TryOpenBlackboardSearchWindow(ctx, _blackboardOpenSearchWindowCts.Token).Forget();
        }

        private async UniTaskVoid TryOpenBlackboardSearchWindow(SearchWindowContext ctx, CancellationToken token) {
            bool isCancelled = await UniTask.DelayFrame(1, cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled || OpenSearchWindow(_blackboardSearchWindow, ctx.screenMousePosition)) return;

            isCancelled = await UniTask.DelayFrame(10, cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled || OpenSearchWindow(_blackboardSearchWindow, ctx.screenMousePosition)) return;

            isCancelled = await UniTask.DelayFrame(100, cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled || OpenSearchWindow(_blackboardSearchWindow, ctx.screenMousePosition)) return;
        }

        private void RepopulateBlackboardView() {
            if (_blueprintAsset == null) return;

            _blackboardView.Clear();

            if (!_blackboardView.visible) return;

            var blackboardSerializedProperty = _blueprintAssetSerializedObject.FindProperty("_blackboard");
            var properties = BlackboardUtils.GetSerializedBlackboardProperties(blackboardSerializedProperty);

            for (int i = 0; i < properties.Count; i++) {
                _blackboardView.Add(BlackboardUtils.CreateBlackboardPropertyView(properties[i]));
            }
        }

        private void OnAddBlackboardPropertyRequest() {
            if (_blueprintAsset == null) return;

            OpenSearchWindow(_blackboardSearchWindow, GetCurrentScreenMousePosition());
        }

        private void CreateBlackboardProperty(Type type) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Blackboard Property");

            string typeName = TypeNameFormatter.GetTypeName(type);
            if (!_blueprintAsset.Blackboard.TryAddProperty($"New {typeName}", type)) return;

            SetBlueprintAssetDirtyAndNotify();
            RepopulateBlackboardView();
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
            RepopulateBlackboardView();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Name Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyName(Blackboard.StringToHash(field.text), newName)) return;

            field.text = newName;
            SetBlueprintAssetDirtyAndNotify();
            RepopulateBlackboardView();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private static Type GetSourceType(Type nodeType) {
            var sourceType = typeof(BlueprintSource<>).MakeGenericType(nodeType);
            var types = TypeCache.GetTypesDerivedFrom(sourceType);
            return types.Count == 0 ? null : types[0];
        }

        private bool TryCreateNode(Type nodeType, Vector2 position, out NodeId id) {
            var meta = _blueprintAsset.BlueprintMeta;
            var sourceType = GetSourceType(nodeType);

            if (sourceType == null) {
                id = default;
                return false;
            }

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Node");
            id = meta.AddNode(sourceType, position);
            SetBlueprintAssetDirtyAndNotify();

            return true;
        }

        private void RemoveNode(NodeId id) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Node");

            _blueprintAsset.BlueprintMeta.RemoveNode(id);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void CreateConnection(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Create Connection");

            _blueprintAsset.BlueprintMeta.TryCreateLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void RemoveConnection(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Connection");

            _blueprintAsset.BlueprintMeta.RemoveLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprintAsset == null) return change;

            bool repaintBlackboard = false;

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };

            if (hasEdgesToCreate) for (int i = 0; i < change.edgesToCreate.Count; i++) {
                var edge = change.edgesToCreate[i];

                if (edge.input.node is BlueprintNodeView toNodeView && edge.output.node is BlueprintNodeView fromNodeView) {
                    var fromNodeId = fromNodeView.nodeId;
                    var toNodeId = toNodeView.nodeId;

                    int fromPortIndex = fromNodeView.GetPortIndex(edge.output);
                    int toPortIndex = toNodeView.GetPortIndex(edge.input);

                    CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                }
            }

            if (hasElementsToRemove) for (int i = 0; i < change.elementsToRemove.Count; i++) {
                var element = change.elementsToRemove[i];
                switch (element) {
                    case BlueprintNodeView view:
                        RemoveNode(view.nodeId);
                        break;

                    case BlackboardField field:
                        repaintBlackboard = true;
                        RemoveBlackboardProperty(field.text);
                        break;

                    case Edge edge:
                        if (edge.input?.node is BlueprintNodeView toNodeView && edge.output?.node is BlueprintNodeView fromNodeView) {
                            var fromNodeId = fromNodeView.nodeId;
                            var toNodeId = toNodeView.nodeId;

                            int fromPortIndex = fromNodeView.GetPortIndex(edge.output);
                            int toPortIndex = toNodeView.GetPortIndex(edge.input);

                            RemoveConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                        }
                        break;
                }
            }

            // Edge views are to be created in the next RepopulateView() call
            if (hasEdgesToCreate) change.edgesToCreate.Clear();
            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) SetBlueprintAssetDirtyAndNotify();
            if (repaintBlackboard) RepopulateBlackboardView();

            FlushChanges();

            return change;
        }

        private void CreateNodeView(NodeId id, Vector2 position) {
            var nodeView = new BlueprintNodeView(_blueprintAsset.BlueprintMeta, id, position, _blueprintAssetSerializedObject) {
                OnPositionChanged = OnNodePositionChanged,
                OnValidate = OnNodeValidate,
            };

            nodeView.CreatePortViews(this);

            AddElement(nodeView);
        }

        private void OnNodePositionChanged(NodeId id, Vector2 position) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Node Position Changed");

            _blueprintAsset.BlueprintMeta.SetNodePosition(id, position);

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnNodeValidate(NodeId id) {
            _blueprintAssetSerializedObject.ApplyModifiedProperties();
            _blueprintAssetSerializedObject.Update();

            var meta = _blueprintAsset.BlueprintMeta;
            var source = meta.GetNodeSource(id);

            source?.OnValidate(meta, id);

            SetBlueprintAssetDirtyAndNotify();
        }

        private void CreateFromNodeConnectionViews(NodeId id) {
            var meta = _blueprintAsset.BlueprintMeta;

            var fromNodeView = FindNodeViewByNodeId(id);
            int fromPortsCount = meta.GetPortCount(id);

            for (int p = 0; p < fromPortsCount; p++) {
                var fromPort = meta.GetPort(id, p);
                if (fromPort.IsHidden()) continue;

                var fromPortView = fromNodeView.GetPortView(p);
                for (meta.TryGetLinksFrom(id, p, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                    var link = meta.GetLink(l);
                    var toPortView = FindNodeViewByNodeId(link.id).GetPortView(link.port);
                    var edge = fromPortView.ConnectTo(toPortView);

                    AddElement(edge);
                }
            }
        }

        private void RemoveNodeView(NodeId id) {
            var nodeView = FindNodeViewByNodeId(id);
            if (nodeView == null) return;

            graphViewChanged -= OnGraphViewChanged;
            RemoveElement(nodeView);
            graphViewChanged += OnGraphViewChanged;
        }

        private BlueprintNodeView FindNodeViewByNodeId(NodeId id) {
            return GetNodeByGuid(id.ToString()) as BlueprintNodeView;
        }

        public override List<PortView> GetCompatiblePorts(PortView startPortView, NodeAdapter nodeAdapter) {
            var meta = _blueprintAsset.BlueprintMeta;

            var startNodeView = (BlueprintNodeView) startPortView.node;
            var startNodeId = startNodeView.nodeId;
            var startPort = meta.GetPort(startNodeId, startNodeView.GetPortIndex(startPortView));

            return ports
                .Where(portView => {
                    var nodeView = (BlueprintNodeView) portView.node;
                    if (nodeView.nodeId == startNodeId) return false;

                    var port = meta.GetPort(nodeView.nodeId, nodeView.GetPortIndex(portView));
                    return PortValidator.ArePortsCompatible(startPort, port);
                })
                .ToList();
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            var meta = _blueprintAsset.BlueprintMeta;

            var portView = edge.input ?? edge.output;
            var nodeView = (BlueprintNodeView) portView.node;

            int portIndex = nodeView.GetPortIndex(portView);
            var port = meta.GetPort(nodeView.nodeId, portIndex);

            _lastDropEdgeData.nodeId = nodeView.nodeId;
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
            RepopulateBlackboardView();
        }
        
        // ---------------- ---------------- Copy paste ---------------- ----------------

        private void InitCopyPaste() {
            canPasteSerializedData = CanPaste;
            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements) {
            var meta = _blueprintAsset.BlueprintMeta;

            var copyData = new CopyPasteData {
                nodes = new List<CopyPasteData.NodeData>(),
                links = new List<CopyPasteData.LinkData>(),
                position = Vector2.zero,
            };

            var elementArray = elements.ToArray();
            for (int i = 0; i < elementArray.Length; i++) {
                var element = elementArray[i];

                if (element is BlueprintNodeView nodeView) {
                    var id = nodeView.nodeId;
                    var position = meta.GetNodePosition(id);
                    var source = meta.GetNodeSource(id);

                    copyData.nodes.Add(new CopyPasteData.NodeData {
                        nodeId = id,
                        position = position,
                        nodeType = new SerializedType(source?.NodeType),
                        nodeJson = source?.GetNodeAsString(id.node),
                    });

                    copyData.position += position;

                    continue;
                }

                if (element is Edge { input: { node: BlueprintNodeView toNodeView }, output: { node: BlueprintNodeView fromNodeView } } edge) {
                    copyData.links.Add(new CopyPasteData.LinkData {
                        fromNodeId = fromNodeView.nodeId,
                        fromPortIndex = fromNodeView.GetPortIndex(edge.output),
                        toNodeId = toNodeView.nodeId,
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

            var meta = _blueprintAsset.BlueprintMeta;
            var positionDiff = _mousePosition - pasteData.position;

            var nodeIdMap = new Dictionary<NodeId, NodeId>();
            var connections = new List<(BlueprintLink2, BlueprintLink2)>();

            for (int i = 0; i < pasteData.nodes.Count; i++) {
                var nodeData = pasteData.nodes[i];
                var nodeType = nodeData.nodeType.ToType();
                var position = nodeData.position + positionDiff;

                if (!TryCreateNode(nodeType, position, out var id)) continue;

                meta.GetNodeSource(id)?.SetNode(id.node, nodeData.nodeJson);
                nodeIdMap[nodeData.nodeId] = id;
            }

            if (pasteData.links != null) {
                for (int i = 0; i < pasteData.links.Count; i++) {
                    var link = pasteData.links[i];
                    if (!nodeIdMap.TryGetValue(link.fromNodeId, out var fromNodeId) ||
                        !nodeIdMap.TryGetValue(link.toNodeId, out var toNodeId)
                    ) {
                        continue;
                    }

                    connections.Add((
                        new BlueprintLink2 { id = fromNodeId, port = link.fromPortIndex },
                        new BlueprintLink2 { id = toNodeId, port = link.toPortIndex }
                    ));

                    CreateConnection(fromNodeId, link.fromPortIndex, toNodeId, link.toPortIndex);
                }
            }

            FlushChanges();

            foreach (var (_, nodeId) in nodeIdMap) {
                AddToSelection(FindNodeViewByNodeId(nodeId));
            }

            for (int i = 0; i < connections.Count; i++) {
                var (from, to) = connections[i];

                var input = FindNodeViewByNodeId(from.id).GetPortView(from.port);
                var output = FindNodeViewByNodeId(to.id).GetPortView(to.port);

                var edge = input.connections
                    .FirstOrDefault(e =>
                        e.input == input && e.output == output ||
                        e.input == output && e.output == input
                    );

                if (edge != null) AddToSelection(edge);
            }
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
