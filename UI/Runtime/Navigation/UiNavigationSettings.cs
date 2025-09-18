using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.UI.Navigation {

    [CreateAssetMenu(fileName = nameof(UiNavigationSettings), menuName = "MisterGames/UI/" + nameof(UiNavigationSettings))]
    public sealed class UiNavigationSettings : ScriptableObject {

        [Header("Inputs")]
        public InputActionRef cancelInput;
        public InputActionRef moveInput;

        [Header("Navigation Scroll")]
        [Min(0f)] public float startSelectDelay = 0.4f;
        [Min(0f)] public float selectNextDelay0 = 0.25f;
        [Min(0f)] public float selectNextDelay1 = 0.1f;
        public AnimationCurve selectNextDelayCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
    
}