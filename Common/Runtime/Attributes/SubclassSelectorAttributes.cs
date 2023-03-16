using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubclassSelectorIgnoreAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute { }



}
