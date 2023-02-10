using System;
using MisterGames.Input.Global;

namespace MisterGames.Input.Bindings {

    [Serializable]
    public sealed class KeyCombo : IKeyBinding {

        public KeyBinding[] keys;

        public bool IsActive {
            get {
                for (int k = 0; k < keys.Length; k++) {
                    if (!keys[k].IsActive()) return false;
                }
                return true;
            }
        }
    }

}
