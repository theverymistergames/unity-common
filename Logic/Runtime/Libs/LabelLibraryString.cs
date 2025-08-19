using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryString), menuName = "MisterGames/Libs/" + nameof(LabelLibraryString))]
    public sealed class LabelLibraryString : LabelLibrary<string> { }
    
}