using UnityEngine;

namespace MisterGames.Common.Attributes {

    public enum ReadOnlyMode {
        Always,
        PlayModeOnly,
    }

    public class ReadOnlyAttribute : PropertyAttribute {
        public readonly ReadOnlyMode mode;

        public ReadOnlyAttribute(ReadOnlyMode mode = ReadOnlyMode.Always) {
            this.mode = mode;
        }
    }

    public class BeginReadOnlyGroupAttribute : PropertyAttribute {
        public readonly ReadOnlyMode mode;

        public BeginReadOnlyGroupAttribute(ReadOnlyMode mode) {
            this.mode = mode;
        }
    }

    public class EndReadOnlyGroupAttribute : PropertyAttribute {
        private readonly ReadOnlyMode _mode;
    }
    
}
