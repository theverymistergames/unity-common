using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Core2;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Port = MisterGames.Blueprints.Core2.Port;
using PortView = UnityEditor.Experimental.GraphView.Port;

namespace MisterGames.Blueprints.Editor.Core2 {

    public sealed class BlueprintsView : GraphView, IEdgeConnectorListener {

        public Func<Vector2, Vector2> OnRequestWorldPosition = _ => Vector2.zero;

        private BlueprintAsset _blueprintAsset;
        private BlueprintSearchWindow _nodeSearchWindow;
        //private BlackboardSearchWindow _blackboardSearchWindow;
        
        private Action _editBlackboardPropertyAction;
        private Blackboard _blackboard;
        private MiniMap _miniMap;
        private Vector2 _mousePosition;

        // ---------------- ---------------- Initialization ---------------- ----------------
        
        public BlueprintsView() {
            InitGrid();
            InitManipulators();
            InitStyle();
            InitUndoRedo();
            //InitCopyPaste();
            InitNodeSearchWindow();
            //InitBlackboard();
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
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            if (_blueprintAsset == null) return;

            EditorUtility.SetDirty(_blueprintAsset);
            RepopulateView();
        }
        /*
        private void InitCopyPaste() {
            canPasteSerializedData = CanPaste;
            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
        }
        */
        private void InitNodeSearchWindow() {
            _nodeSearchWindow = ScriptableObject.CreateInstance<BlueprintSearchWindow>();
            _nodeSearchWindow.onNodeCreationRequest = data => {
                data.position = ConvertPosition(data.position);
                CreateNode(data);
            };

            nodeCreationRequest = ctx => {
                OpenSearchWindow(_nodeSearchWindow, ctx.screenMousePosition);
            };
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

        private void HandleMouseMove(MouseMoveEvent evt) {
            _mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
        }

        // ---------------- ---------------- Population ---------------- ----------------
        
        public void PopulateViewFromAsset(BlueprintAsset blueprintAsset) {
            if (blueprintAsset == _blueprintAsset) return;

            _blueprintAsset = blueprintAsset;

            if (_blueprintAsset.BlueprintMeta.Invalidate()) EditorUtility.SetDirty(_blueprintAsset);

            RepopulateView();

            _blueprintAsset.BlueprintMeta.OnInvalidate = () => {
                EditorUtility.SetDirty(_blueprintAsset);
                RepopulateView();
            };

            _blueprintAsset.BlueprintMeta.OnInvalidateNode = nodeId => {
                EditorUtility.SetDirty(_blueprintAsset);
                RepopulateView();
            };
        }

        private void RepopulateView() {
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

            /*
            _blackboard.Clear();
            var blackboard = _blueprint.Blackboard as IBlackboardEditor;
            foreach (var property in blackboard.Properties) {
                var view = BlackboardUtils.CreateBlackboardPropertyView(property, OnBlackboardPropertyValueChanged);
                _blackboard.Add(view);
            }
            */
            ClearSelection();
        }

        public void ClearView() {
            _blueprintAsset = null;
            DeleteElements(graphElements);
            ClearSelection();
        }

        // ---------------- ---------------- Blackboard ---------------- ----------------
        /*
        private void InitBlackboard() {
            //_blackboardSearchWindow = ScriptableObject.CreateInstance<BlackboardSearchWindow>();
            //_blackboardSearchWindow.onSelectType = OnAddBlackboardProperty;

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

        private void OnAddBlackboardPropertyRequest() {
            if (_currentBlueprintAsset == null) return;
            
            var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            OpenSearchWindow(_blackboardSearchWindow, position);
        }

        private void OnAddBlackboardProperty(Type type) {
            if (_currentBlueprintAsset == null) return;

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
        */
        // ---------------- ---------------- Menu ---------------- ----------------

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (_blueprintAsset == null) return;

            base.BuildContextualMenu(evt);
        }

        private void OpenSearchWindow<T>(T window, Vector2 position) where T : ScriptableObject, ISearchWindowProvider {
            SearchWindow.Open(new SearchWindowContext(position), window);
        }

        public void ToggleMiniMap(bool show) {
            _miniMap.visible = show;
        }
        
        public void ToggleBlackboard(bool show) {
            /*
            _blackboard.visible = show;
            var children = _blackboard.contentContainer.Children();
            foreach (var child in children) {
                child.visible = show;
            }
            */
        }
        
        // ---------------- ---------------- Node creation ---------------- ----------------

        private Vector2 ConvertPosition(Vector2 position) {
            var worldPosition = OnRequestWorldPosition.Invoke(position);
            return contentViewContainer.WorldToLocal(worldPosition);
        }
        
