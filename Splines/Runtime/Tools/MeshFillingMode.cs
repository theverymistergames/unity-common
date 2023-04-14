namespace MisterGames.Splines.Tools {

    /// <summary>
    /// The mode used by <see cref="SplineMeshBender"/> to bend meshes on the interval.
    /// </summary>
    public enum MeshFillingMode {

        /// <summary>
        /// In this mode, source mesh will be placed on the interval by preserving mesh scale.
        /// Vertices that are beyond interval end will be placed on the interval end.
        /// </summary>
        Once,

        /// <summary>
        /// In this mode, the mesh will be repeated to fill the interval, preserving
        /// mesh scale.
        /// This filling process will stop when the remaining space is not enough to
        /// place a whole mesh, leading to an empty interval.
        /// </summary>
        Repeat,

        /// <summary>
        /// In this mode, the mesh is deformed along the X axis to fill exactly the interval.
        /// </summary>
        StretchToInterval
    }

}
