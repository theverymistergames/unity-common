﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blackboards.Editor;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        private const float POPULATE_SCROLL_TO_NODES_CENTER_TOLERANCE_DISTANCE = 70f;

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action OnBlueprintAssetSetDirty = delegate {  };

        private BlueprintAsset _blueprintAsset;
        private SerializedObject _blueprintAssetSerializedObject;

        private BlueprintNodeSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;

        private CancellationTokenSource _blackboardOpenSearchWindowCts;

        private BlackboardView _blackboardView;
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
                public SerializedType nodeType;
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

        public void PopulateViewFromAsset(BlueprintAsset blueprintAsset) {
            if (blueprintAsset == _blueprintAsset) return;

            ClearView();

            _blueprintAsset = blueprintAsset;
            _blueprintAssetSerializedObject = new SerializedObject(_blueprintAsset);

            InvalidateBlueprintAsset(_blueprintAsset);
            RepopulateView();

            GetBlueprintNodesCenter(out var position, out var scale);

            _blueprintAsset.BlueprintMeta.OnInvalidateNodePortsAndLinks = RepaintNodePortsAndLinks;

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

            var nodeEntriesProperty = _blueprintAssetSerializedObject.FindProperty("_blueprintMeta._nodesMap._entries");
            for (int i = 0; i < nodeEntriesProperty.arraySize; i++) {
                var entry = nodeEntriesProperty.GetArrayElementAtIndex(i);

                var nodeMetaProperty = entry.FindPropertyRelative("value");
                var nodeProperty = nodeMetaProperty.FindPropertyRelative("_node");

                CreateNodeView(nodeMetaProperty.GetValue() as BlueprintNodeMeta, nodeProperty);
            }

            foreach (var nodeMeta in _blueprintAsset.BlueprintMeta.NodesMap.Values) {
                CreateFromNodeConnectionViews(nodeMeta);
            }

            RepopulateBlackboardView();
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
                _blueprintAsset.BlueprintMeta.OnInvalidateNodePortsAndLinks = null;
                _blueprintAsset = null;
            }
        }

        private void RepaintNodePortsAndLinks(int nodeId) {
            var nodeView = FindNodeViewByNodeId(nodeId);

            RemoveNodeConnectionViews(nodeView);

            nodeView.ClearPortViews();
            nodeView.CreatePortViews(this);

            CreateFromNodeConnectionViews(nodeView.nodeMeta);
            CreateToNodeConnectionViews(nodeView.nodeMeta);
        }

        private void GetBlueprintNodesCenter(out Vector3 position, out Vector3 scale) {
            position = Vector3.zero;
            scale = Vector3.one;

            if (_blueprintAsset == null) return;

            var positionAccumulator = Vector2.zero;

            var nodeMetas = _blueprintAsset.BlueprintMeta.NodesMap.Values;
            foreach (var nodeMeta in nodeMetas) {
                positionAccumulator += nodeMeta.Position;
            }

            if (nodeMetas.Count > 0) positionAccumulator /= nodeMetas.Count;

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
                    blueprintMeta.RemoveNode(blueprintAsset, nodeId);
                    isNodesDataChanged = true;
                    continue;
                }

                long refId = ManagedReferenceUtility.GetManagedReferenceIdForObject(blueprintAsset, nodeMeta.Node);
                if (refId is ManagedReferenceUtility.RefIdNull or ManagedReferenceUtility.RefIdUnknown) {
                    blueprintMeta.RemoveNode(blueprintAsset, nodeId);
                    isNodesDataChanged = true;
                    continue;
                }

                nodeMeta.OnValidateNode(blueprintAsset);
                isNodesDataChanged |= blueprintMeta.InvalidateNodePorts(blueprintAsset, nodeId, invalidateLinks: true, notify: false);
            }

            if (isNodesDataChanged) SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- Node Search Window ---------------- ----------------

        private static bool OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            return SearchWindow.Open(new SearchWindowContext(position, 280f), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();

            _nodeSearchWindow.onNodeCreationRequest = (node, position) => {
                if (_blueprintAsset == null) return;

                CreateNode(node, ConvertScreenPositionToLocal(position));

                RepopulateView();
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

            InvalidateBlueprintAsset(_blueprintAsset);
            SetBlueprintAssetDirtyAndNotify();
            RepopulateView();
        }

        private void RemoveBlackboardProperty(string propertyName) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Blackboard Property");

            _blueprintAsset.Blackboard.RemoveProperty(Blackboard.StringToHash(propertyName));

            InvalidateBlueprintAsset(_blueprintAsset);
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

            RepopulateBlackboardView();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private BlueprintNodeMeta CreateNode(BlueprintNode node, Vector2 position) {
            var nodeMeta = new BlueprintNodeMeta(node) { Position = position };

            nodeMeta.OnValidateNode(_blueprintAsset);
            nodeMeta.RecreatePorts(_blueprintAsset);

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Node");

            _blueprintAsset.BlueprintMeta.AddNode(nodeMeta);

            SetBlueprintAssetDirtyAndNotify();

            return nodeMeta;
        }

        private void RemoveNode(BlueprintNodeMeta nodeMeta) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Node");

            _blueprintAsset.BlueprintMeta.RemoveNode(_blueprintAsset, nodeMeta.NodeId);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void CreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Create Connection");

            _blueprintAsset.BlueprintMeta.TryCreateConnection(_blueprintAsset, fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }
        
        private void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Connection");

            _blueprintAsset.BlueprintMeta.RemoveConnection(_blueprintAsset, fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            SetBlueprintAssetDirtyAndNotify();
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprintAsset == null) return change;

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };

            if (hasEdgesToCreate) for (int i = 0; i < change.edgesToCreate.Count; i++) {
                var edge = change.edgesToCreate[i];

                if (edge.input.node is BlueprintNodeView toNodeView && edge.output.node is BlueprintNodeView fromNodeView) {
                    int fromNodeId = fromNodeView.nodeMeta.NodeId;
                    int toNodeId = toNodeView.nodeMeta.NodeId;

                    int fromPortIndex = fromNodeView.GetPortIndex(edge.output);
                    int toPortIndex = toNodeView.GetPortIndex(edge.input);

                    CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                }
            }

            if (hasElementsToRemove) for (int i = 0; i < change.elementsToRemove.Count; i++) {
                var element = change.elementsToRemove[i];
                switch (element) {
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
            }

            // Edge views are to be created in the next RepopulateView() call
            if (hasEdgesToCreate) change.edgesToCreate.Clear();
            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) SetBlueprintAssetDirtyAndNotify();
            if (hasElementsToRemove || hasEdgesToCreate) RepopulateView();

            return change;
        }

        private void CreateNodeView(BlueprintNodeMeta nodeMeta, SerializedProperty nodeProperty) {
            var nodeView = new BlueprintNodeView(nodeMeta, nodeProperty) {
                OnPositionChanged = OnNodePositionChanged,
                OnValidate = OnNodeValidate,
            };

            nodeView.CreatePortViews(this);

            AddElement(nodeView);
        }

        private void OnNodePositionChanged(BlueprintNodeMeta nodeMeta, Vector2 position) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Node Position Changed");

            nodeMeta.Position = position;

            SetBlueprintAssetDirtyAndNotify();
        }

        private void OnNodeValidate(BlueprintNodeMeta nodeMeta) {
            _blueprintAssetSerializedObject.ApplyModifiedProperties();
            _blueprintAssetSerializedObject.Update();

            nodeMeta.OnValidateNode(_blueprintAsset);

            SetBlueprintAssetDirtyAndNotify();
        }

        private void CreateFromNodeConnectionViews(BlueprintNodeMeta nodeMeta) {
            var blueprintMeta = _blueprintAsset.BlueprintMeta;

            var fromNodeView = FindNodeViewByNodeId(nodeMeta.NodeId);
            var fromNodePorts = nodeMeta.Ports;

            for (int p = 0; p < fromNodePorts.Length; p++) {
                var fromPort = fromNodePorts[p];
                if (fromPort.IsHidden()) continue;

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
                if (toPort.IsHidden()) continue;

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
                    return PortValidator.ArePortsCompatible(startPort, port);
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
                        nodeType = new SerializedType(nodeMeta.Node.GetType()),
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

            var positionDiff = _mousePosition - pasteData.position;

            var nodeIdMap = new Dictionary<int, int>();
            var connections = new List<(BlueprintLink, BlueprintLink)>();

            for (int i = 0; i < pasteData.nodes.Count; i++) {
                var nodeData = pasteData.nodes[i];

                var nodeType = nodeData.nodeType.ToType();
                var node = JsonUtility.FromJson(nodeData.nodeJson, nodeType) as BlueprintNode;
                var position = nodeData.position + positionDiff;

                var nodeMeta = CreateNode(node, position);
                nodeIdMap[nodeData.nodeId] = nodeMeta.NodeId;
            }

            if (pasteData.links != null) {
                for (int i = 0; i < pasteData.links.Count; i++) {
                    var link = pasteData.links[i];
                    if (!nodeIdMap.TryGetValue(link.fromNodeId, out int fromNodeId) ||
                        !nodeIdMap.TryGetValue(link.toNodeId, out int toNodeId)
                    ) {
                        continue;
                    }

                    connections.Add((
                        new BlueprintLink { nodeId = fromNodeId, portIndex = link.fromPortIndex },
                        new BlueprintLink { nodeId = toNodeId, portIndex = link.toPortIndex }
                    ));
                    CreateConnection(fromNodeId, link.fromPortIndex, toNodeId, link.toPortIndex);
                }
            }

            RepopulateView();

            foreach ((int _, int nodeId) in nodeIdMap) {
                AddToSelection(FindNodeViewByNodeId(nodeId));
            }

            for (int i = 0; i < connections.Count; i++) {
                var (from, to) = connections[i];

                var input = FindNodeViewByNodeId(from.nodeId).GetPortView(from.portIndex);
                var output = FindNodeViewByNodeId(to.nodeId).GetPortView(to.portIndex);

                var edge = input.connections
                    .FirstOrDefault(e =>
                        e.input == input && e.output == output ||
                        e.input == output && e.output == input
                    );

                if (edge != null) AddToSelection(edge);
            }

            connections.Clear();
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