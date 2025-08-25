using System;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [Serializable]
    public struct InputActionFilter {
        
        [SerializeField] private InputActionRef[] _blockActions;
        [SerializeField] private InputActionRef[] _unblockActions;

        public void Apply(object source) {
            InputServices.Blocks.SetInputActionBlockOverrides(source, _blockActions, blocked: true);
            InputServices.Blocks.SetInputActionBlockOverrides(source, _unblockActions, blocked: false);
        }
        
        public void Release(object source) {
            InputServices.Blocks.RemoveInputActionBlockOverrides(source, _blockActions);
            InputServices.Blocks.RemoveInputActionBlockOverrides(source, _unblockActions);
        }
    }

}
