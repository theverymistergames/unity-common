using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryActionArray : MonoBehaviour {

        [SerializeField] private ActionMode _behaviourOnInvoke;
        [SerializeField] private bool _disallowInvokeSameActionSequentially;
        [SerializeField] private ActionData[] _actions;

        private enum ActionMode {
            InvokeNewAction,
            CancelPreviousAction,
        }
        
        [Serializable]
        private struct ActionData {
            [SerializeField] public LabelValue<IActorAction> label;
            [SerializeReference] [SubclassSelector] public IActorAction action;
        }

        private sealed class ActionWrapper : IActorAction {
            
            private readonly LabelValue<IActorAction> _label;
            private readonly IActorAction _action;
            private readonly Func<LabelValue<IActorAction>, IActorAction, IActor, CancellationToken, UniTask> _apply;

            public ActionWrapper(
                LabelValue<IActorAction> label, 
                IActorAction action, 
                Func<LabelValue<IActorAction>, IActorAction, IActor, CancellationToken, UniTask> apply) 
            {
                _label = label;
                _action = action;
                _apply = apply;
            }

            public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
                return _apply.Invoke(_label, _action, context, cancellationToken);
            }
        }

        private CancellationTokenSource _cts;
        private LabelValue<IActorAction> _lastInvokedActionLabel;

        private void Awake() {
            CreateActionsWrappers();
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
            
            for (int i = 0; i < _actions.Length; i++) {
                ref var actionData = ref _actions[i];
                actionData.label.ClearData();
            }
        }

        private void CreateActionsWrappers() {
            for (int i = 0; i < _actions?.Length; i++) {
                ref var actionData = ref _actions[i];
                actionData.label.TrySetData(new ActionWrapper(actionData.label, actionData.action, OnApply));
            }
        }

        private UniTask OnApply(LabelValue<IActorAction> label, IActorAction action, IActor context, CancellationToken cancellationToken) {
            if (_disallowInvokeSameActionSequentially && _lastInvokedActionLabel == label) {
                return UniTask.CompletedTask;
            }
            
            _lastInvokedActionLabel = label;
            
            return action?.Apply(context, CreateCancellationToken(cancellationToken)) ?? UniTask.CompletedTask;
        }
        
        private CancellationToken CreateCancellationToken(CancellationToken defaultCancellationToken) {
            switch (_behaviourOnInvoke) {
                case ActionMode.InvokeNewAction:
                    return defaultCancellationToken;
                
                case ActionMode.CancelPreviousAction:
                    AsyncExt.RecreateCts(ref _cts);
                    return CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, defaultCancellationToken).Token;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private int _actionIndex;

        private void OnValidate() {
            _actionIndex = Mathf.Clamp(_actionIndex, 0, (_actions?.Length ?? 0) - 1);

            if (Application.isPlaying) {
                AsyncExt.DisposeCts(ref _cts);
                CreateActionsWrappers();
            }
        }

        [Button(mode: ButtonAttribute.Mode.Runtime)]
        private void LaunchManually() {
            if (_actions == null || _actionIndex < 0 || _actionIndex >= _actions.Length) return;
            
            ref var actionData = ref _actions[_actionIndex];
            actionData.label.GetData()?.Apply(null).Forget();
        }
#endif
    }
    
}