using MisterGames.Input.Global;

namespace MisterGames.Input.Bindings {

    public abstract class InputBindingKeyBase : InputBinding {

        public abstract KeyBinding[] GetKeys();

        public abstract bool IsActive();

    }

}