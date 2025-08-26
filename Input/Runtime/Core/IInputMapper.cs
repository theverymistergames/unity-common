using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public interface IInputMapper {

        IReadOnlyList<InputActionMap> InputMaps { get; }
        
        InputAction GetInputAction(Guid guid);
        InputActionMap GetInputMap(Guid guid);
    }
    
}