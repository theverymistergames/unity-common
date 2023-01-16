using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintLink {

        public int nodeId;
        public int portIndex;
        public int portSignature;

        public override string ToString() {
            return $"{nodeId}::{portIndex}";
        }
    }

}
