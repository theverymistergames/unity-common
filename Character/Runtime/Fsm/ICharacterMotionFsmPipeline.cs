namespace MisterGames.Character.Fsm {

    public interface ICharacterMotionFsmPipeline {
        void Register(object source);
        void Unregister(object source);

        void SetEnabled(object source, bool isEnabled);
    }

}
