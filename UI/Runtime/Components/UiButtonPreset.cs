using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.UI.Components {

    [CreateAssetMenu(fileName = nameof(UiButtonPreset), menuName = "MisterGames/UI/" + nameof(UiButtonPreset))]
    public sealed class UiButtonPreset : ScriptableObject {
    
        [Header("Click")]
        [Min(0f)] public float clickCooldown = 0.1f;

        [Header("Animation")]
        public bool selectOnHover = true;
        [Min(0f)] public float defaultDuration = 0.25f;
        [Min(0f)] public float hoverDuration = 0.25f;
        [Min(0f)] public float selectDuration = 0.25f;
        [Min(0f)] public float pressDuration = 0.25f;
        public AnimationCurve defaultCurve = EasingType.Linear.ToAnimationCurve();
        public AnimationCurve hoverCurve = EasingType.Linear.ToAnimationCurve();
        public AnimationCurve selectCurve = EasingType.Linear.ToAnimationCurve();
        public AnimationCurve pressCurve = EasingType.Linear.ToAnimationCurve();
        public float defaultScale = 1f;
        public float hoverScale = 1f;
        public float selectScale = 1f;
        public float pressScale = 1.1f;
        
        [Header("Image Colors")]
        public bool applyColorToImage;
        [VisibleIf(nameof(applyColorToImage))]
        public Color defaultColorImage = Color.white;
        [VisibleIf(nameof(applyColorToImage))]
        public Color hoverColorImage = Color.white;
        [VisibleIf(nameof(applyColorToImage))]
        public Color selectColorImage = Color.white;
        [VisibleIf(nameof(applyColorToImage))]
        public Color pressColorImage = Color.white;
        
        [Header("Text Colors")]
        public bool applyColorToText;
        [VisibleIf(nameof(applyColorToText))]
        public Color defaultColorText = Color.white;
        [VisibleIf(nameof(applyColorToText))]
        public Color hoverColorText = Color.white;
        [VisibleIf(nameof(applyColorToText))]
        public Color selectColorText = Color.white;
        [VisibleIf(nameof(applyColorToText))]
        public Color pressColorText = Color.white;
        
    }
    
}