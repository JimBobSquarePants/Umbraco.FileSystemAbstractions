// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;

namespace FileSystem.Abstractions
{
    /// <summary>
    /// A helper class for constructing encoded <see cref="UDI"/> from predefined entity types.
    /// </summary>
    public static class UdiHelper
    {
        /// <summary>
        /// Creates a UDI from a given entity type.
        /// </summary>
        /// <param name="type">The UDI type</param>
        /// <returns>The <see cref="Udi"/>.</returns>
        public static Udi CreateUdi(UdiEntityType type)
        {
            if (type.Type != UdiType.Open)
            {
                throw new ArgumentException(nameof(type));
            }

            return new Udi(type.Value);
        }

        /// <summary>
        /// Creates a UDI from a given entity type and GUID id.
        /// </summary>
        /// <param name="type">The UDI type</param>
        /// <param name="id">The entity id.</param>
        /// <returns>The <see cref="Udi"/>.</returns>
        public static Udi CreateUdi(UdiEntityType type, Guid id)
        {
            if (type.Type != UdiType.ClosedGuid)
            {
                throw new ArgumentException(nameof(type));
            }

            return new Udi(type.Value, id);
        }

        /// <summary>
        /// Creates a UDI from a given entity type and string id.
        /// </summary>
        /// <param name="type">The UDI type</param>
        /// <param name="id">The entity id.</param>
        /// <returns>The <see cref="Udi"/>.</returns>
        public static Udi CreateUdi(UdiEntityType type, string id)
        {
            if (type.Type != UdiType.ClosedString)
            {
                throw new ArgumentException(nameof(type));
            }

            return new Udi(type.Value, id);
        }
    }
}
