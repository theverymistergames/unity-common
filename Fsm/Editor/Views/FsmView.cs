using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Coroutines;
using MisterGames.Common.Editor.Menu;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using MisterGames.Fsm.Core;
using MisterGames.Fsm.Editor.Data;
using MisterGames.Fsm.Editor.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MisterGames.Fsm.Editor.Views {

    public class FsmView : GraphView, IEdgeConnectorListener {

        private static bool EnableLogs = false;

        private const string ChangeStateNameDialogTitle = "Change state name";
        
        private const float SearchWindowStartDelaySec = 0.1f;
        private const float SearchWindowRetryPeriodSec = 0.25f;
        private const int SearchWindowRetryTimes = 10;

        private const float RepopulateViewDelaySec = 0.1f;
        private const float SelectViewDelaySec = 0.2f;

        internal Action<Object> OnObjectSelected = delegate {  };
        internal Action OnNothingSelected = delegate {  };
        
        internal Func<Vector2, Vector2> OnRequestWorldPosition = position => Vector2.zero;
        internal Func<Vector2, Vector2> OnRequestLocalPosition = position => Vector2.zero;
        
        private StateMachine _stateMachine;
        private StateMachineRunner _runner;
        
        private GraphSearchWindow _graphSearchWindow;
        internal EditMode editMode = EditMode.None;
        private readonly TransitionCreationData _transitionData = new TransitionCreationData();
        
        private EditorCoroutineTask _openSearchWindowTask;
        private EditorCoroutineTask _openRenameStateDialogTask;
        private EditorCoroutineTask _repopulateViewTask;
        private EditorCoroutineTask _selectTask;
        
        // ---------------- ---------------- Initialization ---------------- ----------------
        
        public FsmView() {
            InitGrid();
            InitManipulators();
            InitStyle();
            InitCallbacks();
            InitUndoRedo();
            InitSearchWindows();
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
            var styleSheet = Resources.Load<StyleSheet>("FsmEditorViewStyle");
            styleSheets.Add(styleSheet);
        }

        private void InitCallbacks() {
            RegisterCallback<MouseUpEvent>(HandleMouseUp);
            RegisterCallback<ClickEvent>(HandleClick);
        }
        
        private void InitUndoRedo() {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void InitSearchWindows() {
            _graphSearchWindow = ScriptableObject.CreateInstance<GraphSearchWindow>();
            _graphSearchWindow.onStateCreationRequest = CreateState;
            _graphSearchWindow.onTargetStateCreationRequest = CreateTargetState;
            _graphSearchWindow.onTransitionCreationRequest = CreateTransition;
            _graphSearchWindow.onStatesRequest = GetTransitableStatesForPendingTransition;

            nodeCreationRequest = ctx => {
                Log("Node creation request received");
                OpenSearchWindow(ctx.screenMousePosition, GraphSearchWindow.Filter.NewState);    
            };
        }
        
        // ---------------- ---------------- Population ---------------- ----------------

        internal void PopulateViewFromAsset(StateMachine asset) {
            Log($"Populate view from asset {asset.ToStringNullSafe()}, previous {_stateMachine.ToStringNullSafe()}");
            UnsubscribeRuntimeInstance();
            Reload(asset, EditMode.Asset);
        }
        
        internal void PopulateViewFromRunner(StateMachineRunner runner) {
            Log("Populate view from runner");
            UnsubscribeRuntimeInstance();
            Reload(runner.Source, EditMode.RunnerInstance);
            SubscribeRuntimeInstance(runner);
        }

        internal void ClearView() {
            Log("Clear view");
            UnsubscribeRuntimeInstance();
            SetNewEditMode(EditMode.None);
            _stateMachine = null;
            DeleteElements(graphElements);
            ClearSelection();
            UpdateSelection();
        }
        
        private void Reload(StateMachine instance, EditMode mode) {
            Log("Reload: " +
                $"previous {_stateMachine.ToStringNullSafe()}, " +
                $"new {instance.ToStringNullSafe()}, " +
                $"previous == new: {instance == _stateMachine}");
            
            if (instance == null) return;
            
            Log("Reload: reloading");
            SetNewEditMode(mode);
            
            if (_stateMachine == instance) {
                RepopulateView();
                return;
            }
            
            _stateMachine = instance;
            RepopulateView();
        }
        
        private void RepopulateView() {
            if (_stateMachine == null) return;
            Log("Repopulate view");
            
            // ReSharper disable once DelegateSubtraction
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;
            
            _stateMachine.states.ForEach(CreateStateView);
            _stateMachine.states.ForEach(s => s.transitions.ForEach(t => {
                CreateTargetStateView(t);
                CreateTransitionView(s, t);
            }));
            
            if (editMode == EditMode.RunnerInstance) FreezeGraphElements();
            
            ClearSelection();
            UpdateSelection();
        }

        private void SetNewEditMode(EditMode mode) {
            Log($"Change edit mode from {editMode} to {mode}");
            editMode = mode;
        }

        private void SubscribeRuntimeInstance(StateMachineRunner runner) {
            _runner = runner;
            _runner.OnEnterState += HandleEnteredRuntimeState;
        }

        private void UnsubscribeRuntimeInstance() {
            if (_runner == null) return;
            _runner.OnEnterState -= HandleEnteredRuntimeState;
            _runner = null;
        }
        
        private void HandleEnteredRuntimeState(FsmState state) {
            Log($"Entered runtime state {state}");
            RepopulateView();
        }

        // ---------------- ---------------- Selection ---------------- ----------------

        private void UpdateSelection() {
            var selectedElements = graphElements.Where(e => e.selected).ToList();
            if (selectedElements.Count == 1) {
                Select(selectedElements.First());
                return;
            }
            OnNothingSelected.Invoke();
        }

        private void Select(GraphElement element) {
            Log($"Select element {element}");
            AddToSelection(element);
            switch (element) {
                case TargetStateView view:
                    OnObjectSelected.Invoke(view.transition.targetState);
                    return;
                
                case StateView view:
                    OnObjectSelected.Invoke(view.state);
                    return;
                
                case Edge edge: {
                    if (edge.input.node is TargetStateView targetView && edge.output.node is StateView) {
                        OnObjectSelected.Invoke(targetView.transition);
                    }
                    break;
                }
            }
        }

        // ---------------- ---------------- Mouse events ---------------- ----------------
        
        private void HandleMouseUp(MouseUpEvent evt) {
            UpdateSelection();
        }

        private void HandleClick(ClickEvent evt) {
            TryAbortNewTransitionPending();
        }
        
        private void HandleMouseDownOnEdge(MouseDownEvent evt) {
            UpdateSelection();
        }
        
        // ---------------- ---------------- Node creation ---------------- ----------------
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (editMode != EditMode.Asset) return;
            base.BuildContextualMenu(evt);
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange change) {
            if (_stateMachine == null) return change;
            
            Log("Graph view changed: " + 
                $"need create edges {change.edgesToCreate?.Count}, " +
                $"need remove elements {change.elementsToRemove?.Count}, " +
                $"moved elements {change.movedElements?.Count}");
            
            change.elementsToRemove?.ForEach(e => {
                switch (e) {
                    case StateView stateView:
                        var state = stateView.state;
                        DeleteState(state);
                        break;
                    
                    case TargetStateView targetStateView:
                        var connection = targetStateView.input.connections.ToList().First();
                        if (connection.output.node is StateView sourceStateView) {
                            DeleteTransition(sourceStateView.state, targetStateView.transition);
                        }
                        break;
                    
                    case Edge edge:
                        if (edge.input.node is TargetStateView targetView && edge.output.node is StateView sourceView) {
                            DeleteTransition(sourceView.state, targetView.transition);
                        }
                        break;
                }
            });

            bool hasElementsToRemove = change.elementsToRemove is { Count: > 0 };
            bool hasMovedElements = change.movedElements is { Count: > 0 };
            bool hasEdgesToCreate = change.edgesToCreate is { Count: > 0 };

            if (hasMovedElements || hasElementsToRemove) {
                SaveAsset();
            }

            if (hasEdgesToCreate || hasElementsToRemove) {
                var routine = EditorCoroutines.Delay(RepopulateViewDelaySec, RepopulateView);
                _repopulateViewTask = EditorCoroutines.StartCoroutine(this, routine);
            }
            
            return change;
        }

        private void OpenSearchWindow(Vector2 position, GraphSearchWindow.Filter filter) {
            Log($"Start open search window task with filter {filter}");
            _graphSearchWindow.filter = filter;
            var routine = EditorCoroutines.ScheduleTimesWhile(
                SearchWindowStartDelaySec,
                SearchWindowRetryPeriodSec,
                SearchWindowRetryTimes,
                () => !SearchWindow.Open(new SearchWindowContext(position), _graphSearchWindow)
            );
            _openSearchWindowTask = EditorCoroutines.StartCoroutine(this, routine);
        }

        private void SetNewTransitionPending(FsmState source) {
            Log($"Set new transition pending from state {source}");
            _transitionData.source = source;
            _transitionData.pendingStage = TransitionCreationData.PendingStage.PendingTarget;
        }

        private void TryAbortNewTransitionPending() {
            switch (_transitionData.pendingStage) {
                case TransitionCreationData.PendingStage.None:
                    return;
                
                case TransitionCreationData.PendingStage.PendingTarget:
                    break;
                
                case TransitionCreationData.PendingStage.PendingTransition:
                    ResetNewTransitionData();
                    break;
            }
        }

        private void ResetNewTransitionData() {
            Log("Reset new transition data");
            _transitionData.pendingStage = TransitionCreationData.PendingStage.None;
            _transitionData.source = null;
            _transitionData.target = null;
        }

        // ---------------- ---------------- View operations ---------------- ----------------
        
        private void CreateStateView(FsmState state) {
            bool isInitial = _stateMachine.initialState == state;
            bool isRuntime = editMode == EditMode.RunnerInstance;
            bool isCurrentInRuntime = IsStateCurrentInRuntime(state);
            
            var stateView = new StateView(state, isInitial, isRuntime, isCurrentInRuntime, this) {
                onNodePositionChanged = OnNodePositionChanged, 
                onStateSelectedAsInitial = OnSelectedAsInitial,
                onStateRenameRequest = s => {
                    OpenRenameNodeDialog(s, ChangeStateNameDialogTitle);
                    InvalidateStateInAsset(s);
                    RepopulateView();
                }
            };
            AddElement(stateView);
        }

        private void CreateTargetStateView(FsmTransition transition) {
            bool isCurrentInRuntime = IsStateCurrentInRuntime(transition.targetState) && IsTransitionIsLastInRuntime(transition);
            var stateView = new TargetStateView(transition, isCurrentInRuntime) {
                onTransitionPositionChanged = OnTransitionPositionChanged
            };
            AddElement(stateView);
        }

        private void CreateTransitionView(FsmState source, FsmTransition transition) {
            var sourceView = FindNodeViewByGuid<StateView>(source.Guid);
            var targetView = FindNodeViewByGuid<TargetStateView>(transition.Guid);
            
            var edge = sourceView.output.ConnectTo(targetView.input);
            edge.RegisterCallback<MouseDownEvent>(HandleMouseDownOnEdge);
            
            AddElement(edge);
        }

        private bool IsStateCurrentInRuntime(FsmState state) {
            if (editMode != EditMode.RunnerInstance) return false;
            return state.Guid == _runner.Instance.CurrentState.Guid;
        }
        
        private bool IsTransitionIsLastInRuntime(FsmTransition transition) {
            if (editMode != EditMode.RunnerInstance) return false;
            return transition.Guid == _runner.Instance.LastTransition.Guid;
        }

        
        private void OnTransitionPositionChanged(FsmTransition transition, Vector2 position) {
            Undo.RecordObject(transition, "Fsm (SetStatePosition)");
            Undo.RecordObject(_stateMachine, "Fsm (SetStatePosition)");

            (transition as IStatePosition).Position = position;
        }
        
        private void OnNodePositionChanged(FsmState state, Vector2 position) {
            Undo.RecordObject(state, "Fsm (SetStatePosition)");
            Undo.RecordObject(_stateMachine, "Fsm (SetStatePosition)");

            (state as IStatePosition).Position = position;
        }

        private void OnSelectedAsInitial(FsmState state) {
            if (state.Equals(_stateMachine.initialState)) return;

            Undo.RecordObject(_stateMachine, "Fsm (SelectInitialState)");
            _stateMachine.initialState = state;

            RepopulateView();
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            if (editMode != EditMode.Asset) return;
            
            var source = (edge?.output?.node as StateView)?.state;
            SetNewTransitionPending(source);
            
            var local = OnRequestLocalPosition.Invoke(position);
            OpenSearchWindow(local, GraphSearchWindow.Filter.TargetState);
        }

        public void OnDrop(GraphView graphView, Edge edge) { }

        private static void OpenRenameNodeDialog(Object state, string title) {
            string newName = StringInputDialogEditorWindow.Show(title, state.name);
            if (!newName.IsValidFieldName() || newName == state.name) return;
            state.name = newName;
        }

        private void FreezeGraphElements() {
            graphElements.ForEach(FreezeGraphElement);
        }
        
        private static void FreezeGraphElement(GraphElement element) {
            element.capabilities = 0;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            if (editMode != EditMode.Asset) return new List<Port>();
            return ports
                .Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node)
                .ToList();
        }

        private Edge FindEdge(FsmTransition transition) {
            return edges.First(e => (e.input.node as TargetStateView)?.transition.Guid == transition.Guid);
        }
        
        private T FindNodeViewByGuid<T>(string guid) where T : Node {
            return GetNodeByGuid(guid) as T;
        }
        
        // ---------------- ---------------- Creation and deletion ---------------- ----------------
        
        private void CreateState(Type type, Vector2 position) {
            var worldPosition = OnRequestWorldPosition.Invoke(position);
            var localPosition = contentViewContainer.WorldToLocal(worldPosition);
            
            var state = CreateInstance<FsmState>(type);
            state.name = type.Name;

            Undo.RecordObject(_stateMachine, "Fsm (CreateState)");
            _stateMachine.states.Add(state);
            if (_stateMachine.states.Count == 1) _stateMachine.initialState = state;
            
            Undo.RecordObject(state, "Fsm (CreateStateData)");
            ((IStatePosition) state).Position = localPosition;
            
            AddToAsset(state);
            SaveAsset();
            
            CreateStateView(state);

            _openRenameStateDialogTask = EditorCoroutines.StartCoroutine(this, EditorCoroutines.NextFrame(() => {
                OpenRenameNodeDialog(state, ChangeStateNameDialogTitle);
                InvalidateStateInAsset(state);
                RepopulateView();
                Select(FindNodeViewByGuid<StateView>(state.Guid));
                Log($"Created state {state}");
            }));
        }
        
        private void CreateTargetState(FsmState state, Vector2 position) {
            Log($"Set target state {state}");
            
            _transitionData.target = state;
            _transitionData.pendingStage = TransitionCreationData.PendingStage.PendingTransition;

            OpenSearchWindow(position, GraphSearchWindow.Filter.Transition);
        }

        private void CreateTransition(Type type, Vector2 position) {
            _transitionData.pendingStage = TransitionCreationData.PendingStage.None;
            
            var transition = CreateInstance<FsmTransition>(type);
            transition.targetState = _transitionData.target;
            
            Undo.RecordObject(_transitionData.source, "Fsm (CreateTransition)");
            _transitionData.source.transitions.Add(transition);
            RenameTransition(transition, _transitionData.source);
            
            var worldPosition = OnRequestWorldPosition.Invoke(position);
            var localPosition = contentViewContainer.WorldToLocal(worldPosition);

            ((IStatePosition) transition).Position = localPosition;
            
            AddToAsset(transition);
            ChangeInAsset(_transitionData.source);
            SaveAsset();

            RepopulateView();
            
            var routine = EditorCoroutines.Delay(SelectViewDelaySec, () => Select(FindEdge(transition)));
            _selectTask = EditorCoroutines.StartCoroutine(this, routine);
            
            Log($"Created transition {transition}");
        }

        private void DeleteState(FsmState state) {
            state.transitions.ForEach(t => DeleteTransition(state, t));
            _stateMachine.states.ForEach(s => {
                s.transitions
                    .Where(t => t.targetState == state)
                    .ToList()
                    .ForEach(t => DeleteTransition(s, t));
            });
            
            Undo.RecordObject(_stateMachine, "Fsm (DeleteState)");

            _stateMachine.states.Remove(state);
            RemoveFromAsset(state);
            
            if (_stateMachine.initialState == state) {
                _stateMachine.initialState = _stateMachine.states.Count > 0
                    ? _stateMachine.states.First()
                    : null;
            }
            
            Log($"Deleted state {state}");
        }

        private void DeleteTransition(FsmState source, FsmTransition transition) {
            Undo.RecordObject(_stateMachine, "Fsm (DeleteTransition)");
            Undo.RecordObject(source, "Fsm (DeleteTransition)");
            source.transitions.Remove(transition);
            RemoveFromAsset(transition);
            
            source.transitions.ForEach(t => {
                RenameTransition(t, source);
                ChangeInAsset(t);
            });
            
            Log($"Source state {source}: deleted transition {transition}");
        }

        private static void RenameTransition(FsmTransition transition, FsmState source) {
            int id = source.transitions.IndexOf(transition);
            transition.name = $"{source.name}_to_{transition.targetState.name}_{id}";
        }

        private List<FsmState> GetTransitableStatesForPendingTransition() {
            var source = _transitionData.source;
            return _stateMachine.states.Where(s => s != source).ToList();
        }
        
        // ---------------- ---------------- Save operations ---------------- ----------------

        internal void OnDestroyEditorWindow() {
            Log("On destroy editor window");
            _openSearchWindowTask?.Cancel();
            _openRenameStateDialogTask?.Cancel();
            _repopulateViewTask?.Cancel();
            _selectTask?.Cancel();
        }
        
        internal bool IsAssetDestroyed() {
            return _stateMachine == null;
        }
        
        private void OnUndoRedo() {
            if (_stateMachine == null) return;
            Log("On undo redo");
            InvalidateAsset();
            RepopulateView();
            ClearSelection();
            UpdateSelection();
        }

        // ---------------- ---------------- Asset operations ---------------- ----------------

        private T CreateInstance<T>(Type type) where T : ScriptableObject {
            var obj = ScriptableObject.CreateInstance(type) as T;
            Assert.IsNotNull(obj, $"Type {type} is not a subtype of {nameof(T)}");
            return obj;
        }
        
        private bool CanOpenAsset() {
            if (editMode == EditMode.None) return false;
            return _stateMachine != null && AssetDatabase.CanOpenAssetInEditor(_stateMachine.GetInstanceID());
        }

        private void InvalidateAsset() {
            if (!CanOpenAsset()) return;
            ClearAsset();
            
            _stateMachine.states.ForEach(s => {
                AssetDatabase.AddObjectToAsset(s, _stateMachine);
                s.transitions.ForEach(t => {
                    AssetDatabase.AddObjectToAsset(t, _stateMachine);
                });
            });
            
            SaveAsset();
        }

        private void ClearAsset() {
            if (!CanOpenAsset()) return;
            var path = AssetDatabase.GetAssetPath(_stateMachine);
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var subAsset in subAssets) {
                if (subAsset == null) continue;
                AssetDatabase.RemoveObjectFromAsset(subAsset);
            }
        }
        
        private void InvalidateStateInAsset(FsmState state) {
            ChangeInAsset(state);
            
            state.transitions.ForEach(t => {
                RenameTransition(t, state);
                ChangeInAsset(t);
            });
            
            _stateMachine.states.ForEach(s => s.transitions.ForEach(t => { 
                if (t.targetState == state) {
                    RenameTransition(t, s); 
                    ChangeInAsset(t);
                } 
            }));
            
            SaveAsset();
        }
        
        private void AddToAsset(Object obj) {
            if (!CanOpenAsset()) return;
            AssetDatabase.AddObjectToAsset(obj, _stateMachine);
        }

        private void ChangeInAsset(Object obj) {
            if (!CanOpenAsset()) return;
            AssetDatabase.RemoveObjectFromAsset(obj);
            AssetDatabase.AddObjectToAsset(obj, _stateMachine);
        }
        
        private void RemoveFromAsset(Object obj) {
            if (!CanOpenAsset()) return;
            AssetDatabase.RemoveObjectFromAsset(obj);
        }

        private static void SaveAsset() {
            AssetDatabase.SaveAssets();
        }

        // ---------------- ---------------- Logs ---------------- ----------------

        private static void Log(string message) {
            if (EnableLogs) Debug.Log($"State Machine Editor: {message}");
        }
        
        // ---------------- ---------------- Nested classes ---------------- ----------------
        
        public new class UxmlFactory : UxmlFactory<FsmView, UxmlTraits> { }
        
        internal enum EditMode {
            Asset,
            RunnerInstance,
            None
        }
        
    }

}
