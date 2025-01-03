using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    [RequireComponent(typeof(PoolElement))]
    public sealed class PoolElementActions : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _useCharacterAsActor;
        [SerializeField] private bool _cancelOnNextAction;
        [SerializeReference] [SubclassSelector] private IActorAction _actionTake;
        [SerializeReference] [SubclassSelector] private IActorAction _actionRelease;

        private CancellationTokenSource _actionCts;
        private CancellationToken _destroyToken;
        private IActor _actor;
        private PoolElement _poolElement;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _destroyToken = destroyCancellationToken;
            _poolElement = GetComponent<PoolElement>();
            
            _poolElement.OnTake += OnTake;
            _poolElement.OnRelease += OnRelease;
        }

        private void OnDestroy() {
            _poolElement.OnTake -= OnTake;
            _poolElement.OnRelease -= OnRelease;
        }

        private void OnTake() {
            Launch(_actionTake, _destroyToken).Forget();
        }

        private void OnRelease() {
            Launch(_actionRelease, _destroyToken).Forget();
        }

        private UniTask Launch(IActorAction action, CancellationToken cancellationToken) {
            if (_cancelOnNextAction) {
                AsyncExt.RecreateCts(ref _actionCts);
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _actionCts.Token).Token;
            }

            return action?.Apply(GetActor(), cancellationToken) ?? default;
        }

        private IActor GetActor() {
            return _useCharacterAsActor ? CharacterSystem.Instance.GetCharacter() : _actor;
        }
    }
    
}