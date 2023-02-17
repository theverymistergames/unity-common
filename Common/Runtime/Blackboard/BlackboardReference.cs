using System;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct BlackboardReference {
        [SerializeReference] public object data;
    }

}
