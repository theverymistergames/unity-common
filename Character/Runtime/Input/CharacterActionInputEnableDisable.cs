using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using MisterGames.Character.Input;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionInputEnableDisable : ICharacterAction {

        public bool isEnabled;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.GetPipeline<ICharacterInputPipeline>().SetEnabled(isEnabled);
        }
    }
    
}
