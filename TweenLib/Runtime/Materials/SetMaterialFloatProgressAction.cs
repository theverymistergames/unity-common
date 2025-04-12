using System;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetMaterialFloatProgressAction : ITweenProgressAction {

        public Material material;
        public ShaderHashId fieldName;
        public float startValue;
        public float endValue;

        public void OnProgressUpdate(float progress) {
            material.SetFloat(fieldName, Mathf.Lerp(startValue, endValue, progress));
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(material);
#endif
        }
    }

}
