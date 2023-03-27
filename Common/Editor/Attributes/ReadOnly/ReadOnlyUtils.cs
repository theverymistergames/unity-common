using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    internal static class ReadOnlyUtils {

        public static bool IsDisabledGui(ReadOnlyMode readOnlyMode) => readOnlyMode switch {
            ReadOnlyMode.Always => true,
            ReadOnlyMode.PlayModeOnly => Application.isPlaying,
            _ => throw new NotImplementedException($"ReadOnly mode {readOnlyMode} is not supported for ReadOnly editor.")
        };
    }

}
