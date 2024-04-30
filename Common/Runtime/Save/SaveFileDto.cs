using System;
using System.Collections.Generic;
using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    [Serializable]
    public sealed class SaveFileDto {
        [SerializeReference] public List<ISaveTable> tables;
    }
}