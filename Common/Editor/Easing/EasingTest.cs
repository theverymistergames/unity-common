using System;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Common.Editor.Easing {
    
    internal sealed class EasingTest : MonoBehaviour {
        
        public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
        public EasingType easingType = EasingType.Linear;
        
        [Range(1, 500)] public float samples = 100;
        [Min(0f)] public float sizeX = 1f;
        [Min(0f)] public float sizeY = 1f;
        public Vector2 curveEasingOffset;
        public Vector2 labelOffset;
        public bool displayCurve = true;
        public bool displayEasingType = true;

        private void OnValidate() {
            curve = easingType.ToAnimationCurve();
        }

        private void OnDrawGizmos() {
            transform.GetPositionAndRotation(out var pos, out var rot);
            var offset = Vector3.right * curveEasingOffset.x + Vector3.up * curveEasingOffset.y;
            
            if (displayCurve) Draw(t => curve.Evaluate(t), pos, rot, Color.yellow);
            if (displayEasingType) Draw(t => easingType.Evaluate(t), pos + rot * offset, rot, Color.cyan);
            
            DebugExt.DrawLabel(transform.TransformPoint(labelOffset), $"{easingType}");
        }

        private void Draw(Func<float, float> func, Vector3 pos, Quaternion rot, Color color) {
            for (int i = 1; i <= samples; i++) {
                float t = i / samples;
                var p = ConvertPoint(pos, rot, GetPoint(func, t));
                
                var pPrev = ConvertPoint(pos, rot, GetPoint(func, (i - 1) / samples));
                DebugExt.DrawLine(p, pPrev, color);
            }
            
            var p0 = ConvertPoint(pos, rot, GetPoint(func, 0f));
            var p1 = ConvertPoint(pos, rot, GetPoint(func, 1f));
            
            DebugExt.DrawLabel(p0, $"x=0 y={func(0f)}");
            DebugExt.DrawLabel(p1, $"x=1 y={func(1f)}");
            
            DebugExt.DrawRay(pos, rot * (Vector3.right * sizeX), Color.green);
            DebugExt.DrawRay(pos, rot * (Vector3.up * sizeY), Color.green);
        }

        private Vector2 GetPoint(Func<float, float> func, float t) {
            float x = t * sizeX;
            float y = func(t) * sizeY;
            
            return new Vector2(x, y);
        }

        private static Vector3 ConvertPoint(Vector3 pos, Quaternion rot, Vector2 point) {
            return pos + rot * (Vector3.right * point.x) + rot * (Vector3.up * point.y);
        }
    }
    
}