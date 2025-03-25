using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetMaterialColorProgressAction : ITweenProgressAction {

        public Material material;
        [HashIdUsage(HashMethod.Shader)] public HashId fieldName;
        [ColorUsage(showAlpha: true, hdr: true)] public Color startColor;
        [ColorUsage(showAlpha: true, hdr: true)] public Color endColor;
        [Range(0f, 1f)] public float threshold;

        public void OnProgressUpdate(float progress) {
            material.SetColor(fieldName, progress <= threshold ? startColor : endColor);
            
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(material);
#endif
        }
    }

}
