using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintConnection {

        public int fromNodeId;
        public int fromPortIndex;
        public int fromPortHash;

        public int toNodeId;
        public int toPortIndex;
        public int toPortHash;

        public override string ToString() {
            return $"{fromNodeId}::{fromPortIndex} -> {toNodeId}::{toPortIndex}";
        }
    }

}
