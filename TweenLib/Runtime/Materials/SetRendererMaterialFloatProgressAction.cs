using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetRendererMaterialFloatProgressAction : ITweenProgressAction {

        public Renderer renderer;
        [HashIdUsage(HashMethod.Shader)] public HashId fieldName;
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
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(mat);
#endif
        }
    }

}
