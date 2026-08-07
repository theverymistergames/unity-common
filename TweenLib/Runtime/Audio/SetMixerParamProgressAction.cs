using System;
using MisterGames.Common.Audio;
using MisterGames.Common.Service;
using MisterGames.Common.Stats;
using MisterGames.Tweens;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class SetMixerParamProgressAction : ITweenProgressAction {

        public Object source;
        public string param;
        public float startValue;
        public float endValue;
        public ModifierType modifierType = ModifierType.Max;
        
        public void OnProgressUpdate(float progress) {
            float value = Mathf.Lerp(startValue, endValue, progress);
            Services.Get<IAudioMixerService>().SetModifier(source, param, new ValueModifier(modifierType, value));
        }
    }
    
}