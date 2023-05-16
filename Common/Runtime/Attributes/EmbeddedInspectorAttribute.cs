using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EmbeddedInspectorAttribute : PropertyAttribute { }

}
