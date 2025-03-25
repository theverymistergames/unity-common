using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetMaterialFloatProgressAction : ITweenProgressAction {

        [HashIdUsage(HashMethod.Shader)] public Material material;
        public HashId fieldName;
        public float startValue;
        public float endValue;

        public void OnProgressUpdate(float progress) {
            material.SetFloat(fieldName, Mathf.Lerp(startValue, endValue, progress));
            
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(material);
#endif
        }
    }

}
