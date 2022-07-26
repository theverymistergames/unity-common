﻿using UnityEngine;

namespace MisterGames.Input.Core {

    [CreateAssetMenu(fileName = nameof(InputScheme), menuName = "MisterGames/Input/" + nameof(InputScheme))]
    public sealed class InputScheme : ScriptableObject {

        [SerializeField] private InputAction[] _inputActions;
        public InputAction[] InputActions => _inputActions;

    }

}