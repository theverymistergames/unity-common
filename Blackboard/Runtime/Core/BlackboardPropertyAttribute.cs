using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [AttributeUsage(validOn: AttributeTargets.Field)]
    public class BlackboardPropertyAttribute : PropertyAttribute {

        public readonly string pathToBlackboard;

        public BlackboardPropertyAttribute(string pathToBlackboard) {
            this.pathToBlackboard = pathToBlackboard;
        }
    }

}
