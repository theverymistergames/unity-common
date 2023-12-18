using System;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterAccessRegistry : MonoBehaviour, ICharacterAccessRegistry {

        public static ICharacterAccessRegistry Instance { get; private set; }

        public event Action<CharacterAccess> OnRegistered = delegate {  };
        public event Action<CharacterAccess> OnUnregistered = delegate {  };

        private CharacterAccess _characterAccess;

        private void Awake() {
            Instance = this;
        }

        public CharacterAccess GetCharacterAccess() {
            return _characterAccess;
        }

        public void Register(CharacterAccess characterAccess) {
            _characterAccess = characterAccess;
            OnRegistered.Invoke(characterAccess);
        }

        public void Unregister(CharacterAccess characterAccess) {
            _characterAccess = null;
            OnUnregistered.Invoke(characterAccess);
        }
    }

}
