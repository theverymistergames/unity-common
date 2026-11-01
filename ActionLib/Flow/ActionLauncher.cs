using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    public sealed class ActionLauncher : MonoBehaviour, IActorComponent {
        
        [TextAreaExtended(expandable: true, showEditButtons: true)] 
        [SerializeField] private string _comment;
        [SerializeField] private LaunchMode _launchMode;
        [SerializeField] private NextActionMode _cancelOnNextAction;
        [SerializeField] private bool _cancelOnDisable;
        [SerializeField] private bool _useCharacterAsActor;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _actionCts;
        private IActor _actor;
        private bool _isStarted;
        private byte _lastActionId;
        private byte _requestedActionId;

        private enum NextActionMode {
            StartNextWithoutCancelingPrevious,
            CancelPreviousAndStartNext,
            DontStartIfPreviousRunning,
        }
        
        private enum LaunchMode {
            OnAwake,
            OnDestroy,
            OnStart,
            OnEnable,
            OnDisable,
            OnDisableIfStarted,
            Manual,
        }

        public async UniTask Launch(CancellationToken cancellationToken) {
            switch (_cancelOnNextAction) {
                case NextActionMode.StartNextWithoutCancelingPrevious:
                    if (_actionCts == null) AsyncExt.RecreateCts(ref _actionCts);
                    cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _actionCts.Token).Token;
                    break;
                
                case NextActionMode.CancelPreviousAndStartNext:
                    AsyncExt.RecreateCts(ref _actionCts);
                    cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _actionCts.Token).Token;
                    break;
                
                case NextActionMode.DontStartIfPreviousRunning:
                    if (_lastActionId != _requestedActionId) return;
                    if (_actionCts == null) AsyncExt.RecreateCts(ref _actionCts);
                    cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _actionCts.Token).Token;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (_action == null) return;

            _lastActionId.IncrementUncheckedRef();
            _requestedActionId = _lastActionId.ReturnThenIncrementUncheckedRef();
            
            await _action.Apply(GetActor(), cancellationToken);

            _requestedActionId = _lastActionId.IncrementUncheckedRef();
        }

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (_launchMode == LaunchMode.OnAwake) Launch(GetCancellationToken()).Forget();
        }

        private void OnDestroy() {
            if (_launchMode == LaunchMode.OnDestroy) Launch(cancellationToken: CancellationToken.None).Forget();
        }

        private void Start() {
            _isStarted = true;
            if (_launchMode == LaunchMode.OnStart) Launch(GetCancellationToken()).Forget();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (_launchMode == LaunchMode.OnEnable) Launch(GetCancellationToken()).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);

            if (_cancelOnDisable) {
                _requestedActionId = _lastActionId.IncrementUncheckedRef();
            }
            
            if (_launchMode == LaunchMode.OnDisable ||
                _isStarted && _launchMode == LaunchMode.OnDisableIfStarted
            ) {
                Launch(destroyCancellationToken).Forget();
            }
        }

        private IActor GetActor() {
            return _useCharacterAsActor ? CharacterSpawner.Instance.GetCharacter() : _actor;
        }

        private CancellationToken GetCancellationToken() {
            return _cancelOnDisable ? _enableCts.Token : destroyCancellationToken;
        }

#if UNITY_EDITOR
        [Button]
        private void LaunchManual() {
            Launch(destroyCancellationToken).Forget();
        }
#endif
    }
    
}