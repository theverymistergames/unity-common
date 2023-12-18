using System;

namespace MisterGames.Character.Core {

    public interface ICharacterAccessRegistry {

        event Action<CharacterAccess> OnRegistered;

        event Action<CharacterAccess> OnUnregistered;

        CharacterAccess GetCharacterAccess();

        void Register(CharacterAccess characterAccess);

        void Unregister(CharacterAccess characterAccess);
    }

}
