namespace MisterGames.Common.Inputs {

    public interface IGamepadHidColors {

        void Register(object source, int priority);
        void Unregister(object source);

        void SetState(object source, GamepadHidColorState state);

        void PlayAnimation(object source, GamepadHidColorAnimationAsset animation);
        void StopAnimation(object source);
        bool IsAnimationPlaying(object source);
    }

}
