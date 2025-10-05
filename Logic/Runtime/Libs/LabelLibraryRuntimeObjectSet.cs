using System.Collections.Generic;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeObjectSet), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeObjectSet))]
    public sealed class LabelLibraryRuntimeObjectSet : LabelLibraryRuntime<HashSet<Object>> { }
    
}