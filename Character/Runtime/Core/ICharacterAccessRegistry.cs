using MisterGames.Actors;

namespace MisterGames.Character.Core {

    public interface ICharacterAccessRegistry {

        IActor GetCharacterAccess();

        void Register(IActor actor);

        void Unregister(IActor actor);
    }

}
