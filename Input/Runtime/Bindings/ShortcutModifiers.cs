using System;

namespace MisterGames.Input.Bindings {
    
    [Flags]
    public enum ShortcutModifiers
    {
        /// <summary>
        ///   <para>No modifier keys.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>Alt key (or Option key on macOS).</para>
        /// </summary>
        Alt = 1,
        /// <summary>
        ///   <para>Control key on Windows and Linux. Command key on macOS.</para>
        /// </summary>
        Action = 2,
        /// <summary>
        ///   <para>Shift key.</para>
        /// </summary>
        Shift = 4,
        /// <summary>
        ///   <para>Marks that the Control key modifier is part of the key combination. Resolves to control key on Windows, macOS, and Linux.</para>
        /// </summary>
        Control = 8,
    }
    
}