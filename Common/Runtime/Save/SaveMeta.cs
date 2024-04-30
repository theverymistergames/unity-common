using System;

namespace MisterGames.Common.Save {
    
    public readonly struct SaveMeta {
        
        public readonly string id;
        public readonly DateTime time;
        
        public SaveMeta(string id, DateTime time) {
            this.id = id;
            this.time = time;
        }

        public override string ToString() {
            return $"{id} {time}";
        }
    }
    
}