using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blackboards.Editor;
using MisterGames.Blueprints.Editor.Storage;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Maths;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BlackboardView = UnityEditor.Experimental.GraphView.Blackboard;
using Object = UnityEngine.Object;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.View {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;
        public Action<Object> OnSetDirty = delegate {  };

        private BlueprintMeta _blueprintMeta;
        private Blackboards.Core.Blackboard _blackboard;
        private SerializedObject _serializedObject;

        private IBlueprintFactory _factoryOverride;
        private BlueprintFactory _virtualFactory;
        private SerializedObject _virtualSerializedObject;

        private bool _isWaitingEndOfFrameToValidateNodes;
        private bool _isWaitingEndOfFrameToRepaintInvalidNodes;

        private readonly HashSet<NodeId> _serializedPropertyChangedNodes = new HashSet<NodeId>();
        private readonly HashSet<NodeId> _invalidNodes = new HashSet<NodeId>();
        private readonly HashSet<NodeId> _positionChangedNodes = new HashSet<NodeId>();
        private readonly HashSet<int> _positionChangedGroups = new HashSet<int>();
        private readonly HashSet<int> _invalidGroups = new HashSet<int>();

        private BlueprintNodeSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;

        private CancellationTokenSource _blackboardOpenSearchWindowCts;

        private BlackboardView _blackboardView;
        private Vector2 _mousePosition;

        private bool _areGraphOperationsAllowed;
        private bool _areBlackboardOperationsAllowed;

        private DropEdgeData _lastDropEdgeData;
        private BlueprintMetaInvalidateTracker _invalidateTracker;

        private struct DropEdgeData {
            public NodeId nodeId;
            public int portIndex;
        }

        [Serializable]
        public struct CopyPasteData {

            public List<NodeData> nodes;
            public List<BlueprintGroup> groups;
            public List<LinkData> links;
            public Vector2 position;

            [Serializable]
            public struct NodeData {
                public NodeId nodeId;
                public Vector2 position;
                public SerializedType nodeType;
                public string nodeJson;
                public bool expanded;
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

            this.AddManipulator(new ContentZoomer {minScale = 0.01f, maxScale = 10f});
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

        public void PopulateView(
            BlueprintMeta blueprintMeta,
            IBlueprintFactory factoryOverride,
            Blackboards.Core.Blackboard blackboard,
            SerializedObject serializedObject
        ) {
            if (_blueprintMeta == blueprintMeta &&
                _factoryOverride == factoryOverride &&
                _blackboard == blackboard &&
                _serializedObject?.targetObject == serializedObject?.targetObject
            ) {
                return;
            }

            ClearView();

            _blueprintMeta = blueprintMeta;
            _blackboard = blackboard;
            _serializedObject = serializedObject;
            _factoryOverride = factoryOverride;

            if (_factoryOverride == null) {
                // View for asset
                _virtualFactory = null;
                _virtualSerializedObject = null;
                _areGraphOperationsAllowed = _blueprintMeta != null;
                _areBlackboardOperationsAllowed = _blueprintMeta != null;
            }
            else if (_blueprintMeta != null) {
                // View for local override of an asset at BlueprintRunner
                _virtualFactory = new BlueprintFactory();

                // Copy nodes from meta, then from override
                _blueprintMeta.Factory.AdditiveCopyInto(_virtualFactory);
                _factoryOverride?.AdditiveCopyInto(_virtualFactory);

                var container = ScriptableObject.CreateInstance<VirtualBlueprintContainer>();
                container.Blackboard = _blackboard;
                container.Factory = _virtualFactory;

                _virtualSerializedObject = new SerializedObject(container);

                _areGraphOperationsAllowed = false;
                _areBlackboardOperationsAllowed = false;
            }

            InvalidateBlueprint();
            RepopulateView();

            _blueprintMeta?.Bind(OnNodeInvalidated);

            SetupPositionAndScale();
            InitializeGroupCallbacks();
        }

        private void InitializeGroupCallbacks() {
            groupTitleChanged = (group, title) => {
                if (_blueprintMeta == null || group is not BlueprintGroupView groupView) return;

                var g = _blueprintMeta.GroupStorage.GetGroup(groupView.id);
                g.name = title;
                
                Undo.RecordObject(_serializedObject.targetObject, "Blueprint Set Group Title");
                _blueprintMeta.GroupStorage.SetGroup(groupView.id, g);
                SetTargetObjectDirtyAndNotify();
            };
            
            elementsAddedToGroup = (group, elements) =>
            {
                if (_blueprintMeta == null || group is not BlueprintGroupView groupView) return;

                Undo.RecordObject(_serializedObject.targetObject, "Blueprint Add Node Into Group");
                var groupStorage = _blueprintMeta.GroupStorage;
                
                foreach (var element in elements)
                {
                    if (element is not BlueprintNodeView nodeView)
                    {
                        continue;
                    }

                    groupStorage.RemoveNodeFromGroups(nodeView.nodeId);
                    groupStorage.AddNodeIntoGroup(nodeView.nodeId, groupView.id);
                }
                
                SetTargetObjectDirtyAndNotify();
            };

            elementsRemovedFromGroup = (group, elements) => {
                if (_blueprintMeta == null || group is not BlueprintGroupView) return;

                Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Node From Group");
                var groupStorage = _blueprintMeta.GroupStorage;
                
                foreach (var element in elements)
                {
                    if (element is not BlueprintNodeView nodeView)
                    {
                        continue;
                    }

                    groupStorage.RemoveNodeFromGroups(nodeView.nodeId);
                }
                
                SetTargetObjectDirtyAndNotify();
            };
        }

        private void ClearGroupCallbacks() {
            groupTitleChanged = null;
            elementsAddedToGroup = null;
            elementsRemovedFromGroup = null;
        }

        private void RepopulateView() {
            if (_blueprintMeta == null) return;

            ClearGroupCallbacks();
            
            graphViewChanged -= OnGraphViewChanged;
            foreach (var graphElement in graphElements) {
                if (graphElement is BlueprintNodeView view) view.DeInitialize();
            }
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            foreach (var nodeId in _blueprintMeta.Nodes) {
                CreateNodeView(_blueprintMeta, nodeId);
            }

            foreach (var nodeId in _blueprintMeta.Nodes) {
                CreateNodeLinkViews(_blueprintMeta, nodeId);
            }
            
            foreach (var group in _blueprintMeta.GroupStorage.Groups) {
                CreateGroupView(_blueprintMeta, group.id);
            }

            ClearSelection();
            RepopulateBlackboardView();
            InitializeGroupCallbacks();
        }

        private void RepaintInvalidNodes() {
            if (_blueprintMeta == null) return;

            foreach (var nodeId in _invalidNodes) {
                RemoveNodeLinkViews(nodeId);
                RemoveNodeView(nodeId);
                if (_blueprintMeta.ContainsNode(nodeId)) CreateNodeView(_blueprintMeta, nodeId);
            }

            foreach (var nodeId in _invalidNodes) {
                CreateNodeLinkViews(_blueprintMeta, nodeId);
            }

            _invalidNodes.Clear();

            ClearGroupCallbacks();
            var groupStorage = _blueprintMeta.GroupStorage;
            
            foreach (int groupId in _invalidGroups) {
                RemoveGroupView(groupId);
                if (groupStorage.TryGetGroup(groupId, out _)) CreateGroupView(_blueprintMeta, groupId);
            }
            
            _invalidGroups.Clear();
            InitializeGroupCallbacks();
        }

        public void ClearView() {
            ClearGroupCallbacks();
            
            graphViewChanged -= OnGraphViewChanged;
            foreach (var graphElement in graphElements) {
                if (graphElement is BlueprintNodeView view) view.DeInitialize();
            }
            DeleteElements(graphElements);

            _blackboardOpenSearchWindowCts?.Cancel();
            _blackboardOpenSearchWindowCts?.Dispose();

            _blackboardView?.Clear();

            _invalidNodes.Clear();

            _blueprintMeta?.Unbind();

            _blueprintMeta = null;
            _factoryOverride = null;
            _blackboard = null;
            _serializedObject = null;
            _virtualFactory = null;
            _virtualSerializedObject = null;

            _areGraphOperationsAllowed = false;
            _areBlackboardOperationsAllowed = false;

            _isWaitingEndOfFrameToValidateNodes = false;
            _isWaitingEndOfFrameToRepaintInvalidNodes = false;
        }

        private void WriteChangedPositions() {
            if (_blueprintMeta == null || !_areGraphOperationsAllowed) {
                _positionChangedNodes.Clear();
                _positionChangedGroups.Clear();
                return;
            }

            int count = _positionChangedNodes.Count + _positionChangedGroups.Count;

            foreach (var nodeId in _positionChangedNodes) {
                if (FindNodeViewByNodeId(nodeId) is not { } nodeView) continue;

                var rect = nodeView.GetPosition();
                var position = new Vector2(rect.x, rect.y);

                _blueprintMeta.SetNodePosition(nodeId, position);
            }

            _positionChangedNodes.Clear();

            var groupStorage = _blueprintMeta.GroupStorage;
            
            foreach (int groupId in _positionChangedGroups) {
                if (FindGroupViewById(groupId) is not { } groupView) continue;

                var rect = groupView.GetPosition();
                var position = new Vector2(rect.x, rect.y);

                var group = groupStorage.GetGroup(groupId);
                group.position = position;
                
                groupStorage.SetGroup(groupId, group);
            }
            
            _positionChangedGroups.Clear();

            if (count > 0) SetTargetObjectDirtyAndNotify();
        }
        
        private void WritePositionAndZoom() {
            if (_blueprintMeta == null || !_areGraphOperationsAllowed) return;
            
            var t = contentViewContainer.transform;
            
            BlueprintEditorStorage.Instance.SetPosition(
                _serializedObject.targetObject, 
                new Vector3(t.position.x, t.position.y, t.scale.x)
            );
        }

        private void SetupPositionAndScale() {
            if (_blueprintMeta == null) return;

            var positionAndZoom = BlueprintEditorStorage.Instance.GetPosition(_serializedObject.targetObject);
            var position = positionAndZoom.WithZ(0f);
            var scale = Vector3.one * Mathf.Clamp(positionAndZoom.z, 0.01f, 10f);
            
            UpdateViewTransform(position, scale);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_blueprintMeta == null) return;
            
            evt.menu.AppendAction("Create Group", action => {
                var group = new BlueprintGroup {
                    name = "New Group",
                    position = action.eventInfo.localMousePosition,
                };
                
                if (!TryCreateGroup(group, out _)) return;

                RepaintInvalidNodes();
            });
            
            base.BuildContextualMenu(evt);
        }

        private void SetTargetObjectDirtyAndNotify(bool notify = true) {
            if (_serializedObject == null) return;

            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();

            EditorUtility.SetDirty(_serializedObject.targetObject);
            if (notify) OnSetDirty?.Invoke(_serializedObject.targetObject);
        }

        private void InvalidateBlueprint() {
            if (_factoryOverride != null) return;

            var nodes = _blueprintMeta.Nodes;
            bool changed = false;

            foreach (var nodeId in nodes) {
                changed |= _blueprintMeta.InvalidateNode(nodeId, invalidateLinks: true, notify: false);
            }

            if (changed) SetTargetObjectDirtyAndNotify();
        }

        // ---------------- ---------------- Node Search Window ---------------- ----------------

        private static bool OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            return SearchWindow.Open(new SearchWindowContext(position, 400f), window);
        }

        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintNodeSearchWindow>();
            
            _nodeSearchWindow.onNodeCreationRequest = (nodeType, position) => {
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out _)) return;

                RepaintInvalidNodes();
            };

            _nodeSearchWindow.onNodeAndLinkCreationRequest = (nodeType, position, portIndex) => {
                if (!TryCreateNode(nodeType, ConvertScreenPositionToLocal(position), out var id)) return;
                if (!TryCreateLink(_lastDropEdgeData.nodeId, _lastDropEdgeData.portIndex, id, portIndex)) return;

                RepaintInvalidNodes();
            };

            nodeCreationRequest = ctx => {
                if (!_areGraphOperationsAllowed) return;

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
                scrollable = true,
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
            if (_blackboard == null) return;

            _blackboardView.Clear();

            if (!_blackboardView.visible) return;

            var section = new BlackboardSection { headerVisible = false };
            var properties = _blackboard.Properties;

            for (int i = 0; i < properties.Count; i++) {
                if (!_blackboard.TryGetProperty(properties[i], out var property)) continue;

                section.Add(BlackboardUtils.CreateBlackboardPropertyView(property));
            }
            
            _blackboardView.Add(section);
        }

        private void OnAddBlackboardPropertyRequest() {
            if (!_areBlackboardOperationsAllowed) return;

            OpenSearchWindow(_blackboardSearchWindow, GetCurrentScreenMousePosition());
        }

        private void CreateBlackboardProperty(Type type) {
            if (!_areBlackboardOperationsAllowed) return;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Add Blackboard Property");

            string typeName = TypeNameFormatter.GetShortTypeName(type);
            if (!_blackboard.TryAddProperty($"New {typeName}", type)) return;

            SetTargetObjectDirtyAndNotify();
            RepopulateBlackboardView();
        }

        private void RemoveBlackboardProperty(string propertyName) {
            if (!_areBlackboardOperationsAllowed) return;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Blackboard Property");

            _blackboard.RemoveProperty(Blackboards.Core.Blackboard.StringToHash(propertyName));

            SetTargetObjectDirtyAndNotify();
        }

        private void OnBlackboardPropertyPositionChanged(BlackboardField field, int newIndex) {
            if (!_areBlackboardOperationsAllowed) return;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Blackboard Property Position Changed");

            if (!_blackboard.TrySetPropertyIndex(Blackboards.Core.Blackboard.StringToHash(field.text), newIndex)) return;

            SetTargetObjectDirtyAndNotify();
            RepopulateBlackboardView();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (!_areBlackboardOperationsAllowed) return;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Blackboard Property Name Changed");

            if (!_blackboard.TrySetPropertyName(Blackboards.Core.Blackboard.StringToHash(field.text), newName)) return;

            field.text = newName;
            SetTargetObjectDirtyAndNotify();
            RepopulateBlackboardView();
        }

        // ---------------- ---------------- Node and connection creation ---------------- ----------------

        private bool TryCreateGroup(BlueprintGroup group, out int id) {
            if (!_areGraphOperationsAllowed) {
                id = default;
                return false;
            }

            group.nodes ??= new List<NodeId>();
            
            foreach (var selectedElement in selection.Cast<GraphElement>()) {
                if (selectedElement is not BlueprintNodeView nodeView) continue;
                group.nodes.Add(nodeView.nodeId);
            }
            
            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Add Group");
            id = _blueprintMeta.GroupStorage.AddGroup(group);
            SetTargetObjectDirtyAndNotify();

            _invalidGroups.Add(id);
            
            return true;
        }
        
        private bool TryCreateNode(Type nodeType, Vector2 position, out NodeId id) {
            if (!_areGraphOperationsAllowed) {
                id = default;
                return false;
            }
            
            var sourceType = BlueprintNodeUtils.GetSourceType(nodeType);

            if (sourceType == null) {
                id = default;
                return false;
            }

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Add Node");
            id = _blueprintMeta.AddNode(sourceType, nodeType, position);
            SetTargetObjectDirtyAndNotify();
            
            return true;
        }

        private bool TryCreateNode(Type nodeType, Vector2 position, string nodeJson, bool expanded, out NodeId id) {
            if (!_areGraphOperationsAllowed) {
                id = default;
                return false;
            }

            var sourceType = BlueprintNodeUtils.GetSourceType(nodeType);

            if (sourceType == null) {
                id = default;
                return false;
            }

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Add Node");
            id = _blueprintMeta.AddNode(sourceType, nodeType, nodeJson, position);
            _blueprintMeta.SetNodeExpandState(id, expanded);
            SetTargetObjectDirtyAndNotify();
            
            return true;
        }

        private bool TryRemoveNode(NodeId id) {
            if (!_areGraphOperationsAllowed) return false;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Node");

            if (!_blueprintMeta.RemoveNode(id)) return false;

            SetTargetObjectDirtyAndNotify();
            return true;
        }
        
        private bool TryRemoveGroup(int id) {
            if (!_areGraphOperationsAllowed) return false;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Node");

            if (!_blueprintMeta.GroupStorage.RemoveGroup(id)) return false;

            SetTargetObjectDirtyAndNotify();
            return true;
        }
        
        private bool TryCreateLink(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            if (!_areGraphOperationsAllowed) return false;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Create Connection");

            if (!_blueprintMeta.TryCreateLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            SetTargetObjectDirtyAndNotify();
            return true;
        }
        
        private bool TryRemoveLink(NodeId fromNodeId, int fromPortIndex, NodeId toNodeId, int toPortIndex) {
            if (!_areGraphOperationsAllowed) return false;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Connection");

            if (!_blueprintMeta.RemoveLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            SetTargetObjectDirtyAndNotify();
            return true;
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprintMeta == null) return change;

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

                    TryCreateLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                }
            }

            if (hasElementsToRemove) for (int i = 0; i < change.elementsToRemove.Count; i++) {
                var element = change.elementsToRemove[i];
                switch (element) {
                    case BlueprintNodeView view:
                        hasNodesToRemove = true;
                        TryRemoveNode(view.nodeId);
                        break;
                    
                    case BlueprintGroupView groupView:
                        hasNodesToRemove = true;
                        TryRemoveGroup(groupView.id);
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

                            TryRemoveLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                        }
                        break;
                }
            }

            // Edge views are to be created in the next RepopulateView() call
            if (hasEdgesToCreate) change.edgesToCreate.Clear();
            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) SetTargetObjectDirtyAndNotify();
            if (repaintBlackboard) RepopulateBlackboardView();

            if (hasEdgesToCreate || hasElementsToRemove) {
                if (hasNodesToRemove) RepopulateView();
                else RepaintInvalidNodes();
            }

            return change;
        }
        
        private void CreateGroupView(BlueprintMeta meta, int id) {
            var groupView = new BlueprintGroupView(meta, id) {
                OnPositionChanged = OnGroupPositionChanged
            };

            groupView.capabilities &= ~Capabilities.Snappable;

            if (!_areGraphOperationsAllowed) {
                groupView.capabilities &= ~Capabilities.Selectable;
                groupView.capabilities &= ~Capabilities.Movable;
                groupView.capabilities &= ~Capabilities.Deletable;
            }

            AddElement(groupView);

            var nodes = meta.GroupStorage.GetGroup(id).nodes;
            
            foreach (var nodeId in nodes) {
                if (FindNodeViewByNodeId(nodeId) is not {} nodeView) continue;
                
                groupView.AddElement(nodeView);   
            }
        }

        private void CreateNodeView(BlueprintMeta meta, NodeId id) {
            var position = meta.GetNodePosition(id);
            var property = GetNodeSerializedProperty(id);

            var nodeView = new BlueprintNodeView(meta, this, id, position, property) {
                OnPositionChanged = OnNodePositionChanged,
                OnValidate = OnNodeSerializedPropertyChanged,
                OnRemoveFromGroup = RemoveNodeFromGroup
            };

            nodeView.capabilities &= ~Capabilities.Snappable;

            if (!_areGraphOperationsAllowed) {
                nodeView.capabilities &= ~Capabilities.Selectable;
                nodeView.capabilities &= ~Capabilities.Movable;
                nodeView.capabilities &= ~Capabilities.Deletable;
            }

            AddElement(nodeView);
        }

        private SerializedProperty GetNodeSerializedProperty(NodeId id) {
            var serializedObject = _virtualSerializedObject ?? _serializedObject;
            string path = serializedObject?.targetObject switch {
                VirtualBlueprintContainer container => container.GetNodePath(id),
                BlueprintRunner runner => runner.GetNodePath(id, _factoryOverride ?? _blueprintMeta.Factory),
                BlueprintAsset asset => asset.GetNodePath(id),
                _ => null,
            };

            var property = serializedObject?.FindProperty(path);

            if (property == null) {
                Debug.LogWarning($"{nameof(BlueprintsView)}: property for node {id} is null, " +
                                 $"meta {_blueprintMeta}, " +
                                 $"factory override is null: {_factoryOverride == null}, "  +
                                 $"serialized object target {_serializedObject?.targetObject}, " +
                                 $"virtual serialized object target {_virtualSerializedObject?.targetObject}");
            }

            return property;
        }

        private async void OnNodeSerializedPropertyChanged(NodeId id) {
            _serializedPropertyChangedNodes.Add(id);
            
            if (_isWaitingEndOfFrameToValidateNodes) return;
            _isWaitingEndOfFrameToValidateNodes = true;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();

            if (_virtualSerializedObject != null) {
                _virtualSerializedObject.ApplyModifiedProperties();
                _virtualSerializedObject.Update();

                _invalidateTracker ??= new BlueprintMetaInvalidateTracker();
                _invalidateTracker.nodesWithInvalidatedLinks.Clear();

                foreach (var nodeId in _serializedPropertyChangedNodes) {
                    if (_virtualFactory?.GetSource(nodeId.source) is not {} virtualSource) continue;

                    var sourceOverride = _factoryOverride?.GetSource(nodeId.source);
                    virtualSource.OnValidate(_invalidateTracker, nodeId);

                    // Node validation caused links invalidation, abort changes
                    if (_invalidateTracker.nodesWithInvalidatedLinks.Contains(nodeId)) {
                        if (sourceOverride != null) {
                            CopyNode(sourceOverride, virtualSource, nodeId.node, add: false);
                        }
                        else if (_blueprintMeta.GetNodeSource(nodeId) is {} originalSource) {
                            CopyNode(originalSource, virtualSource, nodeId.node, add: false);
                        }

                        continue;
                    }

                    bool hasOverride = sourceOverride != null && sourceOverride.ContainsNode(nodeId.node);
                    sourceOverride ??= _factoryOverride?.GetOrCreateSource(nodeId.source, virtualSource.GetType());

                    CopyNode(virtualSource, sourceOverride, nodeId.node, !hasOverride);
                }

                _invalidateTracker.nodesWithInvalidatedLinks.Clear();

                _virtualSerializedObject.ApplyModifiedProperties();
                _virtualSerializedObject.Update();
            }
            else {
                foreach (var nodeId in _serializedPropertyChangedNodes) {
                    _blueprintMeta.GetNodeSource(nodeId)?.OnValidate(_blueprintMeta, nodeId);
                }
            }

            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();

            SetTargetObjectDirtyAndNotify();

            _serializedPropertyChangedNodes.Clear();
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

        private void RemoveNodeFromGroup(NodeId nodeId, int groupId) {
            if (_blueprintMeta == null) return;

            Undo.RecordObject(_serializedObject.targetObject, "Blueprint Remove Node From Group");

            if (!_blueprintMeta.GroupStorage.RemoveNodeFromGroups(nodeId)) return;

            SetTargetObjectDirtyAndNotify();
            
            _invalidGroups.Add(groupId);
            _invalidNodes.Add(nodeId);

            var group = _blueprintMeta.GroupStorage.GetGroup(groupId);
            if (group.nodes is not {Count: > 0}) {
                group.position = _blueprintMeta.GetNodePosition(nodeId);
            }
            
            RepaintInvalidNodes();
        }
        
        private void OnNodePositionChanged(NodeId id) {
            _positionChangedNodes.Add(id);
        }
        
        private void OnGroupPositionChanged(int id) {
            _positionChangedGroups.Add(id);
        }

        private static void CopyNode(IBlueprintSource fromSource, IBlueprintSource toSource, int id, bool add) {
            switch (fromSource) {
                case null:
                    return;

                case BlueprintSources.ICloneable:
                    if (add) toSource?.AddNodeClone(id, fromSource, id);
                    else toSource?.SetNodeClone(id, fromSource, id);
                    break;

                default:
                    string nodeAsString = fromSource.GetNodeAsString(id);
                    var type = fromSource.GetNodeType(id);
                    if (add) toSource?.AddNodeFromString(id, nodeAsString, type);
                    else toSource?.SetNodeFromString(id, nodeAsString, type);
                    break;
            }
        }

        private void CreateNodeLinkViews(BlueprintMeta meta, NodeId id) {
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

                    var edge = fromPortView.ConnectTo(toPortView);
                    if (!_areGraphOperationsAllowed) {
                        edge.capabilities &= ~Capabilities.Selectable;
                        edge.capabilities &= ~Capabilities.Deletable;
                    }

                    AddElement(edge);
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

                    var edge = toPortView.ConnectTo(fromPortView);
                    if (!_areGraphOperationsAllowed) {
                        edge.capabilities &= ~Capabilities.Selectable;
                        edge.capabilities &= ~Capabilities.Deletable;
                    }

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
        
        private void RemoveGroupView(int id) {
            var groupView = FindGroupViewById(id);
            if (groupView == null) return;

            graphViewChanged -= OnGraphViewChanged;
            RemoveElement(groupView);
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
        
        private BlueprintGroupView FindGroupViewById(int id) {
            return GetElementByGuid($"__{nameof(BlueprintGroupView)}_{id}") as BlueprintGroupView;
        }

        public override List<PortView> GetCompatiblePorts(PortView startPortView, NodeAdapter nodeAdapter) {
            if (_blueprintMeta == null) return new List<PortView>();

            var startNodeView = (BlueprintNodeView) startPortView.node;
            var startNodeId = startNodeView.nodeId;
            var startPort = _blueprintMeta.GetPort(startNodeId, startNodeView.GetPortIndex(startPortView));

            return ports
                .Where(portView => {
                    var nodeView = (BlueprintNodeView) portView.node;
                    if (nodeView.nodeId == startNodeId) return false;

                    var port = _blueprintMeta.GetPort(nodeView.nodeId, nodeView.GetPortIndex(portView));
                    return PortValidator.ArePortsCompatible(startPort, port);
                })
                .ToList();
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            if (_blueprintMeta == null) return;

            var portView = edge.input ?? edge.output;
            var nodeView = (BlueprintNodeView) portView.node;

            int portIndex = nodeView.GetPortIndex(portView);
            var port = _blueprintMeta.GetPort(nodeView.nodeId, portIndex);

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
            if (_serializedObject == null) return;

            SetTargetObjectDirtyAndNotify();
            RepopulateView();
        }
        
        // ---------------- ---------------- Copy paste ---------------- ----------------

        private void InitCopyPaste() {
            canPasteSerializedData = CanPaste;
            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnDeserializeAndPaste;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements) {
            var copyData = new CopyPasteData {
                groups = new List<BlueprintGroup>(),
                nodes = new List<CopyPasteData.NodeData>(),
                links = new List<CopyPasteData.LinkData>(),
                position = Vector2.zero,
            };

            var elementArray = elements.ToList();
            for (int i = 0; i < elementArray.Count; i++) {
                var element = elementArray[i];

                if (element is BlueprintNodeView nodeView) {
                    var id = nodeView.nodeId;
                    var position = _blueprintMeta.GetNodePosition(id);
                    var source = _blueprintMeta.GetNodeSource(id);

                    copyData.nodes.Add(new CopyPasteData.NodeData {
                        nodeId = id,
                        position = position,
                        nodeType = new SerializedType(source?.GetNodeType(id.node)),
                        nodeJson = source?.GetNodeAsString(id.node),
                        expanded = GetNodeSerializedProperty(id).isExpanded
                    });

                    copyData.position += position;

                    continue;
                }

                if (element is BlueprintGroupView groupView) {
                    copyData.groups.Add(_blueprintMeta.GroupStorage.GetGroup(groupView.id));
                    elementArray.AddRange(groupView.containedElements);
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

            if (elementArray.Count > 0) copyData.position /= elementArray.Count;

            return JsonUtility.ToJson(copyData);
        }

        private void OnDeserializeAndPaste(string operationName, string data) {
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
            var nodeIdMap = new Dictionary<NodeId, NodeId>();
            var connections = new List<(BlueprintLink, BlueprintLink)>();

            for (int i = 0; i < pasteData.nodes.Count; i++) {
                var nodeData = pasteData.nodes[i];
                var nodeType = nodeData.nodeType.ToType();
                var position = nodeData.position + positionDiff;

                if (TryCreateNode(nodeType, position, nodeData.nodeJson, nodeData.expanded, out var id)) {
                    nodeIdMap[nodeData.nodeId] = id;
                }
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
                        new BlueprintLink { id = fromNodeId, port = link.fromPortIndex },
                        new BlueprintLink { id = toNodeId, port = link.toPortIndex }
                    ));

                    TryCreateLink(fromNodeId, link.fromPortIndex, toNodeId, link.toPortIndex);
                }
            }

            for (int i = 0; i < pasteData.groups.Count; i++) {
                var group = pasteData.groups[i];
                group.nodes = group.nodes.Select(n => nodeIdMap[n]).ToList();

                TryCreateGroup(group, out _);
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
            return _serializedObject != null;
        }

        // ---------------- ---------------- Mouse position ---------------- ----------------

        private void InitMouse() {
            RegisterCallback<MouseMoveEvent>(HandleMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<WheelEvent>(OnMouseWheel);
        }

        private void OnMouseUp(MouseUpEvent evt) {
            switch (evt.button) {
                // lmb
                case 0:
                    WriteChangedPositions();
                    break;
                
                // wheel
                case 3:
                    WritePositionAndZoom();
                    break;
            }
        }

        private void OnMouseWheel(WheelEvent evt) {
            WritePositionAndZoom();
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
