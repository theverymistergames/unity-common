using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryRuntimeActions), menuName = "MisterGames/Libs/" + nameof(LabelLibraryRuntimeActions))]
    public sealed class LabelLibraryRuntimeActions : LabelLibraryRuntime<IActorAction> { }
    
}