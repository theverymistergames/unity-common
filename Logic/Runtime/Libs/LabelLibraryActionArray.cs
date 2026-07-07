using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Common.Lists;
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
            public LabelValue<IActorAction> label;
            public LabelValue<IActorAction>[] disallowLaunchAfter;
            [SerializeReference] [SubclassSelector] public IActorAction action;
        }

        private sealed class ActionWrapper : IActorAction {

            private readonly int _index;
            private readonly Func<int, IActor, CancellationToken, UniTask> _apply;

            public ActionWrapper(
                int index, 
                Func<int, IActor, CancellationToken, UniTask> apply) 
            {
                _index = index;
                _apply = apply;
            }

            public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
                return _apply.Invoke(_index, context, cancellationToken);
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
                actionData.label.TrySetData(new ActionWrapper(i, OnApply));
            }
        }

        private UniTask OnApply(int index, IActor context, CancellationToken cancellationToken) {
            ref var actionData = ref _actions[index];
            
            if (_disallowInvokeSameActionSequentially && _lastInvokedActionLabel == actionData.label ||
                actionData.disallowLaunchAfter.Contains(_lastInvokedActionLabel)) 
            {
                return UniTask.CompletedTask;
            }
            
            _lastInvokedActionLabel = actionData.label;
            return actionData.action?.Apply(context, CreateCancellationToken(cancellationToken)) ?? UniTask.CompletedTask;
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