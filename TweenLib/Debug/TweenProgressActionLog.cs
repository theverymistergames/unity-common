using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class TweenProgressActionLog : ITweenProgressAction {

        public string text;

        private MonoBehaviour _owner;

        public void Initialize(MonoBehaviour owner) {
            _owner = owner;
        }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            Debug.LogWarning($"Using {nameof(TweenProgressActionLog)} on game object {_owner.name}, text: {text}, progress: {progress}");
        }
    }
    
}
