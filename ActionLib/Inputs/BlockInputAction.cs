using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class BlockInputAction : IActorAction {

        public Source source;
        [VisibleIf(nameof(source), 1)]
        public UnityEngine.Object host;
        public bool block = true;
        public bool unblockOnCancel = true;
        public InputMapRef[] maps;
        public InputActionRef[] overrideEnableInputActions;
        public InputActionRef[] overrideDisableInputActions;

        public enum Source {
            Actor,
            ExplicitHost,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            object source = this.source switch {
                Source.Actor => context,
                Source.ExplicitHost => host,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var blockToken = unblockOnCancel ? cancellationToken : CancellationToken.None;
            
            if (block) {
                InputServices.Blocks.BlockInputMaps(source, maps, blockToken);
                InputServices.Blocks.SetInputActionBlockOverrides(source, overrideEnableInputActions, blocked: false, blockToken);
                InputServices.Blocks.SetInputActionBlockOverrides(source, overrideDisableInputActions, blocked: true, blockToken);
            }
            else {
                InputServices.Blocks.UnblockInputMaps(source, maps);
                InputServices.Blocks.RemoveInputActionBlockOverrides(source, overrideEnableInputActions);
                InputServices.Blocks.RemoveInputActionBlockOverrides(source, overrideDisableInputActions);
            }
            
            return default;
        }
    }
    
}