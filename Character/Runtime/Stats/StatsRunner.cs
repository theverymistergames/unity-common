using MisterGames.Actors;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Stats {
    
    public sealed class StatsRunner : MonoBehaviour, IActorComponent {
        
        public IStatSystem StatSystem => _statSystem;
        private readonly StatSystem _statSystem;

        void IActorComponent.OnAwake(IActor actor) {
            _statSystem.Bind(actor);
            PlayerLoopStage.Update.Subscribe(_statSystem);
        }

        void IActorComponent.OnDestroyed(IActor actor) {
            PlayerLoopStage.Update.Unsubscribe(_statSystem);
            _statSystem.Unbind();
            _statSystem.Clear();
        }
    }
    
}