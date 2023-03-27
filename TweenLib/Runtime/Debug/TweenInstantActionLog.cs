using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenInstantActionLog : ITweenInstantAction {

        public string text;

        public void Initialize(MonoBehaviour owner) {
            Debug.LogWarning($"Using {nameof(TweenInstantActionLog)} on GameObject {owner.name}. " +
                             $"This class is for debug purposes only, " +
                             $"don`t forget to remove it in release mode.");
        }

        public void DeInitialize() { }

        public void InvokeAction() {
            Debug.Log(text);
        }
    }

}
