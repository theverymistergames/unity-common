using System;
using UnityEngine;

namespace MisterGames.Input.Activation {

    internal abstract class KeyActivationStrategy : ScriptableObject {

        internal Action OnUse = delegate {  };

        internal abstract void OnPressed();

        internal abstract void OnReleased();

        internal abstract void Interrupt();

        internal abstract void OnUpdate(float dt);
        
        protected void FireOnUse() {
            OnUse.Invoke();
        }

    }

}