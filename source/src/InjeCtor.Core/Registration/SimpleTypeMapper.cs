using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace InjeCtor.Core.Registration
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
        public ITypeMapping<T> Add<T>()
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
