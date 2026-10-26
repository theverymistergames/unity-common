using System;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    [Serializable]
    public sealed class CameraProviderReference : ICameraProvider {

        public CameraProviderBehaviour cameraProviderBehaviour;
        
        public Camera GetCamera() {
            return cameraProviderBehaviour.GetCamera();
        }
    }
    
}