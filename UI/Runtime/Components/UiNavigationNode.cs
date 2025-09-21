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
        
        [Header("Inner Navigation")]
        [SerializeField] private UiNavigationMode _mode;
        [SerializeField] private bool _loop = true;
        
        [Header("Outer Navigation")]
        [SerializeField] private UiNavigateFromOuterNodesOptions _navigateFromOuterNodesOptions = 
                UiNavigateFromOuterNodesOptions.SelectClosestElement;
        
        [SerializeField] private UiNavigateToOuterNodesOptions _navigateToOuterNodesOptions = 
            UiNavigateToOuterNodesOptions.Parent | UiNavigateToOuterNodesOptions.Siblings | UiNavigateToOuterNodesOptions.Children;
        
        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }
        public UiNavigateFromOuterNodesOptions NavigateFromOuterNodesOptions => _navigateFromOuterNodesOptions;
        
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
            if (_navigateToOuterNodesOptions == UiNavigateToOuterNodesOptions.None ||
                !Services.TryGet(out IUiNavigationService service)) 
            {
                return;
            }

            var selectables = service.Selectables;
            
            bool allowParent = (_navigateToOuterNodesOptions & UiNavigateToOuterNodesOptions.Parent) == UiNavigateToOuterNodesOptions.Parent;
            bool allowSiblings = (_navigateToOuterNodesOptions & UiNavigateToOuterNodesOptions.Siblings) == UiNavigateToOuterNodesOptions.Siblings;
            bool allowChildren = (_navigateToOuterNodesOptions & UiNavigateToOuterNodesOptions.Children) == UiNavigateToOuterNodesOptions.Children;

            var parentNode = service.GetParentNavigationNode(this);

            var rootTrf = transform;
            var origin = rootTrf.InverseTransformPoint(fromSelectable.transform.position).ToFloat2XY();
            
            float minSqrDistance = -1f;
            Selectable closestSelectable = null;
            
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
                closestSelectable = selectable;
            }

            if (closestSelectable == null) return;

            var nextParentNode = service.GetParentNavigationNode(closestSelectable);
            
            var options = nextParentNode?.CurrentSelected == null
                ? UiNavigateFromOuterNodesOptions.SelectClosestElement
                : nextParentNode.NavigateFromOuterNodesOptions;
            
            var selectTarget = options switch {
                UiNavigateFromOuterNodesOptions.SelectClosestElement => closestSelectable.gameObject,
                UiNavigateFromOuterNodesOptions.SelectHistoryElement => nextParentNode!.CurrentSelected,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            service.SelectGameObject(selectTarget);
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