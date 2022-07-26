using UnityEngine;

namespace MisterGames.Input.Activation {

    [CreateAssetMenu(fileName = nameof(KeyActivationStrategyRelease), menuName = "MisterGames/Input/Activation/" + nameof(KeyActivationStrategyRelease))]
    internal class KeyActivationStrategyRelease : KeyActivationStrategy {
        
        internal override void OnPressed() { }

        internal override void OnReleased() { 
            FireOnUse();
        }

        internal override void Interrupt() { }

        internal override void OnUpdate(float dt) { }
        
    }

}