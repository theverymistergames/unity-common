using System;
using System.Collections.Generic;
using MisterGames.Common.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationNodeHelper : IDisposable {

        private readonly Dictionary<int, Selectable> _gameObjectIdToSelectableMap = new();

        public void Dispose() {
            _gameObjectIdToSelectableMap.Clear();
        }

        public void Bind(Selectable selectable) {
            _gameObjectIdToSelectableMap[selectable.gameObject.GetHashCode()] = selectable;
        }

        public void Unbind(Selectable selectable) {
            _gameObjectIdToSelectableMap.Remove(selectable.gameObject.GetHashCode());
        }

        public bool IsBound(GameObject gameObject) {
            return _gameObjectIdToSelectableMap.ContainsKey(gameObject.GetHashCode());
        }
        
        public void UpdateNavigation(Transform rootTrf, UiNavigationMode mode, bool loop) {
            var selectablesArray = new NativeArray<SelectableData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
            var neighborsArray = new NativeArray<SelectableNeighborsData>(_gameObjectIdToSelectableMap.Count, Allocator.TempJob);
            
            int count = 0;
            
            foreach ((int id, var selectable) in _gameObjectIdToSelectableMap) {
                selectablesArray[count++] = new SelectableData(id, rootTrf.InverseTransformPoint(selectable.transform.position));
            }

            var job = new GetSelectableNeighborsJob {
                selectablesArray = selectablesArray,
                mode = mode,
                loop = loop,
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
            
            public SelectableData(int id, float3 position) {
                this.id = id;
                this.position = math.float2(position.x, position.y);
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
            
            [WriteOnly] public NativeArray<SelectableNeighborsData> neighborsArray;
            
            public void Execute(int index) {
                var current = selectablesArray[index];

                int upId = 0;
                int downId = 0;
                int leftId = 0;
                int rightId = 0;
            
                float minSqrDistanceUp = -1f;
                float minSqrDistanceDown = -1f;
                float minSqrDistanceLeft = -1f;
                float minSqrDistanceRight = -1f;
                
                int upmostId = 0;
                int downmostId = 0;
                int leftmostId = 0;
                int rightmostId = 0;
            
                var minDistanceUpmost = new float2(-1f, -1f);
                var minDistanceDownmost = new float2(-1f, -1f);
                var minDistanceLeftmost = new float2(-1f, -1f);
                var minDistanceRightmost = new float2(-1f, -1f);
                
                for (int i = 0; i < selectablesArray.Length; i++) {
                    var data = selectablesArray[i];
                    if (data.id == current.id) continue;

                    bool isUp = data.position.IsHigherThan(current.position, mode);
                    bool isDown = data.position.IsLowerThan(current.position, mode);
                    bool isLeft = data.position.IsToTheLeftTo(current.position, mode);
                    bool isRight = data.position.IsToTheRightTo(current.position, mode);

                    if (loop) {
                        if (isUp &&
                            (minDistanceUpmost.x < 0f ||
                             data.position.y - current.position.y >= minDistanceUpmost.y &&
                             math.abs(current.position.x - data.position.x) <= minDistanceUpmost.x)) 
                        {
                            minDistanceUpmost = new float2(math.abs(current.position.x - data.position.x), data.position.y - current.position.y);
                            upmostId = data.id;
                        }
                    
                        if (isDown &&
                            (minDistanceDownmost.x < 0f ||
                             current.position.y - data.position.y >= minDistanceDownmost.y &&
                             math.abs(current.position.x - data.position.x) <= minDistanceDownmost.x)) 
                        {
                            minDistanceDownmost = new float2(math.abs(current.position.x - data.position.x), current.position.y - data.position.y);
                            downmostId = data.id;
                        }
                    
                        if (isRight &&
                            (minDistanceRightmost.y < 0f ||
                             data.position.x - current.position.x >= minDistanceRightmost.x &&
                             math.abs(current.position.y - data.position.y) <= minDistanceRightmost.y)) 
                        {
                            minDistanceRightmost = new float2(data.position.x - current.position.x, math.abs(current.position.y - data.position.y));
                            rightmostId = data.id;
                        }
                    
                        if (isLeft &&
                            (minDistanceLeftmost.y < 0f ||
                             current.position.x - data.position.x >= minDistanceLeftmost.x &&
                             math.abs(current.position.y - data.position.y) <= minDistanceLeftmost.y)) 
                        {
                            minDistanceLeftmost = new float2(current.position.x - data.position.x, math.abs(current.position.y - data.position.y));
                            leftmostId = data.id;
                        }
                    }
                    
                    float sqrDistance = math.distancesq(current.position, data.position);
                    
                    if (isUp && (minSqrDistanceUp < 0f || sqrDistance < minSqrDistanceUp)) {
                        minSqrDistanceUp = sqrDistance;
                        upId = data.id;
                        continue;
                    }
                
                    if (isDown && (minSqrDistanceDown < 0f || sqrDistance < minSqrDistanceDown)) {
                        minSqrDistanceDown = sqrDistance;
                        downId = data.id;
                        continue;
                    }
                
                    if (isRight && (minSqrDistanceRight < 0f || sqrDistance < minSqrDistanceRight)) {
                        minSqrDistanceRight = sqrDistance;
                        rightId = data.id;
                        continue;
                    }
                
                    if (isLeft && (minSqrDistanceLeft < 0f || sqrDistance < minSqrDistanceLeft)) {
                        minSqrDistanceLeft = sqrDistance;
                        leftId = data.id;
                    }
                }

                if (loop) {
                    if (upId == 0) upId = downmostId;
                    if (downId == 0) downId = upmostId;
                    if (rightId == 0) rightId = leftmostId;
                    if (leftId == 0) leftId = rightmostId;
                }
                
                neighborsArray[index] = new SelectableNeighborsData(upId, downId, leftId, rightId);
            }
        }
    }
    
}