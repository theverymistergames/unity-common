using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Input;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableInput : IActorAction {

        public bool isEnabled;
        public bool isViewEnabled;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var inputPipeline = context.GetComponent<CharacterInputPipeline>();
            inputPipeline.enabled = isEnabled;
            inputPipeline.EnableViewInput(isViewEnabled);
            return default;
        }
    }
    
}
