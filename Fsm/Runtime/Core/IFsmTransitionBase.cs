namespace MisterGames.Fsm.Core {

    public interface IFsmTransitionBase {
        void Arm(IFsmTransitionCallback callback);
        void Disarm();
    }

}
