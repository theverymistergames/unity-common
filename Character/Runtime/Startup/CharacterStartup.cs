using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Startup {

    public class CharacterStartup : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CharacterActionsAsset[] _startupActions;

        private async void Start() {
            for (int i = 0; i < _startupActions.Length; i++) {
                await _startupActions[i].Apply(this, _characterAccess);
            }
        }
    }

}
