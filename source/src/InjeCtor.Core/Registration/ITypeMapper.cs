using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Interface of a type mapper to add mappings for types to use.
    /// </summary>
    public interface ITypeMapper
    {
        /// <summary>
        /// Add's a new mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be registered to the mapper.</typeparam>
        /// <returns>A <see cref="ITypeMapping{TDef}"/> instance for further configuration.</returns>
        ITypeMapping<T> Add<T>();
    }

    /// <summary>
    /// interface for type mapping providers
    /// </summary>
    public interface ITypeMappingProvider
    {
        /// <summary>
        /// The informations for the mappings added to the type mapper.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyList{T}"/> of the created <see cref="ITypeMapping"/> instances.</returns>
        IReadOnlyList<ITypeMapping> GetTypeMappings();

        /// <summary>
        /// Get's the <see cref="ITypeMapping"/> instance for the type <typeparamref name="T"/> if there was
        /// a mapping added!
        /// </summary>
        /// <typeparam name="T">The type for which a <see cref="ITypeMapping"/> entry should be retrieved.</typeparam>
        /// <returns>The <see cref="ITypeMapping"/> entry if found, otherwise <see langword="null"/>!</returns>
        ITypeMapping? GetTypeMapping<T>();
    }

    /// <summary>
    /// Represents data for the type mapping.
    /// </summary>
    public interface ITypeMapping
    {
        /// <summary>
        /// The type which should be registered and for which other types should be created.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// The Type which should be used / created ... for <see cref="SourceType"/>.
        /// </summary>
        Type? MappedType { get; }
    }

    /// <summary>
    /// Interface for type mappings which notify on a mapped type changed.
    /// </summary>
    internal interface INotifyOnMappingChangedTypeMapping : ITypeMapping
    {
        /// <summary>
        /// Event thrown if mapping type changed.
        /// </summary>
        event EventHandler? MappingChanged;
    }

    /// <summary>
    /// Item for further definitions / settings.
    /// </summary>
    public interface ITypeMapping<TDef> : ITypeMapping
    {
        /// <summary>
        /// Set's the type which should be used for the mapping as mapped type.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        void As<T>() where T : TDef;
    }
}
