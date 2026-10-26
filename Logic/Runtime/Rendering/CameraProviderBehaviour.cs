using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public abstract class CameraProviderBehaviour : MonoBehaviour, ICameraProvider {
        public abstract Camera GetCamera();
    }
    
}