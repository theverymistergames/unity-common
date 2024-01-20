using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public class TweenProgressActions : ITweenProgressCallback {

        [SerializeReference] [SubclassSelector] public List<ITweenProgressCallback> actions;

        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnProgressUpdate(progress);
            }
        }
    }
    
}
