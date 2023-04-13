using MisterGames.Character.Access;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [CreateAssetMenu(fileName = nameof(CharacterChangeSet), menuName = "MisterGames/Character/" + nameof(CharacterChangeSet))]
    public class CharacterChangeSet : ScriptableObject, ICharacterAction {

        [SerializeReference] [SubclassSelector] private ICharacterAction[] _actions;

        public void Apply(object source, ICharacterAccess characterAccess) {
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].Apply(source, characterAccess);
            }
        }
    }

}
