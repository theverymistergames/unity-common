using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct BlackboardReference {
        [SerializeReference] [SubclassSelector("type")]
        public object data;
        public SerializedType type;
    }

}
