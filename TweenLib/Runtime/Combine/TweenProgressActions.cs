using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public class TweenProgressActions : ITweenProgressAction {

        [SerializeReference] [SubclassSelector] public List<ITweenProgressAction> actions;

        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnProgressUpdate(progress);
            }
        }
    }
    
}
