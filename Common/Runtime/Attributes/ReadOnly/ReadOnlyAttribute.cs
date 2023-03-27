using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    public class ReadOnlyAttribute : PropertyAttribute {

        public readonly ReadOnlyMode mode;

        public ReadOnlyAttribute(ReadOnlyMode mode) {
            this.mode = mode;
        }

        public ReadOnlyAttribute() {
            mode = ReadOnlyMode.Always;
        }
    }

}
