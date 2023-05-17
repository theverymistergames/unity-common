using System;
using MisterGames.Common.Types;

namespace MisterGames.Common.Dependencies {

    [Serializable]
    public struct DependencyMeta {

        public string category;
        public string name;
        public SerializedType type;
        public int listIndex;
        public int elementIndex;
    }

}
