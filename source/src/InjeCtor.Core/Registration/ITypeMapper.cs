using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Interface of a type mapper to add mappings for types to use.
    /// </summary>
    public interface ITypeMapper : ITypeMappingProvider
    {
        /// <summary>
        /// Add's a new mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be mapped.</typeparam>
        /// <returns>A <see cref="ITypeMapping{TDef}"/> instance for further configuration.</returns>
        ITypeMapping<T> Add<T>();

        /// <summary>
        /// Adds a new mapping for <typeparamref name="T"/> which is directly the type to use / create.
        /// Will be added with createion instruction <see cref="CreationInstruction.Always"/>.
        /// </summary>
        /// <typeparam name="T">The type to add as mapping and directly use to create.</typeparam>
        void AddTransient<T>() where T : class;

        /// <summary>
        /// Adds a new mapping for <typeparamref name="T"/> which is directly the type to use / create.
        /// Will be added with createion instruction <see cref="CreationInstruction.Scope"/>.
        /// </summary>
        /// <typeparam name="T">The type to add as mapping and directly use to create.</typeparam>
        void AddScopeSingleton<T>() where T : class;

        /// <summary>
        /// Adds a new mapping for <typeparamref name="T"/> which is directly the type to use / create.
        /// Will be added with createion instruction <see cref="CreationInstruction.Singleton"/>.
        /// </summary>
        /// <typeparam name="T">The type to add as mapping and directly use to create.</typeparam>
        void AddSingleton<T>() where T : class;

        /// <summary>
        /// Adds a new mapping for <typeparamref name="T"/> which is directly the type to use / create.
        /// Will be added with createion instruction <see cref="CreationInstruction.Singleton"/>.
        /// </summary>
        /// <typeparam name="T">The type to add as mapping and directly use to create.</typeparam>
        /// <param name="instance">The instance to use for the singleton.</param>
        void AddSingleton<T>(T instance) where T : class;
    }

    /// <summary>
    /// Interface for a dynamic type mapper which allows to add type mappings and try to resolve them later.
    /// </summary>
    public interface IDynamicTypeMapper : ITypeMapper
    {
        /// <summary>
        /// Add's a new mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be mapped.</typeparam>
        /// <returns>A <see cref="IDynamicTypeMapping{TDef}"/> instance for further configuration.</returns>
        new IDynamicTypeMapping<T> Add<T>();

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

        /// <summary>
        /// Get's the <see cref="ITypeMapping"/> instance for the passed <paramref name="type"/> if there was
        /// a mapping added!
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which a mapping should be retrieved, if possible.</param>
        /// <returns>The <see cref="ITypeMapping"/> entry if found, otherwise <see langword="null"/>!</returns>
        ITypeMapping? GetTypeMapping(Type type);

        /// <summary>
        /// Event for new added mapping.
        /// </summary>
        event EventHandler<MappingAddedEventArgs>? MappingAdded;
    }

    /// <summary>
    /// Event args for <see cref="ITypeMappingProvider.MappingAdded"/> with the added mapping.
    /// </summary>
    public class MappingAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="MappingAddedEventArgs"/>.
        /// </summary>
        /// <param name="mapping">The <see cref="ITypeMapping"/> which was added.</param>
        public MappingAddedEventArgs(ITypeMapping mapping)
        {
            Mapping = mapping;
        }

        /// <summary>
        /// The added <see cref="ITypeMapping"/>.
        /// </summary>
        public ITypeMapping Mapping { get; }
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
        /// The instance to use if the <see cref="SourceType"/> is requested. Only used if
        /// <see cref="CreationInstruction"/> is set to <see cref="CreationInstruction.Singleton"/>!
        /// </summary>
        object? Instance { get; }

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
        /// Set's the type which should be used for the mapping as mapped type with creation instruction <see cref="CreationInstruction.Always"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> As<T>() where T : TDef;

        /// <summary>
        /// Set's the type to use for the mapping and set's it's singleton to the passed <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <param name="instance">The instance of <typeparamref name="T"/> to use as singleton.</param>
        /// <returns>Own instance for furhter processing.</returns>
        ITypeMapping<TDef> AsSingleton<T>(T instance) where T : TDef;

        /// <summary>
        /// Set's the type which should be used for the mapping as mapped type with creation instruction <see cref="CreationInstruction.Singleton"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> AsSingleton<T>() where T : TDef;

        /// <summary>
        /// Set's the type which should be used for the mapping as mapped type with creation instruction <see cref="CreationInstruction.Scope"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <returns>Own instance for further processing.</returns>
        ITypeMapping<TDef> AsScopeSingleton<T>() where T: TDef;
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
        /// Set's the type which should be used for the mapping as mapped typewith creation instruction <see cref="CreationInstruction.Always"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        /// <returns>Own instance for further processing.</returns>
        new IDynamicTypeMapping<TDef> As<T>() where T : TDef;

        /// <summary>
        /// Sets the creation instruction to <see cref="CreationInstruction.Singleton"/>.
        /// </summary>
        /// <returns></returns>
        IDynamicTypeMapping<TDef> AsSingleton();

        /// <summary>
        /// Sets the creation instruction to <see cref="CreationInstruction.Scope"/>.
        /// </summary>
        /// <returns>Own instance for further processing.</returns>
        IDynamicTypeMapping<TDef> AsScopeSingleton();
    }
}
