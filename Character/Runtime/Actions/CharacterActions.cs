using System;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActions : ICharacterAction, ICharacterAccessInitializable {

        [SerializeReference] [SubclassSelector] public ICharacterAction[] actions;

        public void Initialize(ICharacterAccess characterAccess) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is ICharacterAccessInitializable action) action.Initialize(characterAccess);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is ICharacterAccessInitializable action) action.DeInitialize();
            }
        }

        public void Apply(object source, ICharacterAccess characterAccess) {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].Apply(source, characterAccess);
            }
        }
    }

}
