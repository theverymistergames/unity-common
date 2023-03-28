using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardValue<T> {
        public T value;
    }

}
