﻿using System;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Struct that holds data for blueprint node port. It is used only in editor.
    /// </summary>
    [Serializable]
    public struct BlueprintLink2 {

        /// <summary>
        /// Node id, built from source id and node id.
        /// </summary>
        public NodeId id;

        /// <summary>
        /// Port index.
        /// </summary>
        public int port;

        public BlueprintLink2(NodeId id, int port) {
            this.id = id;
            this.port = port;
        }

        public override string ToString() {
            return $"{nameof(BlueprintLink2)}(source {id.source}, node {id.node}, port {port})";
        }
    }

}
