using MisterGames.Common;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public class GravityTest : MonoBehaviour {
        
        [SerializeField] private Vector3 _gravity = new Vector3(0f, -9.81f, 0f);

        private void Update() {
            Physics.gravity = transform.TransformDirection(_gravity);
            DebugExt.DrawSphere(transform.position, 0.1f, Color.magenta);
            DebugExt.DrawRay(transform.position, transform.TransformDirection(_gravity), Color.magenta);
        }
    }
    
}