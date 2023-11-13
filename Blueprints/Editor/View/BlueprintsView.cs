using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blackboards.Editor;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.View {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        private const float POPULATE_SCROLL_TO_NODES_CENTER_TOLERANCE_DISTANCE = 70f;

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action OnBlueprintAssetSetDirty = delegate {  };

        private BlueprintAsset2 _blueprintAsset;
        private SerializedObject _blueprintAssetSerializedObject;

        private bool _isWaitingEndOfFrameToValidateNodes;
        private bool _isWaitingEndOfFrameToRepaintInvalidNodes;

        private readonly HashSet<NodeId> _serializedPropertyChangedNodes = new HashSet<NodeId>();
        private readonly HashSet<NodeId> _invalidNodes = new HashSet<NodeId>();
        private readonly HashSet<NodeId> _positionChangedNodes = new HashSet<NodeId>();

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

            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseUp(MouseUpEvent evt) {
            if (evt.button == 0) WriteChangedPositions();
        }

        private void WriteChangedPositions() {
            if (_blueprintAsset == null) {
                _positionChangedNodes.Clear();
                return;
            }

            var meta = _blueprintAsset.BlueprintMeta;
            int count = _positionChangedNodes.Count;

            foreach (var nodeId in _positionChangedNodes) {
                if (FindNodeViewByNodeId(nodeId) is not { } nodeView) continue;

                var rect = nodeView.GetPosition();
                var position = new Vector2(rect.x, rect.y);

                meta.SetNodePosition(nodeId, position);
            }

            _positionChangedNodes.Clear();

            if (count > 0) SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- Population and views ---------------- ----------------

        public void PopulateViewFromAsset(BlueprintAsset2 blueprintAsset) {
            if (blueprintAsset == _blueprintAsset) return;

            ClearView();

            _blueprintAsset = blueprintAsset;
            _blueprintAssetSerializedObject = new SerializedObject(_blueprintAsset);

            InvalidateBlueprintAsset(_blueprintAsset);
            RepopulateView();

            GetBlueprintNodesCenter(out var position, out var scale);

            _blueprintAsset.BlueprintMeta.Bind(OnNodeInvalidated);

            var currentPosition = contentViewContainer.transform.position;
            if (Vector3.Distance(position, currentPosition) < POPULATE_SCROLL_TO_NODES_CENTER_TOLERANCE_DISTANCE) {
                return;
            }

            UpdateViewTransform(position, scale);
        }

        private void RepopulateView() {
            if (_blueprintAsset == null) return;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            var meta = _blueprintAsset.BlueprintMeta;

            foreach (var nodeId in meta.Nodes) {
                CreateNodeView(meta, nodeId);
            }

            foreach (var nodeId in meta.Nodes) {
                CreateNodeLinkViews(meta, nodeId);
            }

            ClearSelection();
            RepopulateBlackboardView();
        }

        private void RepaintInvalidNodes() {
            var meta = _blueprintAsset.BlueprintMeta;

            foreach (var nodeId in _invalidNodes) {
                RemoveNodeLinkViews(nodeId);
                RemoveNodeView(nodeId);
                if (meta.ContainsNode(nodeId)) CreateNodeView(meta, nodeId);
            }

            foreach (var nodeId in _invalidNodes) {
                CreateNodeLinkViews(meta, nodeId);
            }

            _invalidNodes.Clear();
        }

        public void ClearView() {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);

            _blackboardOpenSearchWindowCts?.Cancel();
            _blackboardOpenSearchWindowCts?.Dispose();

            _blackboardView?.Clear();

            _invalidNodes.Clear();

            if (_blueprintAsset != null) {
                _blueprintAsset.BlueprintMeta.Unbind();
                _blueprintAsset = null;
            }
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

        private void InvalidateBlueprintAsset(BlueprintAsset2 blueprintAsset) {
            var meta = blueprintAsset.BlueprintMeta;
            var nodes = meta.Nodes;
            bool changed = false;

            foreach (var nodeId in nodes) {
                changed |= meta.InvalidateNode(nodeId, invalidateLinks: true, notify: false);
            }

            if (changed) SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- Node Search Window ---------------- ----------------

        private static bool OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            return SearchWindow.Open(new SearchWindowContext(position, 400f), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();

            _nodeSearchWindow.onNodeCreationRequest = (nodeType, position) => {
                if (_blueprintAsset == null) return;
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out _)) return;

                RepaintInvalidNodes();
            };

            _nodeSearchWindow.onNodeAndLinkCreationRequest = (nodeType, position, portIndex) => {
                if (_blueprintAsset == null) return;
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out var id)) return;

                CreateConnection(_lastDropEdgeData.nodeId, _lastDropEdgeData.portIndex, id, portIndex);
                RepaintInvalidNodes();
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

            var blackboard = _blueprintAsset.Blackboard;
            var properties = blackboard.Properties;

            for (int i = 0; i < properties.Count; i++) {
                if (!blackboard.TryGetProperty(properties[i], out var property)) continue;
                _blackboardView.Add(BlackboardUtils.CreateBlackboardPropertyView(property));
            }
        }

        private void OnAddBlackboardPropertyRequest() {
            if (_blueprintAsset == null) return;

            OpenSearchWindow(_blackboardSearchWindow, GetCurrentScreenMousePosition());
        }

        private void CreateBlackboardProperty(Type type) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Blackboard Property");

            string typeName = TypeNameFormatter.GetShortTypeName(type);
            if (!_blueprintAsset.Blackboard.TryAddProperty($"New {typeName}", type)) return;

            SetBlueprintAssetDirtyAndNotify();
            RepopulateBlackboardView();
        }

        private void RemoveBlackboardProperty(string propertyName) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Blackboard Property");

            _blueprintAsset.Blackboard.RemoveProperty(Blackboards.Core.Blackboard.StringToHash(propertyName));

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnBlackboardPropertyPositionChanged(BlackboardField field, int newIndex) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Position Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyIndex(Blackboards.Core.Blackboard.StringToHash(field.text), newIndex)) return;

            SetBlueprintAssetDirtyAndNotify();
            RepopulateBlackboardView();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Blackboard Property Name Changed");

            if (!_blueprintAsset.Blackboard.TrySetPropertyName(Blackboards.Core.Blackboard.StringToHash(field.text), newName)) return;

            field.text = newName;
            SetBlueprintAssetDirtyAndNotify();
            RepopulateBlackboardView();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private bool TryCreateNode(Type nodeType, Vector2 position, out NodeId id) {
            var meta = _blueprintAsset.BlueprintMeta;
            var sourceType = BlueprintNodeUtils.GetSourceType(nodeType);

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
            Debug.Log($"BlueprintsView.RemoveNode: {id}");

            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Node");

            _blueprintAsset.BlueprintMeta.RemoveNode(id);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void CreateConnection(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Create Connection");

            if (!_blueprintAsset.BlueprintMeta.TryCreateLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return;

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void RemoveConnection(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Connection");

            if (!_blueprintAsset.BlueprintMeta.RemoveLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return;

            SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprintAsset == null) return change;

            bool repaintBlackboard = false;

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };
            bool hasNodesToRemove = false;

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
                        hasNodesToRemove = true;
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

            if (hasEdgesToCreate || hasElementsToRemove) {
                if (hasNodesToRemove) RepopulateView();
                else RepaintInvalidNodes();
            }

            return change;
        }

        private void CreateNodeView(BlueprintMeta2 meta, NodeId id) {
            var position = meta.GetNodePosition(id);

            var nodeView = new BlueprintNodeView(meta, id, position, _blueprintAssetSerializedObject) {
                OnPositionChanged = OnNodePositionChanged,
                OnValidate = OnNodeSerializedPropertyChanged,
            };

            nodeView.CreatePortViews(this);

            AddElement(nodeView);
        }

        private async void OnNodeSerializedPropertyChanged(NodeId id) {
            _serializedPropertyChangedNodes.Add(id);

            if (_isWaitingEndOfFrameToValidateNodes) return;
            _isWaitingEndOfFrameToValidateNodes = true;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _blueprintAssetSerializedObject.ApplyModifiedProperties();
            _blueprintAssetSerializedObject.Update();

            var meta = _blueprintAsset.BlueprintMeta;
            foreach (var nodeId in _serializedPropertyChangedNodes) {
                meta.GetNodeSource(nodeId)?.OnValidate(meta, nodeId);
            }

            SetBlueprintAssetDirtyAndNotify();

            _isWaitingEndOfFrameToValidateNodes = false;
        }

        private async void OnNodeInvalidated(NodeId id) {
            _invalidNodes.Add(id);

            if (_isWaitingEndOfFrameToRepaintInvalidNodes) return;
            _isWaitingEndOfFrameToRepaintInvalidNodes = true;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            RepaintInvalidNodes();

            _isWaitingEndOfFrameToRepaintInvalidNodes = false;
        }

        private void OnNodePositionChanged(NodeId id) {
            _positionChangedNodes.Add(id);
        }

        private void CreateNodeLinkViews(BlueprintMeta2 meta, NodeId id) {
            if (!meta.ContainsNode(id)) return;

            var fromNodeView = FindNodeViewByNodeId(id);
            int fromPortsCount = meta.GetPortCount(id);

            for (int p = 0; p < fromPortsCount; p++) {
                var fromPort = meta.GetPort(id, p);
                if (fromPort.IsHidden() || !fromNodeView.TryGetPortView(p, out var fromPortView)) continue;

                for (meta.TryGetLinksFrom(id, p, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                    var link = meta.GetLink(l);
                    var toNodeView = FindNodeViewByNodeId(link.id);
                    if (!toNodeView.TryGetPortView(link.port, out var toPortView)) continue;

                    bool hasConnectionView = false;
                    foreach (var e in fromPortView.connections) {
                        if (e.input == fromPortView && e.output == toPortView ||
                            e.input == toPortView && e.output == fromPortView
                        ) {
                            hasConnectionView = true;
                            break;
                        }
                    }

                    if (hasConnectionView) continue;

                    AddElement(fromPortView.ConnectTo(toPortView));
                }

                for (meta.TryGetLinksTo(id, p, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                    var link = meta.GetLink(l);
                    var toNodeView = FindNodeViewByNodeId(link.id);
                    if (!toNodeView.TryGetPortView(link.port, out var toPortView)) continue;

                    bool hasConnectionView = false;
                    foreach (var e in fromPortView.connections) {
                        if (e.input == fromPortView && e.output == toPortView ||
                            e.input == toPortView && e.output == fromPortView
                        ) {
                            hasConnectionView = true;
                            break;
                        }
                    }

                    if (hasConnectionView) continue;

                    AddElement(toPortView.ConnectTo(fromPortView));
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

        private void RemoveNodeLinkViews(NodeId id) {
            var nodeView = FindNodeViewByNodeId(id);
            if (nodeView == null) return;

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

            ClearSelection();

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

            RepaintInvalidNodes();

            foreach (var (_, nodeId) in nodeIdMap) {
                AddToSelection(FindNodeViewByNodeId(nodeId));
            }

            for (int i = 0; i < connections.Count; i++) {
                var (from, to) = connections[i];

                if (!FindNodeViewByNodeId(from.id).TryGetPortView(from.port, out var input) ||
                    !FindNodeViewByNodeId(to.id).TryGetPortView(to.port, out var output)
                ) {
                    continue;
                }

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
