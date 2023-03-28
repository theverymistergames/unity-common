using System;

namespace MisterGames.Blueprints.Meta {

    [Flags]
    internal enum PortMode {
        None = 0,

        /// <summary>
        /// Port direction: input if flag is set, otherwise output.
        /// </summary>
        Input = 1,

        /// <summary>
        /// Port mode: data if flag is set, otherwise flow.
        /// </summary>
        Data = 2,

        CapacitySingle = 4,
        CapacityMultiple = 8,

        LayoutLeft = 16,
        LayoutRight = 32,

        External = 64,
        Hidden = 128,

        /// <summary>
        /// Used only for dynamic output data port to allow connections to input data ports with subclass data type.
        /// </summary>
        AcceptSubclass = 256,
    }

}
