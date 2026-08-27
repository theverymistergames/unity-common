using MisterGames.Input.Actions;
using MisterGames.Input.Icons;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(CursorIcon), menuName = "MisterGames/Interactives/" + nameof(CursorIcon))]
    public class CursorIcon : ScriptableObject {

        [Header("Icon")]
        public Sprite sprite;
        [ColorUsage(showAlpha: true)]
        public Color tint = Color.white;
        public Vector2 size = new(10f, 10f);

        [Header("Prompt")]
        public PromptMode showInteractionPrompt = PromptMode.ReplaceCursor;
        public bool disablePromptAfterLearn = true;
        public Vector2 promptSize = new(48f, 48f);
        public InputIconsTable iconsTable;
        public InputActionRef interactionAction;
        [Min(0)] public int interactionBindingMouse = 0;
        [Min(0)] public int interactionBindingGamepad = 1;

        public enum PromptMode {
            Disable,
            ShowAdditive,
            ReplaceCursor,
        }
    }

}
