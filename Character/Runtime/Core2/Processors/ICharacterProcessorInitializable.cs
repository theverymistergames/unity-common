namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorInitializable {
        void Initialize(ICharacterAccess characterAccess);
        void DeInitialize();
    }

}
