using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Components {
    
    public sealed class EnableGameObjectOnEvent : MonoBehaviour, IEventListener {

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private EventReference _eventReference;
        [SerializeField] private CompareMode _compareMode;
        
        [VisibleIf(nameof(_compareMode), 1, CompareMode.LessOrEqual)]
        [SerializeField] private int[] _values;
        
        [VisibleIf(nameof(_compareMode), 2, CompareMode.GreaterOrEqual)]
        [SerializeField] private int _value = 1;

        private void Reset() {
            _gameObject = gameObject;
        }

        private void Awake() {
            _eventReference.Subscribe(this);
        }

        private void OnDestroy() {
            _eventReference.Unsubscribe(this);
        }

        private void OnEnable() {
            OnEventRaised(_eventReference);
        }

        public void OnEventRaised(EventReference e) {
            _gameObject.SetActive(IsMatch(e.GetRaiseCount()));
        }

        private bool IsMatch(int raiseCount) {
            if (_compareMode is not (CompareMode.Equal or CompareMode.NotEqual)) {
                return _compareMode.IsMatch(_value, raiseCount);
            }
            
            for (int i = 0; i < _values.Length; i++) {
                if (_compareMode.IsMatch(_values[i], raiseCount)) return true;
            }
            
            return false;
        }
    }
    
}