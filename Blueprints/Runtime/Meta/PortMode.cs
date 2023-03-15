using System;

namespace MisterGames.Blueprints.Meta {

    [Flags]
    internal enum PortMode {
        None = 0,

        Input = 1,

        Data = 2,

        CapacitySingle = 4,
        CapacityMultiple = 8,

        LayoutLeft = 16,
        LayoutRight = 32,

        External = 64,
        Disabled = 128,
    }

}
