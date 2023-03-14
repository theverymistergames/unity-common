using System;

namespace MisterGames.Fsm.Core {

    public interface IFsmTransition : IFsmTransitionBase {
        Type DataType { get; }
        IFsmTransitionData Data { set; }
    }

}
