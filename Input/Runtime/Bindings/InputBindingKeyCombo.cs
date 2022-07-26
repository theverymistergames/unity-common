using System.Linq;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [CreateAssetMenu(fileName = nameof(InputBindingKeyCombo), menuName = "MisterGames/Input/Binding/" + nameof(InputBindingKeyCombo))]
    public class InputBindingKeyCombo : InputBindingKeyBase {

        [SerializeField] private KeyBinding[] _keys;

        public override void Init() { }

        public override void Terminate() { }

        public override KeyBinding[] GetKeys() {
            return _keys;
        }
        
        public override bool IsActive() {
            return GetKeys().All(GlobalInput.IsActive);
        }
        
    }

}