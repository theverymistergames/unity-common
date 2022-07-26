using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [CreateAssetMenu(fileName = nameof(InputBindingKey), menuName = "MisterGames/Input/Binding/" + nameof(InputBindingKey))]
    public class InputBindingKey : InputBindingKeyBase {

        [SerializeField] private KeyBinding _key;

        private KeyBinding[] _keys;

        private void OnValidate() {
            _keys = new[] { _key };
        }

        public override void Init() {
            _keys = new[] { _key };
        }

        public override void Terminate() { }

        public override KeyBinding[] GetKeys() {
            return _keys;
        }
        
        public override bool IsActive() {
            return _key.IsActive();
        }
        
    }

}