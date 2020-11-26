// Copyright (c) James Jackson-South.
// See LICENSE for more details.

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Provides enumeration of UDI type.
    /// </summary>
    public enum UdiType
    {
        /// <summary>
        /// An open UDI.
        /// </summary>
        Open,

        /// <summary>
        /// A closed UDI with a GUID id.
        /// </summary>
        ClosedGuid,

        /// <summary>
        /// A closed UDI with a string id.
        /// </summary>
        ClosedString
    }
}
