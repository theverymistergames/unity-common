using System;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterAccessRegistry : MonoBehaviour, ICharacterAccessRegistry {

        public static ICharacterAccessRegistry Instance { get; private set; }

        private IActor _actor;

        private void Awake() {
            Instance = this;
        }

        public IActor GetCharacterAccess() {
            return _actor;
        }

        public void Register(IActor actor) {
            _actor = actor;
        }

        public void Unregister(IActor action) {
            _actor = null;
        }
    }

}
