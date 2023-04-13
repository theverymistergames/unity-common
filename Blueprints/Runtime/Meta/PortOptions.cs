using System;

namespace MisterGames.Blueprints.Meta {

    [Flags]
    internal enum PortOptions {

        None = 0,


        External = 1,


        Hidden = 2,

        /// <summary>
        /// Used only for dynamic output data port to allow connections to input data ports with subclass data type.
        /// </summary>
        AcceptSubclass = 4,
    }

}
