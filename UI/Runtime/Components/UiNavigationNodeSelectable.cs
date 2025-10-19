using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class UiNavigationNodeSelectable : MonoBehaviour {
        
        [SerializeField] private Selectable _selectable;
        [SerializeField] private UiNavigationMask _allowNavigate = ~UiNavigationMask.None;
        [SerializeField] private UiNavigationOptions _options;

        private void OnEnable() {
            Services.Get<IUiNavigationService>()?.BindNavigation(_selectable, _allowNavigate, _options);
        }

        private void OnDisable() {
            Services.Get<IUiNavigationService>()?.UnbindNavigation(_selectable);
        }
            
#if UNITY_EDITOR
        private void Reset() {
            _selectable = GetComponent<Selectable>();
            if (_selectable != null && _selectable is Scrollbar) _options |= UiNavigationOptions.Scrollable;
        }
#endif
    }
    
}