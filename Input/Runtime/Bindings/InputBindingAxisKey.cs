using MisterGames.Common.Maths;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [CreateAssetMenu(fileName = nameof(InputBindingAxisKey), menuName = "MisterGames/Input/Binding/" + nameof(InputBindingAxisKey))]
    public class InputBindingAxisKey : InputBindingAxisBase {

        [SerializeField] private KeyBinding _positive;
        [SerializeField] private KeyBinding _negative;
        
        public override void Init() { }

        public override void Terminate() { }

        public override float GetValue() {
            return _positive.IsActive().ToInt() - _negative.IsActive().ToInt();
        }
        
    }

}