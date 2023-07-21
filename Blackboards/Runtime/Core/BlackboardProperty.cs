using System;
using MisterGames.Common.Types;

namespace MisterGames.Blackboards.Core {

    /// <summary>
    /// A struct to store blackboard property meta data: name and value type.
    /// </summary>
    [Serializable]
    public struct BlackboardProperty {

        /// <summary>
        /// The name of the blackboard property.
        /// It is used as display name and to calculate blackboard property hash via <see cref="Blackboard.StringToHash"/>
        /// </summary>
        public string name;

        /// <summary>
        /// The type of the blackboard property value.
        /// </summary>
        public SerializedType type;

        /// <summary>
        /// Index of the blackboard map used to store blackboard property value.
        /// </summary>
        public int mapIndex;
    }

}
