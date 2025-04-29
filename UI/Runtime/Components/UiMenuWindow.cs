using MisterGames.UI.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiMenuWindow : MonoBehaviour {

        [SerializeField] private Selectable _firstSelected;
        
        private void OnEnable() {
            UIWindowsService.Instance.NotifyOpenedWindow(this, true);
            
            if (_firstSelected != null) EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject); 
        }

        private void OnDisable() {
            UIWindowsService.Instance.NotifyOpenedWindow(this, false);
        }
    }
    
}