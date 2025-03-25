using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Logic.Rendering;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Materials {

    [Serializable]
    public sealed class SetCustomPassMaterialFloatProgressAction : ITweenProgressAction {

        public CustomPassVolumeMaterialInstance customPassVolumeMaterialInstance;
        [HashIdUsage(HashMethod.Shader)] public HashId fieldName;
        public float startValue;
        public float endValue;

        public void OnProgressUpdate(float progress) {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            customPassVolumeMaterialInstance.Material.SetFloat(fieldName, Mathf.Lerp(startValue, endValue, progress));
        }
    }

}
