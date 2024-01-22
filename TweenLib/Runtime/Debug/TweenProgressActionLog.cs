using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class TweenProgressActionLog : ITweenProgressAction {

        public string text;

        public void Initialize(MonoBehaviour owner) {
            Debug.LogWarning($"Using {nameof(TweenProgressActionLog)} on GameObject {owner.name}. " +
                             $"This class is for debug purposes only, " +
                             $"don`t forget to remove it in release mode.");
        }

        public void DeInitialize() { }

        public void Start() {

        }

        public void Finish() { }

        public void OnProgressUpdate(float progress) {
            Debug.Log($"text: {text}, progress: {progress}");
        }
    }
    
}
