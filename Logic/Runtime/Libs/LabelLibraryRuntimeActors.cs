using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeActors), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeActors))]
    public sealed class LabelLibraryRuntimeActors : LabelLibraryRuntime<IActor> { }
    
}