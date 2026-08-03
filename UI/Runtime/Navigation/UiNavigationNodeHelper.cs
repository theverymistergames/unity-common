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
            float2 cell,
            UiNavigationMask mask,
            UiNavigationLoop loop) 
        {
            if (!direction.IsPossibleDirection(mask) ||
                direction.IsLoopDirection(loop) ||
                !Services.TryGet(out IUiNavigationService service)) 
            {
                return;
            }
            
            var root = node.GameObject.transform;
            var origin = root.InverseTransformPoint(fromSelectable.transform.position).ToFloat2XY();
            var selectables = service.Selectables;
            
            Selectable closestSelectable = null;
            float minSqr = float.MaxValue;
            var minOrthProjCells = new int2(int.MaxValue, int.MaxValue);

            foreach (var selectable in selectables) {
                if (IsBound(selectable.gameObject) || 
                    service.GetSelectableOptions(selectable) is var opt && 
                    ((opt & UiNavigationOptions.DisallowAnyIncomingNavigation) != 0 || 
                     (opt & UiNavigationOptions.DisallowIncomingNavigationFromOuterNodes) != 0))
                {
                    continue;
                }
            
                var pos = root.InverseTransformPoint(selectable.transform.position).ToFloat2XY();
                
                if (!pos.IsInDirection(origin, direction) ||
                    !UiNavigationUtils.IsCloserAlongDirection(pos, relativeTo: origin, cell, direction, ref minSqr, ref minOrthProjCells)) 
                {
                    continue;
                }
                
                closestSelectable = selectable;
            }

            if (closestSelectable == null) return;
            
            var nextParentNode = service.GetParentNavigationNode(closestSelectable);

            var nextHistoryTarget = nextParentNode?.CurrentSelectable != null
                ? nextParentNode.CurrentSelectable
                : nextParentNode?.DefaultSelectable;
            
            var nextOptions = nextHistoryTarget == null 
                ? UiIncomingOuterNavigationOptions.SelectClosestElement
                : nextParentNode.IncomingOuterNavigation;
            
            var selectTarget = nextOptions switch {
                UiIncomingOuterNavigationOptions.SelectClosestElement => closestSelectable,
                UiIncomingOuterNavigationOptions.SelectHistoryElement => nextHistoryTarget,
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
            
                float minSqrUp = float.MaxValue;
                float minSqrDown = float.MaxValue;
                float minSqrLeft = float.MaxValue;
                float minSqrRight = float.MaxValue;
                
                var minOrthProjCellsUp = new int2(int.MaxValue, int.MaxValue);
                var minOrthProjCellsDown = new int2(int.MaxValue, int.MaxValue);
                var minOrthProjCellsLeft = new int2(int.MaxValue, int.MaxValue);
                var minOrthProjCellsRight = new int2(int.MaxValue, int.MaxValue);
                
                float maxProjMinusOrthUpmost = float.MinValue;
                float maxProjMinusOrthDownmost = float.MinValue;
                float maxProjMinusOrthLeftmost = float.MinValue;
                float maxProjMinusOrthRightmost = float.MinValue;
                
                var maxOrthProjCellsUpmost = new int2(int.MaxValue, int.MinValue);
                var maxOrthProjCellsDownmost = new int2(int.MaxValue, int.MinValue);
                var maxOrthProjCellsLeftmost = new int2(int.MaxValue, int.MinValue);
                var maxOrthProjCellsRightmost = new int2(int.MaxValue, int.MinValue);
                
                for (int i = 0; i < selectablesArray.Length; i++) {
                    var data = selectablesArray[i];
                    if (data.id == current.id || (data.options & UiNavigationOptions.DisallowAnyIncomingNavigation) != 0) continue;
                    
                    var absDistance2 = math.abs(current.position - data.position);  
                    var cells2 = new int2((int) math.floor(absDistance2.x / cellSize.x), (int) math.floor(absDistance2.y / cellSize.y));
                    float sqr = math.lengthsq(absDistance2);
                    
                    bool isUp = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Down) != 0 && data.position.IsHigherThan(current.position);
                    bool isDown = mode != UiNavigationMode.Horizontal && (data.mask & UiNavigationMask.Up) != 0 && data.position.IsLowerThan(current.position);
                    bool isLeft = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Right) != 0 && data.position.IsToTheLeftTo(current.position);
                    bool isRight = mode != UiNavigationMode.Vertical && (data.mask & UiNavigationMask.Left) != 0 && data.position.IsToTheRightTo(current.position);

                    // Vertical and horizontal: check by cell distance then by abs distance
                    if ((isUp || isDown) && (isLeft || isRight)) {
                        if (cells2.y > cells2.x) {
                            isLeft = false;
                            isRight = false;
                        }
                        else if (cells2.y < cells2.x) {
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

                    if (isUp &&
                        UiNavigationUtils.IsCloserAlongDirection(sqr, cells2.xy, ref minSqrUp, ref minOrthProjCellsUp)) 
                    {
                        upId = data.id;
                    }
                    
                    if (isDown &&
                        UiNavigationUtils.IsCloserAlongDirection(sqr, cells2.xy, ref minSqrDown, ref minOrthProjCellsDown)) 
                    {
                        downId = data.id;
                    }
                    
                    if (isLeft &&
                        UiNavigationUtils.IsCloserAlongDirection(sqr, cells2.yx, ref minSqrLeft, ref minOrthProjCellsLeft)) 
                    {
                        leftId = data.id;
                    }
                
                    if (isRight &&
                        UiNavigationUtils.IsCloserAlongDirection(sqr, cells2.yx, ref minSqrRight, ref minOrthProjCellsRight)) 
                    {
                        rightId = data.id;
                    }

                    if ((loop & UiNavigationLoop.Vertical) != 0) {
                        if (isUp && 
                            UiNavigationUtils.IsFartherAlongDirection(absDistance2.y - absDistance2.x, cells2.xy, ref maxProjMinusOrthUpmost, ref maxOrthProjCellsUpmost)) 
                        {
                            upmostId = data.id;
                        }

                        if (isDown && 
                            UiNavigationUtils.IsFartherAlongDirection(absDistance2.y - absDistance2.x, cells2.xy, ref maxProjMinusOrthDownmost, ref maxOrthProjCellsDownmost)) 
                        {
                            downmostId = data.id;
                        }
                    }
                    
                    if ((loop & UiNavigationLoop.Horizontal) != 0)
                    {
                        if (isLeft && 
                            UiNavigationUtils.IsFartherAlongDirection(absDistance2.x - absDistance2.y, cells2.yx, ref maxProjMinusOrthLeftmost, ref maxOrthProjCellsLeftmost)) 
                        {
                            leftmostId = data.id;
                        }
                    
                        if (isRight && 
                            UiNavigationUtils.IsFartherAlongDirection(absDistance2.x - absDistance2.y, cells2.yx, ref maxProjMinusOrthRightmost, ref maxOrthProjCellsRightmost)) 
                        {
                            rightmostId = data.id;
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