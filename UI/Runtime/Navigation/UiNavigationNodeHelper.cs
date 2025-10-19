using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Strings;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationNodeHelper : IDisposable {

        private readonly Dictionary<int, Selectable> _gameObjectIdToSelectableMap = new();
        private readonly Dictionary<int, UiNavigationMask> _gameObjectIdToMaskMap = new();

        public void Dispose() {
            _gameObjectIdToSelectableMap.Clear();
            _gameObjectIdToMaskMap.Clear();
        }

        public void Bind(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None) {
            int hash = selectable.gameObject.GetHashCode();
            
            _gameObjectIdToSelectableMap[hash] = selectable;
            _gameObjectIdToMaskMap[hash] = mask;
        }

        public void Unbind(Selectable selectable) {
            int hash = selectable.gameObject.GetHashCode();
            
            _gameObjectIdToSelectableMap.Remove(hash);
            _gameObjectIdToMaskMap.Remove(hash);
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
            float minSqrDistance = -1f;

            foreach (var selectable in selectables) {
                if (IsBound(selectable.gameObject) || 
                    service.GetParentNavigationNode(selectable) is not { } p || 
                    !allowParent && p != parentNode || 
                    !allowSiblings && !service.IsChildNode(p, parentNode, direct: true) ||
                    !allowChildren && !service.IsChildNode(p, node, direct: true))
                {
                    continue;
                }
            
                var pos = root.InverseTransformPoint(selectable.transform.position).ToFloat2XY();
                if (!pos.IsInDirection(origin, direction)) continue;
                
                float sqrDistance = math.distancesq(pos, origin);
                if (minSqrDistance >= 0f && sqrDistance > minSqrDistance) continue;
                
                minSqrDistance = sqrDistance;
                closestSelectable = selectable;
            }

            if (closestSelectable == null) return;

            var nextParentNode = service.GetParentNavigationNode(closestSelectable);
            
            var nextOptions = nextParentNode?.CurrentSelected == null
                ? UiNavigateFromOuterNodesOptions.SelectClosestElement
                : nextParentNode.NavigateFromOuterNodesOptions;
            
            var selectTarget = nextOptions switch {
                UiNavigateFromOuterNodesOptions.SelectClosestElement => closestSelectable.gameObject,
                UiNavigateFromOuterNodesOptions.SelectHistoryElement => nextParentNode!.CurrentSelected,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            service.SelectGameObject(selectTarget);
        }
        
        public async UniTask UpdateNavigationNextFrame(
            Transform rootTrf,
            UiNavigationMode mode,
            bool loop,
            Vector2 cell,
            CancellationToken cancellationToken) 
        {
            UpdateNavigation(rootTrf, mode, loop, cell);
            
            // The position of the selectable during enabling layout groups maybe inconsistent
            // (all selectables in the layout group share the same selectable.transform.position), 
            // so to avoid setting incorrect navigation lets update it two frames later.
            await UniTask.Yield();
            await UniTask.Yield();
            if (cancellationToken.IsCancellationRequested) return;

            UpdateNavigation(rootTrf, mode, loop, cell);
        }
        
        public void UpdateNavigation(Transform rootTrf, UiNavigationMode mode, bool loop, Vector2 cellSize) {
            var selectablesArray = new NativeArray<SelectableData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
            var neighborsArray = new NativeArray<SelectableNeighborsData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
             
            int count = 0;

            foreach ((int id, var selectable) in _gameObjectIdToSelectableMap) {
                selectablesArray[count++] = new SelectableData(
                    id,
                    rootTrf.InverseTransformPoint(selectable.transform.position), 
                    _gameObjectIdToMaskMap.GetValueOrDefault(id)
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

            public SelectableData(int id, float3 position, UiNavigationMask mask) {
                this.id = id;
                this.position = math.float2(position.x, position.y);
                this.mask = mask;
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
        private struct GetSelectableNeighborsJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<SelectableData> selectablesArray;
            [ReadOnly] public UiNavigationMode mode;
            [ReadOnly] public bool loop;
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
            
                float minSqrDistanceUp = -1f;
                float minSqrDistanceDown = -1f;
                float minSqrDistanceLeft = -1f;
                float minSqrDistanceRight = -1f;
                
                var distanceUpmost = new float2(-1f, -1f);
                var distanceDownmost = new float2(-1f, -1f);
                var distanceLeftmost = new float2(-1f, -1f);
                var distanceRightmost = new float2(-1f, -1f);
                
                var distanceUpmostCells = new int2(-1, -1);
                var distanceDownmostCells = new int2(-1, -1);
                var distanceLeftmostCells = new int2(-1, -1);
                var distanceRightmostCells = new int2(-1, -1);
                
                for (int i = 0; i < selectablesArray.Length; i++) {
                    var data = selectablesArray[i];
                    if (data.id == current.id) continue;

                    bool isUp = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Down) != 0 && data.position.IsHigherThan(current.position);
                    bool isDown = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Up) != 0 && data.position.IsLowerThan(current.position);
                    bool isLeft = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Right) != 0 && data.position.IsToTheLeftTo(current.position);
                    bool isRight = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Left) != 0 && data.position.IsToTheRightTo(current.position);

                    var distance2 = new float2(
                        math.abs(current.position.x - data.position.x),
                        math.abs(current.position.y - data.position.y)
                    );  
                    
                    var distanceCells2 = new int2((int) math.floor(distance2.x / cellSize.x), (int) math.floor(distance2.y / cellSize.y));
                    float sqrDistance = math.distancesq(current.position, data.position);

                    if (isUp && (minSqrDistanceUp < 0f || sqrDistance < minSqrDistanceUp)) {
                        minSqrDistanceUp = sqrDistance;
                        upId = data.id;
                    }
                
                    if (isDown && (minSqrDistanceDown < 0f || sqrDistance < minSqrDistanceDown)) {
                        minSqrDistanceDown = sqrDistance;
                        downId = data.id;
                    }
                
                    if (isRight && (minSqrDistanceRight < 0f || sqrDistance < minSqrDistanceRight)) {
                        minSqrDistanceRight = sqrDistance;
                        rightId = data.id;
                    }
                
                    if (isLeft && (minSqrDistanceLeft < 0f || sqrDistance < minSqrDistanceLeft)) {
                        minSqrDistanceLeft = sqrDistance;
                        leftId = data.id;
                    }
                    
                    if (loop) {
                        if (isUp &&
                            (distanceUpmost.x < 0f ||
                             distance2.y >= distanceUpmost.y && 
                             (distance2.x <= distanceUpmost.x || distanceCells2.x <= distanceUpmostCells.x))) 
                        {
                            distanceUpmost = distance2;
                            distanceUpmostCells = distanceCells2;
                            upmostId = data.id;
                        }
                    
                        if (isDown &&
                            (distanceDownmost.x < 0f ||
                             distance2.y >= distanceDownmost.y && 
                             (distance2.x <= distanceDownmost.x || distanceCells2.x <= distanceDownmostCells.x))) 
                        {
                            distanceDownmost = distance2;
                            distanceDownmostCells = distanceCells2;
                            downmostId = data.id;
                        }
                    
                        if (isRight &&
                            (distanceRightmost.y < 0f ||
                             distance2.x >= distanceRightmost.x && 
                             (distance2.y <= distanceRightmost.y || distanceCells2.y <= distanceRightmostCells.y))) 
                        {
                            distanceRightmost = distance2;
                            distanceRightmostCells = distanceCells2;
                            rightmostId = data.id;
                        }
                    
                        if (isLeft &&
                            (distanceLeftmost.y < 0f ||
                             distance2.x >= distanceLeftmost.x && 
                             (distance2.y <= distanceLeftmost.y || distanceCells2.y <= distanceLeftmostCells.y))) 
                        {
                            distanceLeftmost = distance2;
                            distanceLeftmostCells = distanceCells2;
                            leftmostId = data.id;
                        }
                    }
                }

                if (loop) {
                    if (upId == 0) upId = downmostId;
                    if (downId == 0) downId = upmostId;
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