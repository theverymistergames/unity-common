using System;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;

namespace MisterGames.Character.MotionFsm {
    
    [Serializable]
    public sealed class CharacterActionMotionFsmPipelineEnableDisable : ICharacterAction {

        public bool isEnabled;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.GetPipeline<ICharacterMotionFsmPipeline>().SetEnabled(isEnabled);
        }
    }
    
}
