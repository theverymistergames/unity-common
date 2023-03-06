namespace MisterGames.Fsm.Core {

    public interface IFsmTransition {
        void Arm(IFsmTransitionCallback callback);
        void Disarm();
    }

    public interface IFsmTransition<in T> : IFsmTransition where T : IFsmTransitionData {
        T Data { set; }
    }

}
