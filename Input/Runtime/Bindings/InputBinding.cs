using UnityEngine;

namespace MisterGames.Input.Bindings {

    public abstract class InputBinding : ScriptableObject {

        public abstract void Init();

        public abstract void Terminate();

    }

}