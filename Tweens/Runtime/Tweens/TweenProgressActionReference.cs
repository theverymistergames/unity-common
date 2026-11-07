using System;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class TweenProgressActionReference : ITweenProgressAction {

        public TweenProgressActionContainer tweenProgressActionContainer;
        
        public void OnProgressUpdate(float progress) {
            tweenProgressActionContainer.OnProgressUpdate(progress);
        }
    }
    
}