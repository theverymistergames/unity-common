using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeActorSet), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeActorSet))]
    public sealed class LabelLibraryRuntimeActorSet : LabelLibraryRuntime<HashSet<IActor>> { }
    
}