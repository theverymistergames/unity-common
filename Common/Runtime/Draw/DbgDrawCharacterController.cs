using UnityEngine;

namespace MisterGames.Dbg.Draw {

    [ExecuteInEditMode]
    [RequireComponent(typeof(CharacterController))]
    public class DbgDrawCharacterController : MonoBehaviour {

        private CharacterController _controller;
        
        private void Awake() {
            _controller = GetComponent<CharacterController>();
        }

        private void OnDrawGizmos() {
            float height = _controller.height;
            var pos = transform.position;
            var center = _controller.center;
            float radius = _controller.radius;
            
            var top = pos + center + (height / 2f - radius) * Vector3.up;
            var bottom = pos + center - (height / 2f - radius) * Vector3.up;
            
            DbgCapsule.Create().Color(Color.green).From(top).To(bottom).Radius(radius).Step(0.01f).Draw();
        }
    }

}