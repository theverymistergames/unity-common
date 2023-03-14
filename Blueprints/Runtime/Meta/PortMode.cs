using System;

namespace MisterGames.Blueprints.Meta {

    [Flags]
    internal enum PortMode {
        None = 0,

        Input = 1,

        CapacitySingle = 2,
        CapacityMultiple = 4,

        LayoutLeft = 8,
        LayoutRight = 16,

        External = 32,

        Disabled = 64,
    }

}
