using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintLink {

        public int nodeId;
        public int portIndex;

        public override string ToString() {
            return $"{nameof(BlueprintLink)}(nodeId = {nodeId}, portIndex = {portIndex})";
        }
    }

}
