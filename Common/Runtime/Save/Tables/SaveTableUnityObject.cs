using System;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Object))]
    public sealed class SaveTableUnityObject : SaveTable<Object> { }

}
