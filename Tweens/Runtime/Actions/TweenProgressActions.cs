using System;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {
    
    [Serializable]
    public class TweenProgressActions : ITweenProgressAction {

        [SerializeReference] [SubclassSelector] public ITweenProgressAction[] actions;
        
        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].DeInitialize();
            }
        }

        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].OnProgressUpdate(progress);
            }
        }
    }
    
}
