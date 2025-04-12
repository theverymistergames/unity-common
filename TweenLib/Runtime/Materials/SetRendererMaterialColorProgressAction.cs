using System;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetRendererMaterialColorProgressAction : ITweenProgressAction {

        public Renderer renderer;
        public ShaderHashId fieldName;
        [ColorUsage(showAlpha: true, hdr: true)] public Color startColor;
        [ColorUsage(showAlpha: true, hdr: true)] public Color endColor;
        [Range(0f, 1f)] public float threshold;

        public void OnProgressUpdate(float progress) {
            var mat =
                
#if UNITY_EDITOR
                Application.isPlaying ? renderer.material : renderer.sharedMaterial;
#else
                renderer.material;
#endif
                    
            mat.SetColor(fieldName, progress <= threshold ? startColor : endColor);
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(mat);
#endif
        }
    }

}
