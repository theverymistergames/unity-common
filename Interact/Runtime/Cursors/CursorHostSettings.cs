using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(CursorHostSettings), menuName = "MisterGames/UI/" + nameof(CursorHostSettings))]
    public sealed class CursorHostSettings : ScriptableObject {
    
        [Header("Cursor")]
        public bool enableCursorOverride = true;
        public CursorIcon initialCursorIcon;
        public bool isAlphaControlledByDistance = true;
        public AnimationCurve alphaByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Prompt")]
        [Min(-1)] public int learnCountToDisablePromptKeyboardMouse = 10;
        [Min(-1)] public int learnCountToDisablePromptGamepad = -1;
        [Min(0f)] public float learnCooldown = 3f;
        public LabelValue learnPromptSetting;
    }
    
}