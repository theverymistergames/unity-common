using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionEnableMotionGraph : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterMotionGraphPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
