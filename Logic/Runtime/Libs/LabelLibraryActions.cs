using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryActions), menuName = "MisterGames/Libs/" + nameof(LabelLibraryActions))]
    public sealed class LabelLibraryActions : LabelLibraryByRef<IActorAction> { }
    
}