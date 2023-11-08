using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardValue<T> {

        public T value;

        public BlackboardValue(T value) {
            this.value = value;
        }
    }

}
