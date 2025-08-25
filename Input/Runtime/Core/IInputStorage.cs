using System;
using System.Collections.Generic;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public interface IInputStorage {

        IReadOnlyList<InputActionMap> InputMaps { get; }
        
        InputAction GetInputAction(Guid guid);
        InputActionMap GetInputMap(Guid guid);
    }
    
}