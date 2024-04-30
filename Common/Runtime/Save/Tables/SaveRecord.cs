using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    public struct SaveRecord<T> {
        public string id;
        public int index;
        public T data;
    }
    
    [Serializable]
    public struct SaveRecordByRef<T> {
        public string id;
        public int index;
        [SerializeReference] public T data;
    }
    
}