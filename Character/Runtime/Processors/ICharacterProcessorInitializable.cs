using MisterGames.Character.Access;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorInitializable {
        void Initialize(ICharacterAccess characterAccess);
        void DeInitialize();
    }

}
