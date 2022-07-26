using System;
using MisterGames.Fsm.Core;
using MisterGames.Fsm.Editor.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Fsm.Editor.Views {

    internal sealed class StateView : Node {
        
        internal readonly FsmState state;
        internal readonly Port output;

        internal Action<FsmState> onStateSelectedAsInitial = delegate {  };
        internal Action<FsmState> onStateRenameRequest = delegate {  };
        internal Action<FsmState, Vector2> onNodePositionChanged = delegate {  };

        private readonly bool _isInitial;
        private readonly bool _isCurrentInRuntime;
        private readonly bool _isRuntime;

        public StateView(
            FsmState state,
            bool isInitial,
            bool isRuntime,
            bool isCurrentInRuntime,
            IEdgeConnectorListener connectorListener
        )
            : base(GetUxmlPath()) 
        {
            this.state = state;
            
            _isInitial = isInitial;
            _isRuntime = isRuntime;
            _isCurrentInRuntime = isCurrentInRuntime;

            var titleLabel = this.Q<Label>("title");
            var headerLabel = this.Q<Label>("header");

            titleLabel.text = state.GetType().Name;
            headerLabel.text = state.name;

            var iState = state as IStatePosition;
            viewDataKey = state.Guid;
            style.left = iState.Position.x;
            style.top = iState.Position.y;
            
            output = CreateOutput();
            output.AddManipulator(new EdgeConnector<Edge>(connectorListener));
            
            SetupView();
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("StateView");
            return AssetDatabase.GetAssetPath(asset);
        }

        private void SetupView() {
            AddToClassList("state");
            if (_isInitial) AddToClassList("initial");
            if (_isCurrentInRuntime) AddToClassList("current");
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            var position = new Vector2(newPos.xMin, newPos.yMin);
            onNodePositionChanged.Invoke(state, position);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (!_isRuntime) AddContextualActions(evt);
            evt.StopPropagation();
        }

        private void AddContextualActions(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction(
                "Rename state", 
                _ => onStateRenameRequest.Invoke(state)
            );
            evt.menu.AppendAction(
                "Select as initial state", 
                _ => onStateSelectedAsInitial.Invoke(state),
                _ => _isInitial ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal
            );
        }

        private Port CreateOutput() {
            var port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(FsmTransition));
            port.portName = "";
            outputContainer.Add(port);
            return port;
        }

    }

}