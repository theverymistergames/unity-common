using System;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public abstract class Trigger : MonoBehaviour {

        public abstract event Action<Collider> OnTriggered;
    }

}
