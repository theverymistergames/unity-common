using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyCustomGravity : MonoBehaviour, IUpdate {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private bool _useGravity;
        [SerializeField] private CustomGravity.Mode _gravityMode;

        [VisibleIf(nameof(_gravityMode), 2)]
        [SerializeField] private CustomGravitySource _localGravitySource;
        
        public bool UseGravity { get => _useGravity; set => SetGravityUsage(_gravityMode, value); }
        public CustomGravity.Mode GravityMode { get => _gravityMode; set => SetGravityUsage(value, _useGravity); }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            
        }

        private void SetGravityUsage(CustomGravity.Mode mode, bool useGravity) {
            _gravityMode = mode;
            _useGravity = useGravity;
            
            _rigidbody.useGravity = useGravity && mode == CustomGravity.Mode.Physics;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }  
#endif
    }
    
}