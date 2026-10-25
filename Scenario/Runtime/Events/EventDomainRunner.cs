using UnityEngine;

namespace MisterGames.Scenario.Events {
    
    [DefaultExecutionOrder(-99_999)]
    public sealed class EventDomainRunner : MonoBehaviour, IEventDomainRunner {
        
        [SerializeField] private EventDomain _eventDomain;

        private void OnEnable() {
            EventBus.Main.RegisterEventDomainRunner(this);
        }

        private void OnDisable() {
            EventBus.Main.UnregisterEventDomainRunner(this);
        }

        public EventDomain GetEventDomain() {
            return _eventDomain;
        }
    }
    
}