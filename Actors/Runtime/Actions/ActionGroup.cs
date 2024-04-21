using System;
using MisterGames.Common.Actions;

namespace MisterGames.Actors.Actions {

    [Serializable]
    public sealed class ActionGroup : AsyncActionGroup<IActorAction, IActor>, IActorAction { }

}
