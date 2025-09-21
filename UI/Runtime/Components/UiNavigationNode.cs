using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    public sealed class UiNavigationNode : MonoBehaviour, IUiNavigationNode {
        
        [SerializeField] private UiNavigationMode _mode;
        [SerializeField] private bool _loop = true;
        [SerializeField] private UiNavigationOuterOptions _outerNavigationOptions = UiNavigationOuterOptions.Parent | 
                                                                                    UiNavigationOuterOptions.Siblings | 
                                                                                    UiNavigationOuterOptions.Children;
        
        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }
        
        private readonly UiNavigationNodeHelper _helper = new();
        
        private IUiWindow _window;
        
        private void Awake() {
            _window = GetComponent<IUiWindow>();
        }

        private void OnDestroy() {
            _helper.Dispose();
        }

        private void OnEnable() {
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;
                
                if (_window == null || _window.State == UiWindowState.Opened) {
                    navigationService.BindNavigation(this);
                }
            }
        }

        private void OnDisable() {
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
                
                navigationService.UnbindNavigation(this);
            }
        }

        public void Bind(Selectable selectable) {
            _helper.Bind(selectable);

            if (Services.TryGet(out IUiNavigationService service)) {
                OnSelectedGameObjectChanged(service.SelectedGameObject, service.SelectedGameObjectWindow);   
            }
        }

        public void Unbind(Selectable selectable) {
            _helper.Unbind(selectable);
        }

        public void UpdateNavigation() {
            _helper.UpdateNavigation(transform, _mode, _loop);
        }

        public void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction) {
            if (_outerNavigationOptions == UiNavigationOuterOptions.None ||
                !Services.TryGet(out IUiNavigationService service)) 
            {
                return;
            }

            var selectables = service.Selectables;
            
            bool allowParent = (_outerNavigationOptions & UiNavigationOuterOptions.Parent) == UiNavigationOuterOptions.Parent;
            bool allowSiblings = (_outerNavigationOptions & UiNavigationOuterOptions.Siblings) == UiNavigationOuterOptions.Siblings;
            bool allowChildren = (_outerNavigationOptions & UiNavigationOuterOptions.Children) == UiNavigationOuterOptions.Children;

            var parentNode = service.GetParentNavigationNode(this);

            var rootTrf = transform;
            var origin = rootTrf.InverseTransformPoint(fromSelectable.transform.position).ToFloat2XY();
            
            float minSqrDistance = -1f;
            Selectable nextSelectable = null;
            
            foreach (var selectable in selectables) {
                if (_helper.IsBound(selectable.gameObject) || 
                    service.GetParentNavigationNode(selectable) is not { } node || 
                    !allowParent && node != parentNode || 
                    !allowSiblings && !service.IsChildNode(node, parentNode, direct: true) ||
                    !allowChildren && !service.IsChildNode(node, this, direct: true))
                {
                    continue;
                }
            
                var pos = rootTrf.InverseTransformPoint(selectable.transform.position).ToFloat2XY();
                if (!pos.IsInDirection(origin, direction)) continue;
                
                float sqrDistance = math.distancesq(pos, origin);
                if (minSqrDistance >= 0f && sqrDistance > minSqrDistance) continue;
                
                minSqrDistance = sqrDistance;
                nextSelectable = selectable;
            }
            
            if (nextSelectable != null) service.SelectGameObject(nextSelectable.gameObject);
        }
        
        private void OnWindowsHierarchyChanged() {
            if (_window == null || !Services.TryGet(out IUiNavigationService service)) return;

            switch (_window.State) {
                case UiWindowState.Closed:
                    service.UnbindNavigation(this);
                    break;
                
                case UiWindowState.Opened:
                    service.BindNavigation(this);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSelectedGameObjectChanged(GameObject selected, IUiWindow parent) {
            if (selected != null && _helper.IsBound(selected)) {
                CurrentSelected = selected;
            }
        }
    }
    
}