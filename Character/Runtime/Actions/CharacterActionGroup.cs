using System;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionGroup : AsyncActionGroup<ICharacterAccess, ICharacterAction> { }

}
