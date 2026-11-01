using System;
using MisterGames.Character.Interactives;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Logic {
    
    [Serializable]
    public sealed class CameraShakeWeightTweenAction : ITweenProgressAction {

        public CameraShakeBehaviour cameraShakeBehaviour;
        [Range(0f, 1f)] public float weight0;
        [Range(0f, 1f)] public float weight1 = 1f;

		public void OnProgressUpdate(float progress) {
			cameraShakeBehaviour.SetWeight(Mathf.Lerp(weight0, weight1, progress));
        }
    }
    
}