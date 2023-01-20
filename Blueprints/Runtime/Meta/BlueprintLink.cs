using System;

namespace MisterGames.Blueprints.Meta {

    [Serializable]
    public struct BlueprintLink {

        public int nodeId;
        public int portIndex;

        public override string ToString() {
            return $"{nameof(BlueprintLink)}(nodeId = {nodeId}, portIndex = {portIndex})";
        }
    }

}
