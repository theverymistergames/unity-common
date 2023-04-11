using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using UnityEngine;

namespace MisterGames.Character.Core2.Startup {

    public class CharacterStartup : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CharacterChangeSet[] _startupActions;

        private void Start() {
            for (int i = 0; i < _startupActions.Length; i++) {
                _startupActions[i].Apply(this, _characterAccess);
            }
        }
    }

}
