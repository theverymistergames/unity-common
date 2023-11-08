using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardReference {

        [SerializeReference] public object value;

        public BlackboardReference(object value) {
            this.value = value;
        }
    }

}
