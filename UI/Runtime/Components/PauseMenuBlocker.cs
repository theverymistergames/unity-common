using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class PauseMenuBlocker : MonoBehaviour {

        private void OnEnable() {
            Services.Get<IUiNavigationService>()?.BlockExitToPause(this);
        }

        private void OnDisable() {
            Services.Get<IUiNavigationService>()?.UnblockExitToPause(this);
        }
    }
    
}