using System;
using MisterGames.Character.Interactives;
using MisterGames.Common.Inputs;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Logic {
    
    [Serializable]
    public sealed class GamepadVibrationWeightTweenAction : ITweenProgressAction {

        public GamepadVibrationBehaviour gamepadVibrationBehaviour;
        [Range(0f, 1f)] public float left0;
        [Range(0f, 1f)] public float left1 = 1f;
        [Range(0f, 1f)] public float right0;
        [Range(0f, 1f)] public float right1 = 1f;

        public void OnProgressUpdate(float progress) {
            gamepadVibrationBehaviour.SetWeight(GamepadSide.Left, Mathf.Lerp(left0, left1, progress));
            gamepadVibrationBehaviour.SetWeight(GamepadSide.Right, Mathf.Lerp(right0, right1, progress));
        }
    }
    
}