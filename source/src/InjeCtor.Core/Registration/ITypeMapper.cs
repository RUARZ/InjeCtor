using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// <typeparam name="T">The type which should be mapped.</typeparam>
        /// <returns>A <see cref="ITypeMapping{TDef}"/> instance for further configuration.</returns>
        ITypeMapping<T> Add<T>();
    }

    /// <summary>
    /// Interface for a dynamic type mapper which allows to add type mappings and try to resolve them later.
    /// </summary>
    public interface IDynamicTypeMapper
    {
        /// <summary>
        /// Add's a new mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be mapped.</typeparam>
        /// <returns>A <see cref="IDynamicTypeMapping{TDef}"/> instance for further configuration.</returns>
        IDynamicTypeMapping<T> Add<T>();

        /// <summary>
        /// Tries to resolve all mappings which does not have a mapped type set yet.
        /// </summary>
        /// <returns><see langword="True"/> if all matched type was found, otherwise <see langword="false"/>.</returns>
        bool Resolve();

        /// <summary>
        /// Tries to resolve all mappings which does not have a mapped type set yet.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/>s to search a suitable mapped type.</param>
        /// <returns><see langword="True"/> if all matched type was found, otherwise <see langword="false"/>.</returns>
        bool Resolve(params Assembly[] assemblies);
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

        /// <summary>
        /// The set <see cref="CreationInstruction"/>.
        /// </summary>
        CreationInstruction CreationInstruction { get; }
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
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> As<T>() where T : TDef;

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Singleton"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> AsSingleton();

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Scope"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> AsScopeSingleton();

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Always"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> AsTransient();
    }

    /// <summary>
    /// Interface to allow setting a type mapping with a specific method.
    /// </summary>
    internal interface IMappedTypeSetableTypeMapping
    {
        /// <summary>
        /// Set's the mapped type to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type which should be used as mapped type.</param>
        void SetMappedType(Type type);
    }

    /// <summary>
    /// Type mapping with suppord of searching for implementations with try resolving the type in assemblies.
    /// But it's also supported to directly set a mapped type.
    /// </summary>
    /// <typeparam name="TDef">The type for which the mapping should get a mapped type.</typeparam>
    public interface IDynamicTypeMapping<TDef> : ITypeMapping<TDef>
    {
        /// <summary>
        /// Tries to resolve a mapped type for the source type in the app domain.
        /// </summary>
        /// <returns><see langword="True"/> if a matched type was found, otherwise <see langword="false"/>.</returns>
        bool Resolve();

        /// <summary>
        /// Tries to resolve a mapped type for the source type in the passed <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/>s to search a suitable mapped type.</param>
        /// <returns><see langword="True"/> if a matched type was found, otherwise <see langword="false"/>.</returns>
        bool Resolve(params Assembly[] assemblies);

        /// <summary>
        /// Set's the type which should be used for the mapping as mapped type.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <returns>Own instance for further processing.</returns>
        new IDynamicTypeMapping<TDef> As<T>() where T : TDef;

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Singleton"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        new IDynamicTypeMapping<TDef> AsSingleton();

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Scope"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        new IDynamicTypeMapping<TDef> AsScopeSingleton();

        /// <summary>
        /// Set's the <see cref="CreationInstruction"/> to <see cref="CreationInstruction.Always"/>;
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        new IDynamicTypeMapping<TDef> AsTransient();
    }
}
