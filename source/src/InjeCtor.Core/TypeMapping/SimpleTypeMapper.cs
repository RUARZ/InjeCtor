using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace InjeCtor.Core.TypeMapping
{
    /// <summary>
    /// A Simple implementation of <see cref="ITypeMapper"/> which allows to add mappings for types.
    /// </summary>
    public class SimpleTypeMapper : ITypeMapper, ITypeMappingProvider
    {
        #region Private Fields

        private readonly TypeMappingList mMappingList = new TypeMappingList();

        #endregion

        #region ITypeMapper

        /// <inheritdoc/>
        public event EventHandler<MappingAddedEventArgs>? MappingAdded;

        /// <inheritdoc/>
        public ITypeMappingBuilder Add(Type type)
        {
            TypeMapping mapping = new TypeMapping(type);
            mapping.MappingChanged += Mapping_MappingChanged;
            return mMappingList.Add(mapping);
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> Add<T>()
        {
            TypeMapping<T> mapping = new TypeMapping<T>();
            mapping.MappingChanged += Mapping_MappingChanged;
            return mMappingList.Add(mapping);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            return mMappingList.GetFinishedTypeMappings().ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public ITypeMapping? GetTypeMapping<T>()
        {
            return mMappingList.GetMapping<T>();
        }

        /// <inheritdoc/>
        public ITypeMapping? GetTypeMapping(Type type)
        {
            return mMappingList.GetMapping(type);
        }

        /// <inheritdoc/>
        public void AddTransient<T>() where T : class
        {
            Add<T>().As<T>();
        }

        /// <inheritdoc/>
        public void AddTransient(Type type)
        {
            Add(type).As(type);
        }

        /// <inheritdoc/>
        public void AddScopeSingleton<T>() where T : class
        {
            Add<T>().AsScopeSingleton<T>();
        }

        /// <inheritdoc/>
        public void AddScopeSingleton(Type type)
        {
            Add(type).AsScopeSingleton(type);
        }

        /// <inheritdoc/>
        public void AddSingleton<T>() where T : class
        {
            Add<T>().AsSingleton<T>();
        }

        /// <inheritdoc/>
        public void AddSingleton(Type type)
        {
            Add(type).AsSingleton(type);
        }

        /// <inheritdoc/>
        public void AddSingleton<T>(T instance) where T : class
        {
            Add<T>().AsSingleton(instance);
        }

        /// <inheritdoc/>
        public void AddTransient(object instance)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            Add(instance.GetType()).AsSingleton(instance);
        }

        #endregion

        #region Event Handling

        private void Mapping_MappingChanged(object sender, EventArgs e)
        {
            if (!(sender is INotifyOnMappingChangedTypeMapping mapping))
                return;

            mapping.MappingChanged -= Mapping_MappingChanged;

            MappingAdded?.Invoke(this, new MappingAddedEventArgs(mapping));
        }

        #endregion
    }
}