        private void CreateNode(NodeCreationData data) {
            var nodeMeta = BlueprintNodeMeta.Create(data.type);
            nodeMeta.Position = data.position;

            Undo.RecordObject(_blueprintAsset, "Blueprint Add Node");

            _blueprintAsset.BlueprintMeta.AddNode(nodeMeta);

            EditorUtility.SetDirty(_blueprintAsset);

            CreateNodeView(nodeMeta);
        }

        private void RemoveNode(BlueprintNodeMeta nodeMeta) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Node");

            _blueprintAsset.BlueprintMeta.RemoveNode(nodeMeta.NodeId);

            EditorUtility.SetDirty(_blueprintAsset);
        }
        
        private void CreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Create Connection");

            _blueprintAsset.BlueprintMeta.TryCreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            EditorUtility.SetDirty(_blueprintAsset);
        }
        
        private void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            Undo.RecordObject(_blueprintAsset, "Blueprint Remove Connection");

            _blueprintAsset.BlueprintMeta.RemoveConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);

            EditorUtility.SetDirty(_blueprintAsset);
        }

        // ---------------- ---------------- View creation ---------------- ----------------

        private void CreateNodeView(BlueprintNodeMeta nodeMeta) {
            var nodeView = new BlueprintNodeView(nodeMeta, _blueprintAsset) {
                OnPositionChanged = OnPositionChanged,
                OnValidate = OnValidateNode
            };

            AddElement(nodeView);
        }

        private void OnValidateNode(BlueprintNodeMeta nodeMeta) {
            if (_blueprintAsset == null) return;

            EditorUtility.SetDirty(_blueprintAsset);
        }

        private void OnPositionChanged(BlueprintNodeMeta nodeMeta, Vector2 position) {
            if (_blueprintAsset == null) return;

            Undo.RecordObject(_blueprintAsset, "Blueprint Node Position Changed");

            nodeMeta.Position = position;

            EditorUtility.SetDirty(_blueprintAsset);
        }

        private void CreateNodeConnectionViews(BlueprintNodeMeta nodeMeta) {
            var blueprintMeta = _blueprintAsset.BlueprintMeta;

            var fromNodeView = FindNodeViewByNodeId(nodeMeta.NodeId);
            var fromNodePorts = nodeMeta.Ports;

            for (int p = 0; p < fromNodePorts.Count; p++) {
                var port = fromNodePorts[p];
                if (port.isExternalPort) continue;

                var fromPortView = GetPortView(fromNodeView, p, port.isExitPort);
                var links = blueprintMeta.GetLinksFromNodePort(nodeMeta.NodeId, p);

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    var toNodeView = FindNodeViewByNodeId(link.nodeId);
                    var toPort = toNodeView.nodeMeta.Ports[link.portIndex];
                    var toPortView = GetPortView(toNodeView, link.portIndex, toPort.isExitPort);

                    AddElement(fromPortView.ConnectTo(toPortView));
                }
            }
        }

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
                    /*
                    case BlackboardField field:
                        var blackboard = _blueprint.Blackboard as IBlackboardEditor;
                        blackboard.RemoveProperty(field.text);
                        break;
                    */
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

            if (hasElementsToRemove || hasEdgesToCreate) {
               //RepopulateView();
            }
            
            if (hasMovedElements || hasElementsToRemove || hasEdgesToCreate) {
                Undo.RecordObject(_blueprintAsset, "Blueprint Changed");
                EditorUtility.SetDirty(_blueprintAsset);
            }

            return change;
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
                if (port.isExitPort != isExit) continue;
                
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
                if (port.isExitPort != isExit) continue;

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
        /*
        private static string OnSerializeGraphElements(IEnumerable<GraphElement> elements) {
            var copiedNodes = new List<BlueprintNode>();
            var copiedLinks = new List<CopyPasteLink>();
            
            foreach (var element in elements) {
                if (element is BlueprintNodeView nodeView) {
                    var original = nodeView.NodeMeta;
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
                        fromNodeGuid = fromNodeView.NodeMeta.Guid,
                        toNodeGuid = toNodeView.NodeMeta.Guid,
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
                
                var fromNode = FindNodeViewByGuid(fromNodeGuid).NodeMeta;
                var toNode = FindNodeViewByGuid(toNodeGuid).NodeMeta;

                int fromPort = link.fromPort;
                int toPort = link.toPort;
                
                CreateConnection(fromNode, fromPort, toNode, toPort);
            }
            
            ScheduleRepopulate();
        }

        private bool CanPaste(string data) {
            return _currentBlueprintAsset != null;
        }
        */

        // ---------------- ---------------- Nested classes ---------------- ----------------
        
        public new class UxmlFactory : UxmlFactory<BlueprintsView, UxmlTraits> { }
    }

}
