using System;
using MisterGames.Common.Data;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public struct BlackboardProperty {


        public int hash;


        public string name;


        public SerializedType type;
    }

}
