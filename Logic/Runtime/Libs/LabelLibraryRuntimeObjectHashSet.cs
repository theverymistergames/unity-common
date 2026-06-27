using System.Collections.Generic;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeObjectHashSet), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeObjectHashSet))]
    public sealed class LabelLibraryRuntimeObjectHashSet : LabelLibraryRuntime<HashSet<Object>> { }
    
}