using System;
using MisterGames.Fsm.Core;
using MisterGames.Fsm.Editor.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Fsm.Editor.Views {

    internal sealed class TargetStateView : Node {
        
        internal readonly FsmTransition transition;
        internal readonly Port input;

        internal Action<FsmTransition, Vector2> onTransitionPositionChanged = delegate {  };
        private readonly bool _isCurrentInRuntime;

        public TargetStateView(
            FsmTransition transition,
            bool isCurrentInRuntime
        ) 
            : base(GetUxmlPath()) 
        {
            this.transition = transition;
            _isCurrentInRuntime = isCurrentInRuntime;

            var headerLabel = this.Q<Label>("header");
            headerLabel.text = transition.targetState.name;

            var iPosition = transition as IStatePosition;
            viewDataKey = transition.Guid;
            style.left = iPosition.Position.x;
            style.top = iPosition.Position.y;
            
            input = CreateInput();
            
            SetupView();
        }

        private static string GetUxmlPath() {
            var asset = Resources.Load<VisualTreeAsset>("TargetStateView");
            return AssetDatabase.GetAssetPath(asset);
        }
        
        private void SetupView() {
            if (_isCurrentInRuntime) AddToClassList("current");
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            var position = new Vector2(newPos.xMin, newPos.yMin);
            onTransitionPositionChanged.Invoke(transition, position);
        }

        private Port CreateInput() {
            var port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(FsmTransition));
            port.portName = "";
            inputContainer.Add(port);
            port.capabilities = 0;
            return port;
        }

    }

}