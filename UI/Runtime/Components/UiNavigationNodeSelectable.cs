using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class UiNavigationNodeSelectable : MonoBehaviour {
        
        [SerializeField] private Selectable _selectable;

        private void OnEnable() {
            Services.Get<IUiNavigationService>()?.BindNavigation(_selectable);
        }

        private void OnDisable() {
            Services.Get<IUiNavigationService>()?.UnbindNavigation(_selectable);
        }
            
#if UNITY_EDITOR
        private void Reset() {
            _selectable = GetComponent<Selectable>();
        }
#endif
    }
    
}