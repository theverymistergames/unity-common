using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardReference {
        [SerializeReference] public object value;
    }

}
