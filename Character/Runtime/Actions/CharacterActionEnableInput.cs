using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionEnableInput : ICharacterAction {

        public bool isEnabled;
        public bool isViewEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var inputPipeline = characterAccess.GetPipeline<ICharacterInputPipeline>();
            inputPipeline.IsEnabled = isEnabled;
            inputPipeline.EnableViewInput(isViewEnabled);
            return default;
        }
    }
    
}
