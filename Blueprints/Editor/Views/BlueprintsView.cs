using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Editor.Blueprints.Editor.Utils;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Coroutines;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using MisterGames.Fsm.Editor.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;
using Object = UnityEngine.Object;
using Port = MisterGames.Blueprints.Core.Port;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Views {

    public class BlueprintsView : GraphView, IEdgeConnectorListener {
        
        private static bool EnableLogs = false;

        private const float SearchWindowStartDelaySec = 0.1f;
        private const float SearchWindowRetryPeriodSec = 0.25f;
        private const int SearchWindowRetryTimes = 10;
        
        private const float RepopulateViewDelaySec = 0.1f;
        private const float EditBlackboardPropertyDelaySec = 3f;
        
        internal Func<Vector2, Vector2> OnRequestWorldPosition = position => Vector2.zero;
        
        private EditMode _editMode = EditMode.None;

        private Blueprint _blueprint;
        private BlueprintSearchWindow _nodeSearchWindow;
        private BlackboardSearchWindow _blackboardSearchWindow;
        
        private EditorCoroutineTask _openSearchWindowTask;
        private EditorCoroutineTask _repopulateViewTask;
        private EditorCoroutineTask _editBlackboardPropertyViewTask;
        
        private Action _editBlackboardPropertyAction;
        private Blackboard _blackboard;
        private MiniMap _miniMap;
        private IBlueprintHost _host;
        private Vector2 _mousePosition;

        // ---------------- ---------------- Initialization ---------------- ----------------
        
        public BlueprintsView() {
            InitGrid();
            InitManipulators();
            InitStyle();
            InitUndoRedo();
            InitCopyPaste();
            InitSearchWindows();
            InitBlackboard();
            InitMiniMap();
            InitMouse();
        }

        private void InitGrid() {
            Insert(0, new GridBackground());
        }
        
        private void InitManipulators() {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }
        
        private void InitStyle() {
            var styleSheet = Resources.Load<StyleSheet>("BlueprintsEditorViewStyle");
            styleSheets.Add(styleSheet);
        }

        private void InitUndoRedo() {
            Undo.undoRedoPerformed += OnUndoRedo;
        }
        
        private void InitCopyPaste() {
            canPasteSerializedData = CanPaste;
            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
        }

        private void InitSearchWindows() {
            _blackboardSearchWindow = ScriptableObject.CreateInstance<BlackboardSearchWindow>();
            _blackboardSearchWindow.onSelectType = OnAddBlackboardProperty;
            
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintSearchWindow>();
            _nodeSearchWindow.onNodeCreationRequest = data => {
                data.position = ConvertPosition(data.position);
                CreateNode(data);
            };

            nodeCreationRequest = ctx => {
                Log("Node creation request received");
                OpenSearchWindow(_nodeSearchWindow, ctx.screenMousePosition);
            };
        }

        private void InitBlackboard() {
            _blackboard = new Blackboard(this) {
                windowed = false,
                addItemRequested = _ => { OnAddBlackboardPropertyRequest(); },
                moveItemRequested = (_, index, element) => {
                    OnBlackboardPropertyPositionChanged((BlackboardField) element, index);
                },
                editTextRequested = (_, element, newName) => {
                    OnBlackboardPropertyNameChanged((BlackboardField) element, newName);
                }
            };
            _blackboard.SetPosition(new Rect(0, 0, 300, 300));
            Add(_blackboard);
            _blackboard.visible = false;
        }
        
        private void InitMiniMap() {
            _miniMap = new MiniMap { windowed = false };
            _miniMap.SetPosition(new Rect(600, 0, 400, 250));
            Add(_miniMap);
            _miniMap.visible = false;
        }

        private void InitMouse() {
            RegisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        // ---------------- ---------------- Population ---------------- ----------------
        
        internal void PopulateViewFromAsset(Blueprint asset) {
            Log($"Populate view from asset {asset.ToStringNullSafe()}, previous {_blueprint.ToStringNullSafe()}");
            UnsubscribeRuntimeInstance();
            Reload(asset, EditMode.Asset);
        }

        internal void PopulateViewFromHost(IBlueprintHost host) {
            Log("Populate view from runner");
            UnsubscribeRuntimeInstance();
            Reload(host.Instance, EditMode.RunnerInstance);
            SubscribeRuntimeInstance(host);
            RepopulateView();
        }
        
        private void SubscribeRuntimeInstance(IBlueprintHost host) {
            _host = host;
            _host.OnFlow -= HandleFlow;
            _host.OnFlow += HandleFlow;
        }

        private void UnsubscribeRuntimeInstance() {
            if (_host == null) return;
            _host.OnFlow -= HandleFlow;
            _host = null;
        }
        
        private void HandleFlow(BlueprintNode from, BlueprintNode to) {
            Log($"Entered runtime node {from} -> {to}");
            
            // todo visualize edges
            //var fromNodeView = FindNodeViewByGuid(from.Guid);
            var toNodeView = FindNodeViewByGuid(to.Guid);
            toNodeView?.VisualizeFlow();
        }
        
        private void Reload(Blueprint instance, EditMode mode) {
            Log("Reload: " +
                $"previous {_blueprint.ToStringNullSafe()}, " +
                $"new {instance.ToStringNullSafe()}, " +
                $"previous == new: {instance == _blueprint}, ");

            if (instance == null) return;
            if (_blueprint == instance) return;
            
            Dispose();
            
            Log("Reload: reloading");
            SetNewEditMode(mode);
            
            _blueprint = instance;
            InvalidateAsset();
            InitBlueprintPorts();
        }

        private void InitBlueprintPorts() {
            if (_editMode != EditMode.Asset) return;
            var iNode = _blueprint.AsIBlueprintNode();
            iNode.OnPortsUpdated -= ScheduleRepopulate;
            iNode.OnPortsUpdated += ScheduleRepopulate;
            iNode.InitPorts();
        }
        
        private void DeInitBlueprintPorts() {
            if (_blueprint == null || _editMode != EditMode.Asset) return;
            var iNode = _blueprint.AsIBlueprintNode();
            iNode.OnPortsUpdated -= ScheduleRepopulate;
            iNode.InitPorts();
            iNode.DeInitPorts();
        }
        
        private void RepopulateView() {
            _repopulateViewTask?.Cancel();
            
            if (_blueprint == null) return;
            Log("Repopulate view");
            
            // ReSharper disable once DelegateSubtraction
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;
            
            var blueprintNodes = _blueprint.AsIBlueprint().Nodes;
            foreach (var node in blueprintNodes) {
                CreateNodeView(node);
            }
            foreach (var node in blueprintNodes) {
                CreateLinkViews(node);
            }
            
            _blackboard.Clear();
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            foreach (var property in blackboard.Properties) {
                var view = BlackboardUtils.CreateBlackboardPropertyView(property, OnBlackboardPropertyValueChanged);
                _blackboard.Add(view);
            }
            
            if (_editMode == EditMode.RunnerInstance) FreezeGraphElements();
            
            ClearSelection();
        }

        private void FreezeGraphElements() {
            graphElements.ForEach(element => element.capabilities = 0);
        }

        private void SetNewEditMode(EditMode mode) {
            Log($"Change edit mode from {_editMode} to {mode}");
            _editMode = mode;
        }

        internal void ClearView() {
            SetNewEditMode(EditMode.None);
            if (_blueprint != null) DeInitBlueprintPorts();
            _blueprint = null;
            DeleteElements(graphElements);
            ClearSelection();
        }
        
        internal void OnDestroyEditorWindow() {
            Log("On destroy editor window");
            Dispose();
            _blueprint = null;
        }

        private void Dispose() {
            if (_blueprint != null) DeInitBlueprintPorts();

            foreach (var element in graphElements) {
                if (element is BlueprintNodeView view) view.Dispose();
            }
            
            _openSearchWindowTask?.Cancel();
            _repopulateViewTask?.Cancel();
            _editBlackboardPropertyViewTask?.Cancel();
            _editBlackboardPropertyAction?.Invoke();
        }

        // ---------------- ---------------- Blackboard ---------------- ----------------
        
        private void OnAddBlackboardPropertyRequest() {
            if (_blueprint == null) return;
            
            var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            OpenSearchWindow(_blackboardSearchWindow, position);
        }

        private void OnAddBlackboardProperty(Type type) {
            if (_blueprint == null) return;
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            if (!blackboard.ValidateType(type)) return;

            Undo.RecordObject(_blueprint, "Blueprint Add Blackboard Property");
            
            string typeName = Common.Data.Blackboard.GetTypeName(type);
            var property = blackboard.AddProperty($"New {typeName}", type);
            SaveAsset();
            
            var view = BlackboardUtils.CreateBlackboardPropertyView(property, OnBlackboardPropertyValueChanged);
            _blackboard.Add(view);
        }

        private void OnBlackboardPropertyPositionChanged(BlackboardField field, int newIndex) {
            if (_blueprint == null) return;
            
            Undo.RecordObject(_blueprint, "Blueprint Blackboard Property Position Changed");
            
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            blackboard.SetPropertyIndex(field.text, newIndex);
            SaveAsset();
        }

        private void OnBlackboardPropertyNameChanged(BlackboardField field, string newName) {
            if (_blueprint == null) return;
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            string oldName = field.text;
            if (blackboard.SetPropertyName(oldName, newName, out string propertyName)) {
                Undo.RecordObject(_blueprint, "Blueprint Blackboard Property Name Changed");
                field.text = propertyName;
                SaveAsset();
            }
        }

        private void OnBlackboardPropertyValueChanged(string property, object value) {
            _editBlackboardPropertyViewTask?.Cancel();
            _editBlackboardPropertyAction = () => { ChangeBlackboardPropertyValue(property, value); };
            var routine = EditorCoroutines.Delay(EditBlackboardPropertyDelaySec, _editBlackboardPropertyAction);
            _editBlackboardPropertyViewTask = EditorCoroutines.StartCoroutine(this, routine);
        }

        private void ChangeBlackboardPropertyValue(string property, object value) {
            _editBlackboardPropertyAction = null;
            
            Undo.RecordObject(_blueprint, "Blueprint Blackboard Property Value Changed");
            
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            blackboard.SetPropertyValue(property, value);
            SaveAsset();
        }
        
        // ---------------- ---------------- Menu ---------------- ----------------
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_editMode != EditMode.Asset) return;
            base.BuildContextualMenu(evt);
        }
        
        private void OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            Log("Start open search window task");
            var routine = EditorCoroutines.ScheduleTimesWhile(
                SearchWindowStartDelaySec,
                SearchWindowRetryPeriodSec,
                SearchWindowRetryTimes,
                () => !SearchWindow.Open(new SearchWindowContext(position), window)
            );
            _openSearchWindowTask = EditorCoroutines.StartCoroutine(this, routine);
        }

        internal void OnEnteredEditMode() {
            if (_editMode == EditMode.RunnerInstance) ClearView();
        }
        
        internal void ToggleMiniMap(bool show) {
            _miniMap.visible = show;
        }
        
        internal void ToggleBlackboard(bool show) {
            _blackboard.visible = show;
            var children = _blackboard.contentContainer.Children();
            foreach (var child in children) {
                child.visible = show;
            }
        }
        
        // ---------------- ---------------- Node creation ---------------- ----------------

        private Vector2 ConvertPosition(Vector2 position) {
            var worldPosition = OnRequestWorldPosition.Invoke(position);
            return contentViewContainer.WorldToLocal(worldPosition);
        }
        
        private BlueprintNode CreateNode(NodeCreationData data) {
            Log($"Create node: {data.name} {data.type.Name}");

            var node = (BlueprintNode) ScriptableObject.CreateInstance(data.type);
            var iNode = node.AsIBlueprintNode();
            
            node.name = data.name;
            iNode.Position = data.position;
            
            Undo.RecordObject(_blueprint, "Blueprint Add Node");
            Undo.RecordObject(node, "Blueprint Created Node");
            _blueprint.AsIBlueprint().AddNode(node);
            
            AddToAsset(node);
            SaveAsset();

            return node;
        }

        private void DeleteNode(BlueprintNodeView view) {
            Log($"Delete node: {view.Node.name}");
            var node = view.Node;
            
            Undo.RecordObject(_blueprint, "Blueprint Removed Node");
            Undo.RecordObject(node, "Blueprint Deleted Node");
            
            _blueprint.AsIBlueprint().RemoveNode(node);
            
            RemoveFromAsset(node);
            SaveAsset();
        }
        
        private void CreateLink(BlueprintNode fromNode, int fromPort, BlueprintNode toNode, int toPort) {
            Undo.RecordObject(_blueprint, "Blueprint Added Link");
            Undo.RecordObject(fromNode, "Blueprint Added Link Node 1");
            Undo.RecordObject(toNode, "Blueprint Added Link Node 2");
            
            var iFromNode = fromNode.AsIBlueprintNode();
            var iToNode = toNode.AsIBlueprintNode();

            iFromNode.ConnectPort(fromPort, toNode, toPort);
            iToNode.ConnectPort(toPort, fromNode, fromPort);
            
            iFromNode.InitPorts();
            iToNode.InitPorts();
            
            ChangeInAsset(fromNode);
            ChangeInAsset(toNode);
        }
        
        private void DeleteLink(BlueprintNode fromNode, int fromPort, BlueprintNode toNode, int toPort) {
            Undo.RecordObject(_blueprint, "Blueprint Removed Link");
            Undo.RecordObject(fromNode, "Blueprint Removed Link Node 1");
            Undo.RecordObject(toNode, "Blueprint Removed Link Node 2");
            
            var iFromNode = fromNode.AsIBlueprintNode();
            var iToNode = toNode.AsIBlueprintNode();
            
            iFromNode.DisconnectPort(fromPort, toNode, toPort);
            iToNode.DisconnectPort(toPort, fromNode, fromPort);
                            
            iFromNode.InitPorts();
            iToNode.InitPorts();
                            
            ChangeInAsset(fromNode);
            ChangeInAsset(toNode);
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private void CreateNodeView(BlueprintNode node) {
            var view = new BlueprintNodeView(node) { OnPositionChanged = OnPositionChanged };
            AddElement(view);
        }

        private void CreateLinkViews(BlueprintNode node) {
            var view = FindNodeViewByGuid(node.Guid);
            var nodePorts = node.AsIBlueprintNode().Ports;
            
            for (int p = 0; p < nodePorts.Length; p++) {
                var port = nodePorts[p];
                if (port.IsExposed) continue;
                
                var links = port.Links;
                for (int l = 0; l < links.Length; l++) {
                    var link = links[l];
                    var remote = link.remote;
                    
                    var remoteView = FindNodeViewByGuid(remote.Guid);
                    if (remoteView == null) continue;
                    
                    int remotePortIndex = link.remotePort;
                    var remotePort = remote.AsIBlueprintNode().Ports[remotePortIndex];
                    
                    var sourcePortView = GetPortView(view, p, port.IsExit);
                    var remotePortView = GetPortView(remoteView, remotePortIndex, remotePort.IsExit);
                    
                    var edge = sourcePortView.ConnectTo(remotePortView);
                    AddElement(edge);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_blueprint == null) return change;
            
            Log("Graph view changed: " + 
                $"need create edges {change.edgesToCreate?.Count}, " +
                $"need remove elements {change.elementsToRemove?.Count}, " +
                $"moved elements {change.movedElements?.Count}");
            
            change.edgesToCreate?.ForEach(edge => {
                if (edge.input.node is BlueprintNodeView toView && edge.output.node is BlueprintNodeView fromView) {
                    var fromNode = fromView.Node;
                    var toNode = toView.Node;

                    int fromPort = GetPortIndex(edge.output);
                    int toPort = GetPortIndex(edge.input);

                    CreateLink(fromNode, fromPort, toNode, toPort);
                }
            });
            
            change.elementsToRemove?.ForEach(e => {
                switch (e) {
                    case BlueprintNodeView view:
                        DeleteNode(view);
                        break;
                    
                    case BlackboardField field:
                        var blackboard = _blueprint.Blackboard as IBlackboardEditor;
                        blackboard.RemoveProperty(field.text);
                        break;
                    
                    case Edge edge:
                        if (edge.input.node is BlueprintNodeView toView && edge.output.node is BlueprintNodeView fromView) {
                            var fromNode = fromView.Node;
                            var toNode = toView.Node;

                            int fromPort = GetPortIndex(edge.output);
                            int toPort = GetPortIndex(edge.input);
                            
                            DeleteLink(fromNode, fromPort, toNode, toPort);
                        }
                        break;
                }
            });

            bool hasElementsToRemove = change.elementsToRemove != null && change.elementsToRemove.Count > 0;
            bool hasMovedElements = change.movedElements != null && change.movedElements.Count > 0;
            bool hasEdgesToCreate = change.edgesToCreate != null && change.edgesToCreate.Count > 0;

            if (hasElementsToRemove || hasEdgesToCreate) {
               ScheduleRepopulate();
            }
            
            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) {
                Undo.RecordObject(_blueprint, "Blueprint Changed");
                SaveAsset();
            }

            return change;
        }

        private void ScheduleRepopulate() {
            Log("Schedule Repopulate view");
            _repopulateViewTask?.Cancel();
            var routine = EditorCoroutines.Delay(RepopulateViewDelaySec, RepopulateView);
            _repopulateViewTask = EditorCoroutines.StartCoroutine(this, routine);
        }

        private void OnPositionChanged(BlueprintNode node, Vector2 position) {
            if (_blueprint == null) return;
            Undo.RecordObject(node, "Blueprint Node Position Changed Node");
            Undo.RecordObject(_blueprint, "Blueprint Node Position Changed");
            node.AsIBlueprintNode().Position = position;
        }
        
        private BlueprintNodeView FindNodeViewByGuid(string guid) {
            return GetNodeByGuid(guid) as BlueprintNodeView;
        }

        public override List<PortView> GetCompatiblePorts(PortView startPortView, NodeAdapter nodeAdapter) {
            if (_editMode != EditMode.Asset) return new List<PortView>();
            var startPort = GetPort(startPortView);
            var startNode = GetNode(startPortView);
            return ports.Where(portView => 
                    portView.node != startPortView.node && 
                    portView.direction != startPortView.direction &&
                    GetPort(portView).IsCompatibleWith(GetNode(portView), startPort, startNode)
                )
                .ToList();
        }
        
        private static BlueprintNode GetNode(PortView view) {
            return ((BlueprintNodeView) view.node).Node;
        }

        private static Port GetPort(PortView view) {
            return GetNode(view).AsIBlueprintNode().Ports[GetPortIndex(view)];
        }

        private static PortView GetPortView(BlueprintNodeView view, int index, bool isExit) {
            var ports = view.Node.AsIBlueprintNode().Ports;
            int portIndex = -1;
            
            for (int i = 0; i < ports.Length; i++) {
                var port = ports[i];
                if (port.IsExit != isExit) continue;
                
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
            var ports = ((BlueprintNodeView) node).Node.AsIBlueprintNode().Ports;

            int count = -1;
            for (int i = 0; i < ports.Length; i++) {
                var port = ports[i];
                if (port.IsExit != isExit) continue;
                
                if (++count == portIndex) {
                    portIndex = i;
                    break;
                }
            }

            return portIndex;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }

        public void OnDrop(GraphView graphView, Edge edge) { }
        
        // ---------------- ---------------- Copy paste ---------------- ----------------
        
        private void HandleMouseMove(MouseMoveEvent evt) {
            _mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
        }
        
        private static string OnSerializeGraphElements(IEnumerable<GraphElement> elements) {
            var copiedNodes = new List<BlueprintNode>();
            var copiedLinks = new List<CopyPasteLink>();
            
            foreach (var element in elements) {
                if (element is BlueprintNodeView nodeView) {
                    var original = nodeView.Node;
                    var clone = Object.Instantiate(original);
                    
                    clone.name = original.name;
                    clone.AsIBlueprintNode().DisconnectAllPorts();
                    
                    copiedNodes.Add(clone);
                    continue;
                }

                if (element is Edge edge && 
                    edge.input.node is BlueprintNodeView toNodeView && 
                    edge.output.node is BlueprintNodeView fromNodeView) {
                    
                    copiedLinks.Add(new CopyPasteLink {
                        fromNodeGuid = fromNodeView.Node.Guid,
                        toNodeGuid = toNodeView.Node.Guid,
                        fromPort = GetPortIndex(edge.output),
                        toPort = GetPortIndex(edge.input)
                    });
                }
            }
            
            return CopyData.Serialize(copiedNodes, copiedLinks);
        }

        private void OnUnserializeAndPaste(string operationName, string data) {
            var pasteData = PasteData.Deserialize(data);

            if (pasteData.nodes.Count == 0) return;

            var positionDiff = _mousePosition - pasteData.position;
            var guidMap = new Dictionary<string, string>();
            
            foreach (var nodeData in pasteData.nodes) {
                var creationData = new NodeCreationData {
                    name = nodeData.name,
                    position = nodeData.position + positionDiff,
                    type = nodeData.type
                };

                var node = CreateNode(creationData);
                guidMap[nodeData.guid] = node.Guid;
            }
            
            RepopulateView();
            
            foreach (var link in pasteData.links) {
                if (!guidMap.ContainsKey(link.fromNodeGuid) || !guidMap.ContainsKey(link.toNodeGuid)) {
                    continue;
                }
                
                string fromNodeGuid = guidMap[link.fromNodeGuid];
                string toNodeGuid = guidMap[link.toNodeGuid];
                
                var fromNode = FindNodeViewByGuid(fromNodeGuid).Node;
                var toNode = FindNodeViewByGuid(toNodeGuid).Node;

                int fromPort = link.fromPort;
                int toPort = link.toPort;
                
                CreateLink(fromNode, fromPort, toNode, toPort);
            }
            
            ScheduleRepopulate();
        }

        private bool CanPaste(string data) {
            return _editMode == EditMode.Asset && _blueprint != null;
        }
        
        // ---------------- ---------------- Assets ---------------- ----------------
        
        private void OnUndoRedo() {
            if (_blueprint == null) return;
            Log("On undo redo");
            InvalidateAsset();
            RepopulateView();
            ClearSelection();
        }
        
        private bool CanOpenAsset() {
            if (_editMode == EditMode.None) return false;
            return _blueprint != null && AssetDatabase.CanOpenAssetInEditor(_blueprint.GetInstanceID());
        }
        
        private void InvalidateAsset() {
            if (!CanOpenAsset()) return;
            
            string path = AssetDatabase.GetAssetPath(_blueprint);
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            
            foreach (var subAsset in subAssets) {
                if (subAsset == null) continue;
                RemoveFromAsset(subAsset);
            }
            
            var iBlueprint = _blueprint.AsIBlueprint();
            var blueprintNodes = iBlueprint.Nodes;

            foreach (var node in blueprintNodes) {
                AddToAsset(node);
            }
            
            SaveAsset();
        }

        private void AddToAsset(Object obj) {
            if (!CanOpenAsset() || obj == null) return;
            AssetDatabase.AddObjectToAsset(obj, _blueprint);
            EditorUtility.SetDirty(obj);
            EditorUtility.SetDirty(_blueprint);
        }

        private void ChangeInAsset(Object obj) {
            if (!CanOpenAsset() || obj == null) return;
            AssetDatabase.RemoveObjectFromAsset(obj);
            AssetDatabase.AddObjectToAsset(obj, _blueprint);
            EditorUtility.SetDirty(_blueprint);
            EditorUtility.SetDirty(obj);
        }
        
        private void RemoveFromAsset(Object obj) {
            if (!CanOpenAsset() || obj == null) return;
            AssetDatabase.RemoveObjectFromAsset(obj);
            EditorUtility.SetDirty(_blueprint);
            EditorUtility.SetDirty(obj);

        }

        private void SaveAsset() {
            if (!CanOpenAsset()) return;
            EditorUtility.SetDirty(_blueprint);
            AssetDatabase.SaveAssets();
        }
        
        internal bool IsAssetDestroyed() {
            return _blueprint == null;
        }
        
        // ---------------- ---------------- Logs ---------------- ----------------

        private static void Log(string message) {
            if (EnableLogs) Debug.Log($"Blueprints Editor: {message}");
        }
        
        // ---------------- ---------------- Nested classes ---------------- ----------------
        
        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
        
        private enum EditMode {
            None,
            Asset,
            RunnerInstance
        }
    }

}