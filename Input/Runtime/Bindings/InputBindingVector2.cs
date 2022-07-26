using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [CreateAssetMenu(fileName = nameof(InputBindingVector2), menuName = "MisterGames/Input/Binding/" + nameof(InputBindingVector2))]
    public class InputBindingVector2 : InputBindingVector2Base {
        
        [SerializeField] private AxisBinding _axis;
        
        public override void Init() { }

        public override void Terminate() { }

        protected override Vector2 GetVector() {
            return _axis.GetValue();
        }
        
    }

}