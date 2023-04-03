namespace MisterGames.Character.Core2 {

    public interface ICharacterPipeline {
        T GetProcessor<T>() where T : class;
        void SetEnabled(bool isEnabled);
    }

}
