using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.MotionFsm;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionMotionFsmPipelineEnableDisable : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterMotionFsmPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
