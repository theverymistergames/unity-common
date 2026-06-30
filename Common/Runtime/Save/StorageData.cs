using System;

namespace MisterGames.Common.Save {
    
    public readonly struct StorageData {
        
        public readonly string storageId;
        public readonly DateTime changeTime;
        
        public StorageData(string storageId, DateTime changeTime) {
            this.storageId = storageId;
            this.changeTime = changeTime;
        }

        public override string ToString() {
            return $"{nameof(StorageData)}(id {storageId}, time {changeTime})";
        }
    }
    
}