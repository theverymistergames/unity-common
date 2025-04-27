using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.UI.Services {
    
    public interface IFader {

        float Progress { get; }
        
        /// <summary>
        /// Fade in or out based on direction.
        /// </summary>
        public void Fade(FadeMode mode, float duration = -1f, AnimationCurve curve = null);

        /// <summary>
        /// Fade current fader color to alpha = 1.
        /// </summary>
        public void FadeIn(float duration = -1f, AnimationCurve curve = null);

        /// <summary>
        /// Fade current fader color to alpha = 0.
        /// </summary>
        public void FadeOut(float duration = -1f, AnimationCurve curve = null);

        /// <summary>
        /// Fade in or out based on direction, awaitable.
        /// </summary>
        public UniTask FadeAsync(FadeMode mode, float duration = -1f, AnimationCurve curve = null);

        /// <summary>
        /// Fade current fader color to alpha = 1, awaitable.
        /// </summary>
        public UniTask FadeInAsync(float duration = -1f, AnimationCurve curve = null);

        /// <summary>
        /// Fade current fader color to alpha = 0, awaitable.
        /// </summary>
        public UniTask FadeOutAsync(float duration = -1f, AnimationCurve curve = null);
    }
    
}