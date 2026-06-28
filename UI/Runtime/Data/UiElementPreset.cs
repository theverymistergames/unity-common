using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.UI.Data;
using UnityEngine;

namespace MisterGames.UI.Components {

    [CreateAssetMenu(fileName = nameof(UiElementPreset), menuName = "MisterGames/UI/" + nameof(UiElementPreset))]
    public sealed class UiElementPreset : ScriptableObject {
    
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
        
        public void GetStateData(UiElementState state, out UiElementStateData data) {
            data = default;
            
            switch (state) {
                case UiElementState.Default:
                    data.duration = defaultDuration;
                    data.curve = defaultCurve;
                    data.imageColor = defaultColorImage;
                    data.textColor = defaultColorText;
                    data.scale = defaultScale;
                    break;
                
                case UiElementState.Hover:
                    data.duration = hoverDuration;
                    data.curve = hoverCurve;
                    data.imageColor = hoverColorImage;
                    data.textColor = hoverColorText;
                    data.scale = hoverScale;
                    break;
                
                case UiElementState.Selected:
                    data.duration = selectDuration;
                    data.curve = selectCurve;
                    data.imageColor = selectColorImage;
                    data.textColor = selectColorText;
                    data.scale = selectScale;
                    break;
                
                case UiElementState.Pressed:
                    data.duration = pressDuration;
                    data.curve = pressCurve;
                    data.imageColor = pressColorImage;
                    data.textColor = pressColorText;
                    data.scale = pressScale;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
    
}