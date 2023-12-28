using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionEnableInput : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterInputPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
