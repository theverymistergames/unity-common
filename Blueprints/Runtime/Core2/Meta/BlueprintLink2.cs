using System;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Struct that holds data for blueprint node port. It is used only in editor.
    /// </summary>
    [Serializable]
    public struct BlueprintLink2 {

        /// <summary>
        /// Blueprint node id.
        /// </summary>
        public long nodeId;

        /// <summary>
        /// Port index of the given blueprint node.
        /// </summary>
        public int port;

        public override string ToString() {
            return $"{nameof(BlueprintLink2)}(nodeId {nodeId}, port {port})";
        }
    }

}
