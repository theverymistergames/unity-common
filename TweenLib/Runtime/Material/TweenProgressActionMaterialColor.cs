using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMaterialColor : ITweenProgressAction {

        public Renderer renderer;
        public string fieldName;
        [ColorUsage(showAlpha: true, hdr: true)] public Color startColor;
        [ColorUsage(showAlpha: true, hdr: true)] public Color endColor;
        [Range(0f, 1f)] public float threshold;

        public void OnProgressUpdate(float progress) {
            var material = renderer.material;

            if (material == renderer.sharedMaterial) {
                material = new Material(renderer.sharedMaterial);
                renderer.material = material;
            }

            material.SetColor(fieldName, progress <= threshold ? startColor : endColor);
            
#if UNITY_EDITOR
            if (!Application.isPlaying && renderer != null) UnityEditor.EditorUtility.SetDirty(renderer);
#endif
        }
    }

}
