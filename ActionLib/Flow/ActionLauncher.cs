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
        [SerializeField] private LaunchMode _launchMode;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

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
            if (_launchMode == LaunchMode.OnAwake) Launch(destroyCancellationToken).Forget();
        }

        private void OnDestroy() {
            if (_launchMode == LaunchMode.OnDestroy) Launch(cancellationToken: default).Forget();
        }

        private void Start() {
            _isStarted = true;
            if (_launchMode == LaunchMode.OnStart) Launch(destroyCancellationToken).Forget();
        }

        private void OnEnable() {
            if (_launchMode == LaunchMode.OnEnable) Launch(destroyCancellationToken).Forget();
        }

        private void OnDisable() {
            if (_launchMode == LaunchMode.OnDisable ||
                _isStarted && _launchMode == LaunchMode.OnDisableIfStarted
            ) {
                Launch(destroyCancellationToken).Forget();
            }
        }

        private IActor GetActor() {
            return _useCharacterAsActor ? CharacterSystem.Instance.GetCharacter() : _actor;
        }
    }
    
}