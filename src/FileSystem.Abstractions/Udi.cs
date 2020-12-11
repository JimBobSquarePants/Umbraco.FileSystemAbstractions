// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Represents an Umbraco Document Identifier.
    /// <remarks>
    /// A UDI can be fully qualified or "closed" <c>umb://document/{id}</c>
    /// or "open" <c>umb://document</c>.
    /// </remarks>
    /// </summary>
    public readonly struct Udi : IEquatable<Udi>
    {
        private readonly Guid guid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Udi"/> struct.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        public Udi(string entityType)
            : this(new UdiEntityType(UdiType.Open, entityType), string.Empty, default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Udi"/> struct.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="id">The entity id.</param>
        public Udi(string entityType, string id)
            : this(new UdiEntityType(UdiType.ClosedString, entityType), id, default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Udi"/> struct.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="id">The entity id.</param>
        public Udi(string entityType, Guid id)
            : this(new UdiEntityType(UdiType.ClosedGuid, entityType), id.ToString("N"), id)
        {
        }

        private Udi(UdiEntityType entityType, string id, Guid guid)
        {
            if (entityType.Type != UdiType.Open && string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id));
            }

            this.EntityType = entityType;
            this.Id = Uri.EscapeDataString(id).Replace("%2F", "/");
            this.guid = guid;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public UdiEntityType EntityType { get; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        public StringValues Id { get; }

        public static bool operator ==(Udi left, Udi right)
            => left.Equals(right);

        public static bool operator !=(Udi left, Udi right)
            => !(left == right);

        /// <summary>
        /// Returns a value indicating whether the UDI is open.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsOpen() => this.EntityType.Type == UdiType.Open;

        /// <summary>
        /// Attempts to return a GUID from this instance.
        /// </summary>
        /// <param name="result">
        /// The structure that will contain the parsed value.
        /// If the method returns <see langword="true"/>, result contains a valid <see cref="Guid"/>.
        /// If the method returns <see langword="false"/>, result equals <see cref="Guid.Empty"/>.
        /// </param>
        /// <returns>The <see cref="bool"/>.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryGetGuid(out Guid result)
        {
            result = this.guid;
            if (this.EntityType.Type == UdiType.ClosedGuid)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the string representation of a UDI to the equivalent <see cref="Udi"/> structure.
        /// </summary>
        /// <param name="value">The UDI to convert.</param>
        /// <param name="result">
        /// The structure that will contain the parsed value.
        /// If the method returns <see langword="true"/>, result contains a valid <see cref="Udi"/>.
        /// If the method returns <see langword="false"/>, result equals <see cref="default"/>.
        /// </param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool TryParse(string value, out Udi result)
            => ParseImpl(value, true, out result);

        /// <summary>
        /// Returns the fully qualified URI of this instance.
        /// </summary>
        /// <returns>The <see cref="Uri"/>.</returns>
        public Uri ToUri() => new Uri(this.ToString());

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is Udi udi && this.Equals(udi);

        /// <inheritdoc/>
        public bool Equals(Udi other)
            => this.EntityType.Equals(other.EntityType)
            && this.Id.Equals(other.Id);

        /// <inheritdoc/>
        public override int GetHashCode()
            => HashCode.Combine(this.EntityType, this.Id);

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!this.IsOpen())
            {
                return $"umb://{this.EntityType.Value}/{this.Id}";
            }

            return $"umb://{this.EntityType.Value}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ParseImpl(string value, bool tryParse, out Udi result)
        {
            result = default;

            if (!Uri.IsWellFormedUriString(value, UriKind.Absolute)
                || !Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                if (tryParse)
                {
                    return false;
                }

                ThrowBadFormat(value);
                return false;
            }

            string entityType = null;
            if (uri.Segments.Length >= 2)
            {
                entityType = uri.Segments[1].Trim('/');
            }

            if (string.IsNullOrEmpty(entityType))
            {
                if (tryParse)
                {
                    return false;
                }

                ThrowBadFormat(value);
            }

            string path = null;
            if (uri.Segments.Length >= 3)
            {
                path = uri.Segments[2];
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (Guid.TryParse(path, out Guid id))
                {
                    result = new Udi(entityType, id);
                }
                else
                {
                    result = new Udi(entityType, path);
                }
            }
            else
            {
                result = new Udi(entityType);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowBadFormat(string value)
            => throw new FormatException($"Input string \"{value}\" is not a valid Udi.");
    }
}
