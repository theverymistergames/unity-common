using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Libs {

    [CreateAssetMenu(fileName = nameof(LabelLibraryAudioClips), menuName = "MisterGames/Libs/" + nameof(LabelLibraryAudioClips))]
    public sealed class LabelLibraryAudioClips : LabelLibrary<AudioClip[]> { }
    
}