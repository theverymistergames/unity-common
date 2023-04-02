namespace MisterGames.Character.Core2 {

    public interface ICharacterMotionPipeline {
        void SetEnabled(bool isEnabled);

        T GetInputProcessor<T>() where T : class, ICharacterProcessorVector2;
        T GetMotionProcessor<T>() where T : class, ICharacterProcessorVector3;
        T GetInputToMotionConverter<T>() where T : class, ICharacterProcessorVector2ToVector3;
    }

}
