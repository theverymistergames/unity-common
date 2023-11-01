using UnityEngine;

namespace MisterGames.Blueprints.Factory {

    public static class BlueprintFactories {

        public static IBlueprintFactory Global {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) _globalFactory ??= new BlueprintFactory();
#endif
                return _globalFactory;
            }

            internal set {
                _globalFactory = value;
            }
        }

        private static IBlueprintFactory _globalFactory;
    }

}
