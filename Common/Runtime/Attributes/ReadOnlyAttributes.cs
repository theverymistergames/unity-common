using UnityEngine;

namespace MisterGames.Common.Attributes {

    public enum ReadOnlyMode {
        Always,
        PlayModeOnly,
    }

    public class ReadOnlyAttribute : PropertyAttribute {

        public readonly ReadOnlyMode mode;

        public ReadOnlyAttribute(ReadOnlyMode mode) {
            this.mode = mode;
        }

        public ReadOnlyAttribute() {
            mode = ReadOnlyMode.Always;
        }
    }

    public class BeginReadOnlyGroupAttribute : PropertyAttribute {

        public readonly ReadOnlyMode mode;

        public BeginReadOnlyGroupAttribute(ReadOnlyMode mode) {
            this.mode = mode;
        }

        public BeginReadOnlyGroupAttribute() {
            mode = ReadOnlyMode.Always;
        }
    }

    public class EndReadOnlyGroupAttribute : PropertyAttribute { }
    
}
