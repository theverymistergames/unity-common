using System;
using MisterGames.Tweens;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class TweenProgressActionDecalFadeFactor : ITweenProgressAction {
        
        public DecalProjector[] projectors;
        public Vector2 fromTo;

        public void OnProgressUpdate(float progress) {
            float fadeFactor = Mathf.Lerp(fromTo.x, fromTo.y, progress);

            for (int i = 0; i < projectors.Length; i++) {
                projectors[i].fadeFactor = fadeFactor;
            }
        }
    }
    
}
