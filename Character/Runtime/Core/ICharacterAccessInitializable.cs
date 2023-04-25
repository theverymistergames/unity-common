namespace MisterGames.Character.Core {

    public interface ICharacterAccessInitializable {
        void Initialize(ICharacterAccess characterAccess);
        void DeInitialize();
    }

}
