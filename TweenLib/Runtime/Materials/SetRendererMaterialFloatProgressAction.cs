using System;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetRendererMaterialFloatProgressAction : ITweenProgressAction {

        public Renderer renderer;
        public ShaderHashId fieldName;
        public float startValue = 0;
        public float endValue;

        public void OnProgressUpdate(float progress) {
            var mat =
                
#if UNITY_EDITOR
                Application.isPlaying ? renderer.material : renderer.sharedMaterial;
#else
                renderer.material;
#endif
                    
            mat.SetFloat(fieldName, Mathf.Lerp(startValue, endValue, progress));
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(mat);
#endif
        }
    }

}
