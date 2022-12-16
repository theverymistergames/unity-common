using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {
    
    [Serializable]
    public sealed class TweenProgressActionLog : ITweenProgressAction {

        [SerializeField] private string _text;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            Debug.Log($"{_text}. Progress: {progress}");
        }
    }
    
}
