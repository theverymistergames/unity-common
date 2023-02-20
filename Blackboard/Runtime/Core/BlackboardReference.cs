using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public struct BlackboardReference {
        [SerializeReference] [SubclassSelector("type")]
        public object data;
        public SerializedType type;
    }

}
