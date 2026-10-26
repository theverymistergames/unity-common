using System;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    [Serializable]
    public sealed class CameraReference : ICameraProvider {

        public Camera camera;
        
        public Camera GetCamera() {
            return camera;
        }
    }
    
}