using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeActorHashSet), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeActorHashSet))]
    public sealed class LabelLibraryRuntimeActorHashSet : LabelLibraryRuntime<HashSet<IActor>> { }
    
}