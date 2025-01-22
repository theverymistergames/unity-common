using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    public sealed class ActionLauncher : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _useCharacterAsActor;
        [SerializeField] private bool _cancelOnNextAction;
        [SerializeField] private bool _cancelOnDisable;
        [SerializeField] private LaunchMode _launchMode;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _actionCts;
        private IActor _actor;
        private bool _isStarted;
        
        private enum LaunchMode {
            OnAwake,
            OnDestroy,
            OnStart,
            OnEnable,
            OnDisable,
            OnDisableIfStarted,
            Manual,
        }

        public UniTask Launch(CancellationToken cancellationToken) {
            if (_cancelOnNextAction) {
                AsyncExt.RecreateCts(ref _actionCts);
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _actionCts.Token).Token;
            }

            return _action?.Apply(GetActor(), cancellationToken) ?? default;
        }

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (_launchMode == LaunchMode.OnAwake) Launch(GetCancellationToken()).Forget();
        }

        private void OnDestroy() {
            if (_launchMode == LaunchMode.OnDestroy) Launch(cancellationToken: default).Forget();
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
            
            if (_launchMode == LaunchMode.OnDisable ||
                _isStarted && _launchMode == LaunchMode.OnDisableIfStarted
            ) {
                Launch(destroyCancellationToken).Forget();
            }
        }

        private IActor GetActor() {
            return _useCharacterAsActor ? CharacterSystem.Instance.GetCharacter() : _actor;
        }

        private CancellationToken GetCancellationToken() {
            return _cancelOnDisable ? _enableCts.Token : destroyCancellationToken;
        }
    }
    
}