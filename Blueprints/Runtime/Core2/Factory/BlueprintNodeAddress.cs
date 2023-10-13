using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Static class to create node address as long from two ints: source id and node id.
    /// </summary>
    internal static class BlueprintNodeAddress {

        /// <summary>
        /// Return long value that holds two passed int values of source id and node id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Pack(int sourceId, int nodeId) {
            return (long) sourceId << 32 | (uint) nodeId;
        }

        /// <summary>
        /// Parse long value into two int values of source id and node id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(long id, out int sourceId, out int nodeId) {
            sourceId = (int) (id >> 32);
            nodeId = (int) (id & 0xFFFFFFFFL);
        }
    }

}
