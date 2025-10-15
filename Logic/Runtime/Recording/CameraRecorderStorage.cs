using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Logic.Recording {

    [CreateAssetMenu(fileName = nameof(CameraRecorderStorage), menuName = "MisterGames/CameraRecorder/" + nameof(CameraRecorderStorage))]
    public sealed class CameraRecorderStorage : ScriptableSingleton<CameraRecorderStorage> {

        public List<Entry> recordings = new();
        public List<Entry> backup = new();
        
        [Serializable]
        public struct Entry {
            public string saveName;
            public List<CameraRecorder.Data> data;   
        }
    }
    
}