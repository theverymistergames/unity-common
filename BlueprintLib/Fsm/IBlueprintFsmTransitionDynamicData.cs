using System;

namespace MisterGames.BlueprintLib.Fsm {

    public interface IDynamicData {}

    public interface IBlueprintFsmTransitionDynamicData {
        Type DataType { get; }
        IDynamicData Data { set; }
    }

}
