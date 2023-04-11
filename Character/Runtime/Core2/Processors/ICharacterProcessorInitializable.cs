using MisterGames.Character.Core2.Access;

namespace MisterGames.Character.Core2.Processors {

    public interface ICharacterProcessorInitializable {
        void Initialize(ICharacterAccess characterAccess);
        void DeInitialize();
    }

}
