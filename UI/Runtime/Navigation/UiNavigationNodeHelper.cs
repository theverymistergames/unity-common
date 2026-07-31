using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationNodeHelper : IDisposable {

        private readonly Dictionary<int, Selectable> _gameObjectIdToSelectableMap = new();
        private readonly Dictionary<int, (UiNavigationMask mask, UiNavigationOptions options)> _gameObjectIdToDataMap = new();
        private byte _updateNavigationId;
        
        public void Dispose() {
            _gameObjectIdToSelectableMap.Clear();
            _gameObjectIdToDataMap.Clear();
        }

        public void Bind(
            Selectable selectable,
            UiNavigationMask mask = ~UiNavigationMask.None,
            UiNavigationOptions options = UiNavigationOptions.None) 
        {
            int hash = selectable.gameObject.GetHashCode();
            
            _gameObjectIdToSelectableMap[hash] = selectable;
            _gameObjectIdToDataMap[hash] = (mask, options);
        }

        public void Unbind(Selectable selectable) {
            int hash = selectable.gameObject.GetHashCode();
            
            _gameObjectIdToSelectableMap.Remove(hash);
            _gameObjectIdToDataMap.Remove(hash);
        }

        public bool IsBound(GameObject gameObject) {
            return _gameObjectIdToSelectableMap.ContainsKey(gameObject.GetHashCode());
        }
        
        public void NavigateOut(
            IUiNavigationNode node,
            Selectable fromSelectable,
            UiNavigationDirection direction,
            UiNavigateToOuterNodesOptions options) 
        {
            if (options == UiNavigateToOuterNodesOptions.None ||
                !Services.TryGet(out IUiNavigationService service)) 
            {
                return;
            }
            
            bool allowParent = (options & UiNavigateToOuterNodesOptions.Parent) == UiNavigateToOuterNodesOptions.Parent;
            bool allowSiblings = (options & UiNavigateToOuterNodesOptions.Siblings) == UiNavigateToOuterNodesOptions.Siblings;
            bool allowChildren = (options & UiNavigateToOuterNodesOptions.Children) == UiNavigateToOuterNodesOptions.Children;

            var parentNode = service.GetParentNavigationNode(node);
            var root = node.GameObject.transform;
            var origin = root.InverseTransformPoint(fromSelectable.transform.position).ToFloat2XY();

            var selectables = service.Selectables;
            Selectable closestSelectable = null;
            float minDistance = -1f;

            foreach (var selectable in selectables) {
                if (IsBound(selectable.gameObject) || 
                    
                    service.GetSelectableOptions(selectable) is var opt && 
                    ((opt & UiNavigationOptions.DisallowAnyIncomingNavigation) != 0 || 
                     (opt & UiNavigationOptions.DisallowIncomingNavigationFromOuterNodes) != 0) ||
                      
                    service.GetParentNavigationNode(selectable) is not { } p || 
                    !allowParent && p == parentNode || 
                    !allowSiblings && service.IsChildNode(p, parentNode, direct: false) ||
                    !allowChildren && service.IsChildNode(p, node, direct: false))
                {
                    continue;
                }
            
                var pos = root.InverseTransformPoint(selectable.transform.position).ToFloat2XY();

                if (!pos.IsInDirection(origin, direction)) {
                    continue;
                }

                float distance = (pos - origin).Project(direction).Abs();
                
                if (minDistance >= 0f && distance > minDistance) {
                    continue;
                }
                
                minDistance = distance;
                closestSelectable = selectable;
            }
            
            if (closestSelectable == null) return;
            
            var nextParentNode = service.GetParentNavigationNode(closestSelectable);
            
            var nextOptions = nextParentNode?.CurrentSelected == null
                ? UiNavigateFromOuterNodesOptions.SelectClosestElement
                : nextParentNode.IncomeOuterNavigation;
            
            var selectTarget = nextOptions switch {
                UiNavigateFromOuterNodesOptions.SelectClosestElement => closestSelectable,
                UiNavigateFromOuterNodesOptions.SelectHistoryElement => nextParentNode!.CurrentSelected,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            service.NavigateOutTo(selectTarget, direction);
        }
        
        public async UniTask UpdateNavigationNextFrame(
            Transform rootTrf,
            UiNavigationMode mode,
            UiNavigationLoop loop,
            Vector2 cell,
            CancellationToken cancellationToken) 
        {
            byte id = _updateNavigationId.IncrementUncheckedRef();
            UpdateNavigation(rootTrf, mode, loop, cell);
            
            // The position of the selectable during enabling layout groups maybe inconsistent
            // (all selectables in the layout group share the same selectable.transform.position), 
            // so to avoid setting incorrect navigation lets update it two frames later.
            await UniTask.Yield();
            await UniTask.Yield();
            if (id != _updateNavigationId || cancellationToken.IsCancellationRequested) return;

            UpdateNavigation(rootTrf, mode, loop, cell);
        }

        private void UpdateNavigation(Transform rootTrf, UiNavigationMode mode, UiNavigationLoop loop, Vector2 cellSize) {
            var selectablesArray = new NativeArray<SelectableData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
            var neighborsArray = new NativeArray<SelectableNeighborsData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
             
            int count = 0;

            foreach ((int id, var selectable) in _gameObjectIdToSelectableMap) {
                var data = _gameObjectIdToDataMap.GetValueOrDefault(id);
                selectablesArray[count++] = new SelectableData(
                    id,
                    rootTrf.InverseTransformPoint(selectable.transform.position), 
                    data.mask,
                    data.options
                );
            }

            var job = new GetSelectableNeighborsJob {
                selectablesArray = selectablesArray,
                mode = mode,
                loop = loop,
                cellSize = cellSize,
                neighborsArray = neighborsArray,
            };
            
            job.Schedule(count, JobExt.BatchFor(count)).Complete();
            
            for (int i = 0; i < count; i++) {
                var data = selectablesArray[i];
                var neighborsData = neighborsArray[i];

                var selectable = _gameObjectIdToSelectableMap[data.id];
                
                var navigation = selectable.navigation;
                navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;

                navigation.selectOnUp = _gameObjectIdToSelectableMap.GetValueOrDefault(neighborsData.upId);
                navigation.selectOnDown = _gameObjectIdToSelectableMap.GetValueOrDefault(neighborsData.downId);
                navigation.selectOnLeft = _gameObjectIdToSelectableMap.GetValueOrDefault(neighborsData.leftId);
                navigation.selectOnRight = _gameObjectIdToSelectableMap.GetValueOrDefault(neighborsData.rightId);

                selectable.navigation = navigation;
            }
            
            selectablesArray.Dispose();
            neighborsArray.Dispose();
        }

        private readonly struct SelectableData {
            
            public readonly int id;
            public readonly float2 position;
            public readonly UiNavigationMask mask;
            public readonly UiNavigationOptions options;

            public SelectableData(int id, float3 position, UiNavigationMask mask, UiNavigationOptions options) {
                this.id = id;
                this.position = math.float2(position.x, position.y);
                this.mask = mask;
                this.options = options;
            }
        }
        
        private readonly struct SelectableNeighborsData {
            
            public readonly int upId;
            public readonly int downId;
            public readonly int leftId;
            public readonly int rightId;
            
            public SelectableNeighborsData(int upId, int downId, int leftId, int rightId) {
                this.upId = upId;
                this.downId = downId;
                this.leftId = leftId;
                this.rightId = rightId;
            }
        }

        [BurstCompile]
        private struct GetSelectableNeighborsJob : IJobParallelFor 
        {
            [ReadOnly] public NativeArray<SelectableData> selectablesArray;
            [ReadOnly] public UiNavigationMode mode; 
            [ReadOnly] public UiNavigationLoop loop;
            [ReadOnly] public float2 cellSize;
            [WriteOnly] public NativeArray<SelectableNeighborsData> neighborsArray;
            
            public void Execute(int index) {
                var current = selectablesArray[index];

                int upId = 0;
                int downId = 0;
                int leftId = 0;
                int rightId = 0;
                
                int upmostId = 0;
                int downmostId = 0;
                int leftmostId = 0;
                int rightmostId = 0;
            
                float minDistanceUp = -1f;
                float minDistanceDown = -1f;
                float minDistanceLeft = -1f;
                float minDistanceRight = -1f;
                
                var distanceUpmost = new float2(-1f, -1f);
                var distanceDownmost = new float2(-1f, -1f);
                var distanceLeftmost = new float2(-1f, -1f);
                var distanceRightmost = new float2(-1f, -1f);
                
                for (int i = 0; i < selectablesArray.Length; i++) {
                    var data = selectablesArray[i];
                    if (data.id == current.id || (data.options & UiNavigationOptions.DisallowAnyIncomingNavigation) != 0) continue;
                    
                    var absDistance2 = new float2(math.abs(current.position.x - data.position.x), math.abs(current.position.y - data.position.y));  
                    var distanceCells2 = new int2((int) math.floor(absDistance2.x / cellSize.x), (int) math.floor(absDistance2.y / cellSize.y));

                    bool isUp = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Down) != 0 && data.position.IsHigherThan(current.position);
                    bool isDown = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Up) != 0 && data.position.IsLowerThan(current.position);
                    bool isLeft = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Right) != 0 && data.position.IsToTheLeftTo(current.position);
                    bool isRight = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Left) != 0 && data.position.IsToTheRightTo(current.position);

                    // Vertical and horizontal: check by cell distance then by abs distance
                    if ((isUp || isDown) && (isLeft || isRight)) {
                        if (distanceCells2.y > distanceCells2.x) {
                            isLeft = false;
                            isRight = false;
                        }
                        else if (distanceCells2.y < distanceCells2.x) {
                            isUp = false;
                            isDown = false;
                        }
                        else if (absDistance2.y >= absDistance2.x) {
                            isLeft = false;
                            isRight = false;
                        }
                        else {
                            isUp = false;
                            isDown = false;
                        }
                    }
                    
                    if (isUp && (minDistanceUp < 0f || absDistance2.y < minDistanceUp)) {
                        minDistanceUp = absDistance2.y;
                        upId = data.id;
                    }
                
                    if (isDown && (minDistanceDown < 0f || absDistance2.y < minDistanceDown)) {
                        minDistanceDown = absDistance2.y;
                        downId = data.id;
                    }
                
                    if (isRight && (minDistanceRight < 0f || absDistance2.x < minDistanceRight)) {
                        minDistanceRight = absDistance2.x;
                        rightId = data.id;
                    }
                
                    if (isLeft && (minDistanceLeft < 0f || absDistance2.x < minDistanceLeft)) {
                        minDistanceLeft = absDistance2.x;
                        leftId = data.id;
                    }

                    if ((loop & UiNavigationLoop.Vertical) != 0) {
                        if (isUp && (distanceUpmost.x < 0f || absDistance2.y >= distanceUpmost.y)) {
                            distanceUpmost = absDistance2;
                            upmostId = data.id;
                        }

                        if (isDown && (distanceDownmost.x < 0f || absDistance2.y >= distanceDownmost.y)) {
                            distanceDownmost = absDistance2;
                            downmostId = data.id;
                        }
                    }
                    
                    if ((loop & UiNavigationLoop.Horizontal) != 0)
                    {
                        if (isRight && (distanceRightmost.y < 0f || absDistance2.x >= distanceRightmost.x)) {
                            distanceRightmost = absDistance2;
                            rightmostId = data.id;
                        }
                    
                        if (isLeft && (distanceLeftmost.y < 0f || absDistance2.x >= distanceLeftmost.x)) {
                            distanceLeftmost = absDistance2;
                            leftmostId = data.id;
                        }
                    }
                }

                if ((loop & UiNavigationLoop.Vertical) != 0) {
                    if (upId == 0) upId = downmostId;
                    if (downId == 0) downId = upmostId;
                }

                if ((loop & UiNavigationLoop.Horizontal) != 0) {
                    if (rightId == 0) rightId = leftmostId;
                    if (leftId == 0) leftId = rightmostId;
                }

                if ((current.mask & UiNavigationMask.Up) == 0) upId = 0;
                if ((current.mask & UiNavigationMask.Down) == 0) downId = 0;
                if ((current.mask & UiNavigationMask.Left) == 0) leftId = 0;
                if ((current.mask & UiNavigationMask.Right) == 0) rightId = 0;
                
                neighborsArray[index] = new SelectableNeighborsData(upId, downId, leftId, rightId);
            }
        }
    }
    
}