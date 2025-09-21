using System;
using System.Collections.Generic;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiNavigationNode : MonoBehaviour, IUiNavigationNode {

        [SerializeField] private Mode _mode;
        [SerializeField] private bool _loop = true;
        
        private enum Mode {
            Grid,
            Vertical,
            Horizontal,
        }

        GameObject IUiNavigationNode.GameObject => gameObject;
        
        private readonly Dictionary<int, Selectable> _selectableMap = new();
        
        private void OnEnable() {
            Services.Get<IUiNavigationService>()?.BindNavigation(this);
        }

        private void OnDisable() {
            Services.Get<IUiNavigationService>()?.UnbindNavigation(this);
        }

        void IUiNavigationNode.Bind(Selectable selectable) {
            _selectableMap[selectable.GetHashCode()] = selectable;
            RecalculateSelectablesLayout();
        }

        void IUiNavigationNode.Unbind(Selectable selectable) {
            _selectableMap.Remove(selectable.GetHashCode());
            RecalculateSelectablesLayout();
        }

        void IUiNavigationNode.Bind(IUiNavigationNode node) {
            
        }

        void IUiNavigationNode.Unbind(IUiNavigationNode node) {
            
        }

        private void RecalculateSelectablesLayout() {
            var selectablesArray = new NativeArray<SelectableData>(_selectableMap.Count, Allocator.TempJob);
            var neighborsArray = new NativeArray<SelectableNeighborsData>(_selectableMap.Count, Allocator.TempJob);
            
            int count = 0;
            var rootTrf = transform;
            
            foreach ((int id, var selectable) in _selectableMap) {
                selectablesArray[count++] = new SelectableData(id, rootTrf.InverseTransformPoint(selectable.transform.position));
            }

            var job = new GetSelectableNeighborsJob {
                selectablesArray = selectablesArray,
                mode = _mode,
                loop = _loop,
                neighborsArray = neighborsArray,
            };
            
            job.Schedule(count, JobExt.BatchFor(count)).Complete();

            for (int i = 0; i < count; i++) {
                var data = selectablesArray[i];
                var neighborsData = neighborsArray[i];

                var selectable = _selectableMap[data.id];
                var navigation = selectable.navigation;

                navigation.selectOnUp = _selectableMap.GetValueOrDefault(neighborsData.upId);
                navigation.selectOnDown = _selectableMap.GetValueOrDefault(neighborsData.downId);
                navigation.selectOnLeft = _selectableMap.GetValueOrDefault(neighborsData.leftId);
                navigation.selectOnRight = _selectableMap.GetValueOrDefault(neighborsData.rightId);
                
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
            [ReadOnly] public Mode mode;
            [ReadOnly] public bool loop;
            
            [WriteOnly] public NativeArray<SelectableNeighborsData> neighborsArray;

            private static readonly int2 Up = new(0, 1);
            private static readonly int2 Down = new(0, -1);
            private static readonly int2 Left = new(-1, 0);
            private static readonly int2 Right = new(1, 0);
            
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

                    float sqrDistance = math.distancesq(current.position, data.position);

                    if (loop) {
                        if (data.id != current.id && IsInDirection(mode, Up, current.position, data.position) &&
                            (minDistanceUpmost.x < 0f ||
                             data.position.y - current.position.y >= minDistanceUpmost.y &&
                             math.abs(current.position.x - data.position.x) <= minDistanceUpmost.x)) 
                        {
                            minDistanceUpmost = new float2(math.abs(current.position.x - data.position.x), data.position.y - current.position.y);
                            upmostId = data.id;
                        }
                    
                        if (data.id != current.id && IsInDirection(mode, Down, current.position, data.position) &&
                            (minDistanceDownmost.x < 0f ||
                             current.position.y - data.position.y >= minDistanceDownmost.y &&
                             math.abs(current.position.x - data.position.x) <= minDistanceDownmost.x)) 
                        {
                            minDistanceDownmost = new float2(math.abs(current.position.x - data.position.x), current.position.y - data.position.y);
                            downmostId = data.id;
                        }
                    
                        if (data.id != current.id && IsInDirection(mode, Right, current.position, data.position) &&
                            (minDistanceRightmost.y < 0f ||
                             data.position.x - current.position.x >= minDistanceRightmost.x &&
                             math.abs(current.position.y - data.position.y) <= minDistanceRightmost.y)) 
                        {
                            minDistanceRightmost = new float2(data.position.x - current.position.x, math.abs(current.position.y - data.position.y));
                            rightmostId = data.id;
                        }
                    
                        if (data.id != current.id && IsInDirection(mode, Left, current.position, data.position) &&
                            (minDistanceLeftmost.y < 0f ||
                             current.position.x - data.position.x >= minDistanceLeftmost.x &&
                             math.abs(current.position.y - data.position.y) <= minDistanceLeftmost.y)) 
                        {
                            minDistanceLeftmost = new float2(current.position.x - data.position.x, math.abs(current.position.y - data.position.y));
                            leftmostId = data.id;
                        }
                    }
                    
                    if (data.id != current.id && IsInDirection(mode, Up, current.position, data.position) &&
                        (minSqrDistanceUp < 0f || sqrDistance < minSqrDistanceUp)) 
                    {
                        minSqrDistanceUp = sqrDistance;
                        upId = data.id;
                        continue;
                    }
                
                    if (data.id != current.id && IsInDirection(mode, Down, current.position, data.position) &&
                        (minSqrDistanceDown < 0f || sqrDistance < minSqrDistanceDown)) 
                    {
                        minSqrDistanceDown = sqrDistance;
                        downId = data.id;
                        continue;
                    }
                
                    if (data.id != current.id && IsInDirection(mode, Right, current.position, data.position) &&
                        (minSqrDistanceRight < 0f || sqrDistance < minSqrDistanceRight)) 
                    {
                        minSqrDistanceRight = sqrDistance;
                        rightId = data.id;
                        continue;
                    }
                
                    if (data.id != current.id && IsInDirection(mode, Left, current.position, data.position) &&
                        (minSqrDistanceLeft < 0f || sqrDistance < minSqrDistanceLeft)) 
                    {
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
            
            private static bool IsInDirection(Mode mode, int2 direction, float2 origin, float2 position) {
                return mode switch {
                    Mode.Grid => VectorUtils.Angle(direction, position - origin) <= 45f,
                
                    Mode.Vertical => direction switch {
                        { x: 0, y: 1 } => position.y > origin.y,
                        { x: 0, y: -1 } => position.y < origin.y,
                        _ => false,
                    },
                    
                    Mode.Horizontal => direction switch {
                        { x: 1, y: 0 } => position.x > origin.x,
                        { x: -1, y: 0 } => position.x < origin.x,
                        _ => false,
                    },
                    
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
    
}