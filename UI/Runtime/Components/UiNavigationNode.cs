using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
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
        [SerializeField] private Vector2 _cell = new(400f, 50f);
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
        private CancellationTokenSource _enableCts;
        
        private IUiWindow _window;
        
        private void Awake() {
            _window = GetComponent<IUiWindow>();
        }

        private void OnDestroy() {
            _helper.Dispose();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;
                
                if (_window == null || _window.State == UiWindowState.Opened) {
                    navigationService.BindNavigation(this);
                }
            }
            
            UpdateNavigation();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
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
            
            DebugExt.DrawSphere(selectable.transform.position, 0.013f, RandomExtensions.GetRandomColor(), duration: 3f);
            if (Services.TryGet(out IUiNavigationService service)) {
                OnSelectedGameObjectChanged(service.SelectedGameObject, service.SelectedGameObjectWindow);   
            }
        }

        public void Unbind(Selectable selectable) {
            _helper.Unbind(selectable);
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)]
#endif
        public void UpdateNavigation() {
            UpdateNavigationNextFrame(_enableCts?.Token ?? default).Forget();
        }

        private async UniTask UpdateNavigationNextFrame(CancellationToken cancellationToken) {
            _helper.UpdateNavigation(transform, _mode, _loop, _cell);
            
            // The position of the selectable during enabling layout groups maybe inconsistent
            // (all selectables in the layout group share the same selectable.transform.position), 
            // so to avoid setting incorrect navigation lets update it two frames later.
            await UniTask.Yield();
            if (cancellationToken.IsCancellationRequested) return;
            
            _helper.UpdateNavigation(transform, _mode, _loop, _cell);
            
            await UniTask.Yield();
            if (cancellationToken.IsCancellationRequested) return;
            
            _helper.UpdateNavigation(transform, _mode, _loop, _cell);
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
                if (!pos.IsInDirection(origin, direction, _cell)) continue;
                
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