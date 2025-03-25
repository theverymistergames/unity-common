using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class ActionOnDeath : MonoBehaviour, IActorComponent {
        
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _enableCts;
        private IActor _actor;
        private HealthBehaviour _health;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _health = actor.GetComponent<HealthBehaviour>();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _health.OnDeath += OnDeath;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _health.OnDeath -= OnDeath;
        }

        private void OnDeath() {
            _action?.Apply(_actor, _enableCts.Token).Forget();
        }
    }
    
}