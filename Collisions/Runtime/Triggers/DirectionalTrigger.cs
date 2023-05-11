using System;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public abstract class DirectionalTrigger : MonoBehaviour {

        public abstract event Action<GameObject> OnTriggeredForward;
        public abstract event Action<GameObject> OnTriggeredBackward;
    }

}
