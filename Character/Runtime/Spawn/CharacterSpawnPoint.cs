using UnityEngine;

namespace MisterGames.Character.Spawn {
    
    public class CharacterSpawnPoint : MonoBehaviour {

        [SerializeField] private Vector3 _offset;

        private Transform _transform;
        
        private void Awake() {
            _transform = transform;
        }

        public Vector3 GetPoint() {
            return _transform.position + _offset;
        }
        
    }
    
}