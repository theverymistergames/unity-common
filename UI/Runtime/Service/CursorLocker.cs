using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.UiServices {
    
    public sealed class CursorLocker : MonoBehaviour {

        private void OnEnable() {
			Services.Get<ICursorService>()?.BlockCursor(this, true);            
        }

        private void OnDisable() { 
			Services.Get<ICursorService>()?.BlockCursor(this, false);
		}
    }
    
}