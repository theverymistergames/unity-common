using MisterGames.Common.Service;
using MisterGames.UI.Service;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiMenuWindow : MonoBehaviour {

        [SerializeField] private Selectable _firstSelected;
        
        private void OnEnable() {
            Services.Get<IUIWindowService>()?.NotifyOpenedWindow(this, true);
            
            if (_firstSelected != null) EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject); 
        }

        private void OnDisable() {
            Services.Get<IUIWindowService>()?.NotifyOpenedWindow(this, false);
        }
    }
    
}