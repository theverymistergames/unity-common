using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    public struct SaveRecord<T> {
        
        public T data;

        public SaveRecord(T data) {
            this.data = data;
        }
    }
    
    [Serializable]
    public struct SaveRecordByRef<T> {
        
        [SerializeReference] public T data;

        public SaveRecordByRef(T data) {
            this.data = data;
        }
    }
    
}