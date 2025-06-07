using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Components {
    
    public sealed class ActionOnEvent : MonoBehaviour, IActorComponent, IEventListener {

        [Header("Event")]
        [SerializeField] private EventReference _eventReference;
        [SerializeField] private bool _checkOnEnable = true;
        [SerializeField] private bool _unsubscribeAfterMatch;
        
        [Header("Comparison")]
        [SerializeField] private CompareMode _compareMode;
        
        [Header("For equal or not equal comparisons")]
        [SerializeField] private int[] _values;
        
        [Header("For other comparisons")]
        [SerializeField] private int _value = 1;

        [Header("Action")]
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _eventReference.Subscribe(this);
        }

        private void OnDestroy() {
            _eventReference.Unsubscribe(this);
        }

        private void OnEnable() {
            if (_checkOnEnable) OnEventRaised(_eventReference);
        }

        public void OnEventRaised(EventReference e) {
            bool isMatch = IsMatch(e.GetCount());
            if (!isMatch) return;

            _action?.Apply(_actor, destroyCancellationToken).Forget();
            
            if (_unsubscribeAfterMatch) _eventReference.Unsubscribe(this);
        }

        private bool IsMatch(int raiseCount) {
            if (_compareMode is not (CompareMode.Equal or CompareMode.NotEqual)) {
                return _compareMode.IsMatch(raiseCount, _value);
            }
            
            for (int i = 0; i < _values.Length; i++) {
                if (_compareMode.IsMatch(raiseCount, _values[i])) return true;
            }
            
            return false;
        }
    }
    
}