using System.Collections.Generic;
using System.Threading;
using MisterGames.Input.Actions;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public interface IInputBlockService {

        bool IsInputActionEnabled(InputAction inputAction);
        bool IsInputMapEnabled(InputActionMap inputMap);

        bool BlockInputMap(object source, InputActionMap inputMap, CancellationToken cancellationToken = default);
        bool BlockInputMap(object source, InputMapRef inputMapRef, CancellationToken cancellationToken = default);
        bool BlockInputMaps(object source, IReadOnlyList<InputActionMap> inputMaps, CancellationToken cancellationToken = default);
        bool BlockInputMaps(object source, IReadOnlyList<InputMapRef> inputMapRefs, CancellationToken cancellationToken = default);

        bool UnblockInputMap(object source, InputActionMap inputMap);
        bool UnblockInputMap(object source, InputMapRef inputMapRef);
        bool UnblockInputMaps(object source, IReadOnlyList<InputActionMap> inputMaps);
        bool UnblockInputMaps(object source, IReadOnlyList<InputMapRef> inputMapRefs);

        bool ClearAllInputMapBlocksOf(object source);
        bool ClearAllInputMapBlocks();

        bool SetInputActionBlockOverride(object source, InputAction inputAction, bool blocked, CancellationToken cancellationToken = default);
        bool SetInputActionBlockOverride(object source, InputActionRef inputActionRef, bool blocked, CancellationToken cancellationToken = default);
        bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions, bool blocked, CancellationToken cancellationToken = default);
        bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs, bool blocked, CancellationToken cancellationToken = default);

        bool RemoveInputActionBlockOverride(object source, InputAction inputAction);
        bool RemoveInputActionBlockOverride(object source, InputActionRef inputActionRef);
        bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions);
        bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs);

        bool ClearAllInputActionBlockOverridesOf(object source);
        bool ClearAllInputActionBlockOverrides();

        void ClearAllBlocks();
    }
    
}