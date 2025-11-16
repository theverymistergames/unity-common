using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Easing {
    
    internal sealed class EasingVisualizer : MonoBehaviour {
        
        [Header("Curve")]
        //public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
        public EasingType easingType = EasingType.Linear;
        [Min(0f)] public float power = 10f;
        [Range(0f, 1f)] public float weight = 0.5f;
        [Min(0)] public int count = 1;
        [Range(0f, 1f)] public float point = 0.5f;

        [Header("Grid")]
        [Range(1, 1000)] public float samples = 100;
        [Min(0f)] public float sizeX = 1f;
        [Min(0f)] public float sizeY = 1f;
        public Vector2 curveEasingOffset;
        public Vector2 labelOffset;
        
        [Header("Display")]
        public bool displayCurve = true;
        public bool displayEasingType = true;
        public bool displaySum = false;
        
        private float _t;
        private float _x;
        
        private void OnValidate() {
            //curve = easingType.ToAnimationCurve();
        }

        public static float LinearInterpolation(float value, float b, float t, float tPrev) {
            return value + (b - value) * ((t - tPrev) / (1f - tPrev));
        }
        
        
        public static float Smooth(float value, float b, float speed, float dt) {
            return value + (b - value) * speed * dt;
        }
        
        private void OnDrawGizmos() {
            transform.GetPositionAndRotation(out var pos, out var rot);
            var offset = Vector3.right * curveEasingOffset.x + Vector3.up * curveEasingOffset.y;
            
            //if (displayCurve) Draw(t => curve.Evaluate(t), pos, rot, Color.yellow);
            //if (displayEasingType) Draw(t => easingType.Evaluate(t), pos + rot * offset, rot, Color.cyan);
            //if (displaySum) Draw(t => (curve.Evaluate(t) + easingType.Evaluate(t)) * 0.5f, pos + rot * offset * 2f, rot, Color.white);

            Draw(GetExpValue, pos, rot, Color.magenta);
            Draw(GetT, pos, rot, Color.green);
            Draw(t => GetExpValue(GetT(t)), pos, rot, Color.white);
            
            //Draw(t => curve2.Evaluate(GetExpValue(t)), pos, rot, Color.blue);
            
            DebugExt.DrawLabel(transform.TransformPoint(labelOffset), $"{easingType}");
        }

        private float GetExpValue(float t) {
            _x = _x.SmoothExp(1f, t);
            return _x;
        }

        private float GetT(float t) {
            for (int i = 0; i < count && t > point; i++) {
                t = (t - point) / (1f - point);
            }
            
            return t * (1f - weight) + easingType.EvaluatePower(t, power) * weight;
        }
        
        private static float ConvertLerpExpT(float t) {
            for (int i = 0; i < 9 && t > 0.5f; i++) {
                t = 2f * (t - 0.5f);
            }
            
            return t * (1f - 0.98105f) + EasingFunctions.EaseInExpo(t, 19f) * 0.98105f;
        }

        private void Draw(Func<float, float> func, Vector3 pos, Quaternion rot, Color color) {
            _x = 0f;
            _t = 0f;
            
            var lastPoint = pos;
            
            for (int i = 1; i <= samples; i++) {
                float t = i / samples;
                var p = ConvertPoint(pos, rot, GetPoint(func, t));
                
                DebugExt.DrawLine(p, lastPoint, color);
                lastPoint = p;
            }
            /*
            var p0 = ConvertPoint(pos, rot, GetPoint(func, 0f));
            var p1 = ConvertPoint(pos, rot, GetPoint(func, 1f));
            
            DebugExt.DrawLabel(p0, $"x=0 y={func(0f)}");
            DebugExt.DrawLabel(p1, $"x=1 y={func(1f)}");
            */
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