using System;

namespace MisterGames.BlueprintLib.Fsm {

    public interface IBlueprintFsmTransitionDynamicData {
        Type DataType { get; }
        object Data { set; }
    }

}
