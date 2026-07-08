using System;

namespace MisterGames.Scenes.Core {
    
    [Serializable]
    public struct SceneReference {
        
        public string scene;

        public SceneReference(string scene) {
            this.scene = scene;
        }
        
        public bool IsValid() => !string.IsNullOrWhiteSpace(scene);
        public override string ToString() => scene;
    }

}
