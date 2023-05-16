using UnityEngine;

namespace MisterGames.Common.Attributes {

    public sealed class BeginReadOnlyGroupAttribute : PropertyAttribute {

        public readonly ReadOnlyMode mode;

        public BeginReadOnlyGroupAttribute(ReadOnlyMode mode) {
            this.mode = mode;
        }

        public BeginReadOnlyGroupAttribute() {
            mode = ReadOnlyMode.Always;
        }
    }

    public sealed class EndReadOnlyGroupAttribute : PropertyAttribute { }
}
