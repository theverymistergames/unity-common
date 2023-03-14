using MisterGames.Fsm.Core;

namespace MisterGames.BlueprintLib {

    internal interface IBlueprintNodeFsmTransition {
        void Arm(IFsmTransitionCallback callback);
        void Disarm();
    }

}
