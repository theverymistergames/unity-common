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
        /// Port type: data-based port if flag is set, otherwise flow-based port.
        /// </summary>
        Data = 2,

        CapacitySingle = 4,
        CapacityMultiple = 8,

        LayoutLeft = 16,
        LayoutRight = 32,
    }
}
