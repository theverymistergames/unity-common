using UnityEngine;

namespace MisterGames.Input.Core {

    [CreateAssetMenu(fileName = nameof(InputScheme), menuName = "MisterGames/Input/" + nameof(InputScheme))]
    public sealed class InputScheme : ScriptableObject {

        [SerializeField] private InputActionBase[] _inputActions;
        public InputActionBase[] InputActions => _inputActions;
    }

}
