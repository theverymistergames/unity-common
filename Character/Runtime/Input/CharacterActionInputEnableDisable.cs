using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionInputEnableDisable : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterInputPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
