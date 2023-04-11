using System;
using MisterGames.BlueprintLib.Fsm;
using MisterGames.Character.Core2;

namespace MisterGames.BlueprintLib.Character {

    [Serializable]
    public sealed class CharacterAccessDynamicData : IDynamicData {

        public CharacterAccess characterAccess;
    }

}
