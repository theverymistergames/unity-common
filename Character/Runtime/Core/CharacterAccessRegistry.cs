using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterAccessRegistry : MonoBehaviour {

        [SerializeField] private Actor _heroPrefab;

        public static CharacterAccessRegistry Instance { get; private set; }

        private IActor _actor;

        private void Awake() {
            Instance = this;
        }

        public IActor GetCharacterAccess(bool spawnIfNotRegistered = false) {
            if (_actor == null && spawnIfNotRegistered) {
                _actor = Instantiate(_heroPrefab);
            }
            
            return _actor;
        }

        public void Register(IActor actor) {
            _actor = actor;
        }

        public void Unregister(IActor actor) {
            _actor = null;
        }
    }

}
