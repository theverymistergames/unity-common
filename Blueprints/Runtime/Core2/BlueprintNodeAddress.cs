using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Static class to create long value from two int values of factory id and node id
    /// and to parse long value into two int values of factory id and node id.
    /// </summary>
    public static class BlueprintNodeAddress {

        /// <summary>
        /// Return long value that holds two passed int values of factory id and node id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Create(int factoryId, int nodeId) {
            return (long) factoryId << 32 | (uint) nodeId;
        }

        /// <summary>
        /// Parse long value into two int values of factory id and node id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Parse(long id, out int factoryId, out int nodeId) {
            factoryId = (int) (id >> 32);
            nodeId = (int) (id & 0xFFFFFFFFL);
        }
    }

}
