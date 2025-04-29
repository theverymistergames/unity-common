using System;

namespace MisterGames.Scenes.Core {
    
    [Serializable]
    public struct SceneReference {
        public string scene;

        public bool IsValid() {
            return !string.IsNullOrWhiteSpace(scene);
        }
    }

}
