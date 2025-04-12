using System;
using MisterGames.Common.Data;
using MisterGames.Logic.Rendering;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetCustomPassMaterialColorProgressAction : ITweenProgressAction {

        public CustomPassVolumeMaterialInstance customPassVolumeMaterialInstance;
        public ShaderHashId fieldName;
        [ColorUsage(showAlpha: true, hdr: true)] public Color startColor;
        [ColorUsage(showAlpha: true, hdr: true)] public Color endColor;
        [Range(0f, 1f)] public float threshold;

        public void OnProgressUpdate(float progress) {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            customPassVolumeMaterialInstance.Material.SetColor(fieldName, progress <= threshold ? startColor : endColor);
        }
    }

}
