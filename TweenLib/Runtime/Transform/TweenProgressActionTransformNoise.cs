using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionTransformNoise : ITweenProgressAction {

        public Transform transform;
        public AnimationCurve curve;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public Vector3 scaleOffset;
        public Vector3 positionMultiplier;
        public Vector3 rotationMultiplier;
        public Vector3 scaleMultiplier;
        public float noiseScale;
        public bool useLocal = true;
        public ApplyMode applyMode = ApplyMode.Position | ApplyMode.Rotation | ApplyMode.Scale;
        
        [Flags]
        public enum ApplyMode {
            None = 0,
            Position = 1,
            Rotation = 2,
            Scale = 4,
        }
        
        public void OnProgressUpdate(float progress) {
            float s = curve.Evaluate(progress);
            float t = progress * noiseScale;

            var positionNoise = s * new Vector3(
                (Mathf.PerlinNoise1D(t + positionOffset.x) - 0.5f) * positionMultiplier.x,
                (Mathf.PerlinNoise1D(t + positionOffset.y) - 0.5f) * positionMultiplier.y,
                (Mathf.PerlinNoise1D(t + positionOffset.z) - 0.5f) * positionMultiplier.z
            );
            
            var rotationNoise = s * new Vector3(
                (Mathf.PerlinNoise1D(t + rotationOffset.x) - 0.5f) * rotationMultiplier.x,
                (Mathf.PerlinNoise1D(t + rotationOffset.y) - 0.5f) * rotationMultiplier.y,
                (Mathf.PerlinNoise1D(t + rotationOffset.z) - 0.5f) * rotationMultiplier.z
            );

            var scaleNoise = Vector3.one + s * new Vector3(
                (Mathf.PerlinNoise1D(t + scaleOffset.x) - 0.5f) * scaleMultiplier.x,
                (Mathf.PerlinNoise1D(t + scaleOffset.y) - 0.5f) * scaleMultiplier.y,
                (Mathf.PerlinNoise1D(t + scaleOffset.z) - 0.5f) * scaleMultiplier.z
            );
            
            if ((applyMode & ApplyMode.Position) == ApplyMode.Position) {
                if (useLocal) transform.localPosition = positionNoise;
                else transform.position = positionNoise;        
            }
            
            if ((applyMode & ApplyMode.Rotation) == ApplyMode.Rotation) {
                if (useLocal) transform.localRotation = Quaternion.Euler(rotationNoise);
                else transform.rotation = Quaternion.Euler(rotationNoise);
            }
            
            if ((applyMode & ApplyMode.Scale) == ApplyMode.Scale) {
                transform.localScale = scaleNoise;
            }
        }
    }

}
