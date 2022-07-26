using UnityEngine;

namespace MisterGames.Input.Activation {

    [CreateAssetMenu(fileName = nameof(KeyActivationStrategyPress), menuName = "MisterGames/Input/Activation/" + nameof(KeyActivationStrategyPress))]
    internal class KeyActivationStrategyPress : KeyActivationStrategy {
        
        internal override void OnPressed() {
            FireOnUse();
        }

        internal override void OnReleased() { }

        internal override void Interrupt() { }

        internal override void OnUpdate(float dt) { }
        
    }

}