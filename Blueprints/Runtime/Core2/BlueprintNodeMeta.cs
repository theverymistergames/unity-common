using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node data that is used for:
    /// - Blueprint Editor operations;
    /// - Compilation of the runtime blueprint node instance with links to other runtime node instances.
    /// </summary>
    [Serializable]
    public sealed class BlueprintNodeMeta {

        /// <summary>
        /// Reference to the serializable blueprint node implementation,
        /// to be able to store serialized data inside node.
        /// </summary>
        [SerializeReference]
        public BlueprintNode node;

        /// <summary>
        /// Position of the blueprint node in the Blueprint Editor window.
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// Port array created by BlueprintNode.CreatePorts().
        /// </summary>
        public Port[] ports;
    }

}
