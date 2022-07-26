using MisterGames.Common.Maths;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [CreateAssetMenu(fileName = nameof(InputBindingVector2Key), menuName = "MisterGames/Input/Binding/" + nameof(InputBindingVector2Key))]
    public class InputBindingVector2Key : InputBindingVector2Base {

        [SerializeField] private KeyBinding _positiveX;
        [SerializeField] private KeyBinding _negativeX;
        
        [SerializeField] private KeyBinding _positiveY;
        [SerializeField] private KeyBinding _negativeY;
        
        public override void Init() { }

        public override void Terminate() { }

        protected override Vector2 GetVector() {
            return new Vector2(
                _positiveX.IsActive().ToInt() - _negativeX.IsActive().ToInt(),
                _positiveY.IsActive().ToInt() - _negativeY.IsActive().ToInt()
            );
        }

    }

}