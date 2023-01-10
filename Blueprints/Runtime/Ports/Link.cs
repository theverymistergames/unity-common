using System;

namespace MisterGames.Blueprints.Ports {

    [Serializable]
    public struct Link {

        public int nodeId;
        public int port;

        public override string ToString() {
            return $"{nodeId}::{port}";
        }
    }

}
