using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Service;
using MisterGames.UI.UiServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.UI.Navigation {
    
    public static class UiNavigationUtils {
    
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHigherThan(this float2 position, float2 relativeTo) => position.y > relativeTo.y;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowerThan(this float2 position, float2 relativeTo) => position.y < relativeTo.y;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToTheLeftTo(this float2 position, float2 relativeTo) => position.x < relativeTo.x;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToTheRightTo(this float2 position, float2 relativeTo) => position.x > relativeTo.x;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInDirection(this float2 position, float2 relativeTo, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up => position.y > relativeTo.y,
                UiNavigationDirection.Down => position.y < relativeTo.y,
                UiNavigationDirection.Left => position.x < relativeTo.x,
                UiNavigationDirection.Right => position.x > relativeTo.x,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPossibleDirection(this UiNavigationDirection direction, UiNavigationMask mask) {
            return ((1 << (int) direction) & (int) mask) != 0;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLoopDirection(this UiNavigationDirection direction, UiNavigationLoop loop) {
            int vertical = (int) (loop & UiNavigationLoop.Vertical);
            int horizontal = (int) (loop & UiNavigationLoop.Horizontal);
            int mask = vertical | (vertical << 1) | (horizontal << 1) | (horizontal << 2);
            return ((1 << (int) direction) & mask) != 0;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Project(this float2 vector, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up or UiNavigationDirection.Down => vector.y, 
                UiNavigationDirection.Left or UiNavigationDirection.Right => vector.x,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Project(this int2 vector, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up or UiNavigationDirection.Down => vector.y, 
                UiNavigationDirection.Left or UiNavigationDirection.Right => vector.x,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Orthogonal(this float2 vector, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up or UiNavigationDirection.Down => vector.x, 
                UiNavigationDirection.Left or UiNavigationDirection.Right => vector.y,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Orthogonal(this int2 vector, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up or UiNavigationDirection.Down => vector.x, 
                UiNavigationDirection.Left or UiNavigationDirection.Right => vector.y,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        /// <summary>
        /// To sort positions by:
        /// 1) Min orthogonal distance to ray along direction measured in cells
        /// 2) Min projection distance between positions along direction measured in cells
        /// 3) Min direct distance between positions.
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCloserAlongDirection(
            float sqrDistance,
            int2 orthProjCells, 
            ref float minSqrDistance,
            ref int2 minOrthProjCells) 
        {
            if (orthProjCells.x < minOrthProjCells.x || 
                orthProjCells.x == minOrthProjCells.x && 
                (orthProjCells.y < minOrthProjCells.y || orthProjCells.y == minOrthProjCells.y && sqrDistance < minSqrDistance)) 
            {
                minSqrDistance = sqrDistance;
                minOrthProjCells = orthProjCells;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// To sort positions by:
        /// 1) Min orthogonal distance to ray along direction measured in cells
        /// 2) Max projection distance between positions along direction.
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFartherAlongDirection(
            float projMinusOrthDistance,
            int orthCells, 
            ref float maxProjMinusOrthDistance,
            ref int minOrthCells) 
        {
            if (orthCells < minOrthCells || 
                orthCells == minOrthCells && projMinusOrthDistance > maxProjMinusOrthDistance) 
            {
                maxProjMinusOrthDistance = projMinusOrthDistance;
                minOrthCells = orthCells;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// To sort positions by:
        /// 1) Min orthogonal distance to ray along direction measured in cells
        /// 2) Min projection distance between positions along direction measured in cells
        /// 3) Min direct distance between positions.
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCloserAlongDirection(
            float2 pos,
            float2 relativeTo,
            float2 cell,
            UiNavigationDirection direction,
            ref float minSqrDistance,
            ref int2 minOrthProjCells) 
        {
            var absDistance2 = math.abs(pos - relativeTo);
            var distanceCells2 = new int2((int) math.floor(absDistance2.x / cell.x), (int) math.floor(absDistance2.y / cell.y));
            float sqrDistance = math.lengthsq(absDistance2);
            var orthProjCells = direction switch {
                UiNavigationDirection.Up or UiNavigationDirection.Down => new int2(distanceCells2.x, distanceCells2.y),
                UiNavigationDirection.Left or UiNavigationDirection.Right => new int2(distanceCells2.y, distanceCells2.x),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            return IsCloserAlongDirection(sqrDistance, orthProjCells, ref minSqrDistance, ref minOrthProjCells);
        }
        
        /// <summary>
        /// To sort positions by:
        /// 1) Min orthogonal distance to ray along direction measured in cells
        /// 2) Max projection distance between positions along direction.
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFartherAlongDirection(
            float2 pos,
            float2 relativeTo,
            float2 cell,
            UiNavigationDirection direction,
            ref float maxProjMinusOrthDistance,
            ref int minOrthCells) 
        {
            var absDistance2 = math.abs(pos - relativeTo);
            int orthCells;
            float projMinusOrthDistance;
            
            switch (direction) {
                case UiNavigationDirection.Up:
                case UiNavigationDirection.Down:
                    projMinusOrthDistance = absDistance2.y - absDistance2.x;
                    orthCells = (int) math.floor(absDistance2.x / cell.x);
                    break;
                
                case UiNavigationDirection.Left:
                case UiNavigationDirection.Right:
                    projMinusOrthDistance = absDistance2.x - absDistance2.y;
                    orthCells = (int) math.floor(absDistance2.y / cell.y);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            return IsFartherAlongDirection(projMinusOrthDistance, orthCells, ref maxProjMinusOrthDistance, ref minOrthCells);
        }
        
        public static bool IsCursorInsideRect(RectTransform rectTransform, Vector4 offset = default) {
            var camera = Services.TryGet(out CanvasRegistry canvasRegistry) &&
                         canvasRegistry.TryGetCurrentEventCamera(out var c)
                ? c
                : null;

            return Cursor.visible &&
                   RectTransformUtility.RectangleContainsScreenPoint(rectTransform, UnityEngine.Input.mousePosition, camera, offset);
        }
    }
    
}