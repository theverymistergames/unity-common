using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Components {
    
    public sealed class EnableGameObjectsOnEvent : MonoBehaviour, IEventListener {

        [SerializeField] private GameObject[] _gameObjects;
        [SerializeField] private EventReference _eventReference;
        [SerializeField] private CompareMode _compareMode;
        
        [VisibleIf(nameof(_compareMode), 1, CompareMode.LessOrEqual)]
        [SerializeField] private int[] _values;
        
        [VisibleIf(nameof(_compareMode), 2, CompareMode.GreaterOrEqual)]
        [SerializeField] private int _value = 1;

        private void Reset() {
            _gameObjects = new[] { gameObject };
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
            bool isMatch = IsMatch(e.GetRaiseCount());

            for (int i = 0; i < _gameObjects.Length; i++) {
                _gameObjects[i].SetActive(isMatch);
            }
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