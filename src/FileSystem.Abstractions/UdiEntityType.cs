// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Primitives;

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Represents a known UDI entity type.
    /// </summary>
    public readonly struct UdiEntityType : IEquatable<UdiEntityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdiEntityType"/> struct.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <param name="value">The entity type value.</param>
        public UdiEntityType(UdiType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(nameof(value));
            }

            this.Type = type;
            this.Value = Uri.UnescapeDataString(value);
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public UdiType Type { get; }

        /// <summary>
        /// Gets the entity type value.
        /// </summary>
        public StringValues Value { get; }

        public static bool operator ==(UdiEntityType left, UdiEntityType right)
            => left.Equals(right);

        public static bool operator !=(UdiEntityType left, UdiEntityType right)
            => !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is UdiEntityType type && this.Equals(type);

        /// <inheritdoc/>
        public bool Equals(UdiEntityType other)
            =>
            this.Type == other.Type
            && this.Value.Equals(other.Value);

        /// <inheritdoc/>
        public override int GetHashCode()
            => HashCode.Combine(this.Type, this.Value);
    }
}
