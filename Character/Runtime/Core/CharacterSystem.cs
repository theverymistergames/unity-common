using System;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterSystem : MonoBehaviour {

        [SerializeField] private Actor _heroPrefab;

        public event Action<IActor> OnCharacterInstanceChanged = delegate { }; 
        public static CharacterSystem Instance { get; private set; }

        private IActor _actor;

        private void Awake() {
            Instance = this;
        }

        public IActor GetCharacter(bool spawnIfNotRegistered = false) {
            if (_actor == null && spawnIfNotRegistered) {
                _actor = Instantiate(_heroPrefab);
            }
            
            return _actor;
        }

        public void Register(IActor actor) {
            if (_actor == actor || actor == null) return;
            
            _actor = actor;
            OnCharacterInstanceChanged.Invoke(_actor);
        }

        public void Unregister(IActor actor) {
            if (_actor != actor || _actor == null) return;
            
            _actor = null;
            OnCharacterInstanceChanged.Invoke(null);
        }
    }

}
