using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintNodeMeta2 {

        /// <summary>
        /// Blueprint node id.
        /// </summary>
        public long nodeId;

        /// <summary>
        /// Blueprint node position.
        /// </summary>
        public Vector2 position;
    }

}
