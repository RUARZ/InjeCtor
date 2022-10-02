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

        private readonly ConcurrentDictionary<Type, ITypeMapping> mMappings = new ConcurrentDictionary<Type, ITypeMapping>();

        #endregion

        #region ITypeMapper

        /// <inheritdoc/>
        public ITypeMapping<T> Add<T>()
        {
            if (mMappings.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"The type '{typeof(T).Name}' was alread added!");

            TypeMapping<T> item = new TypeMapping<T>();
            item.MappingChanged += Item_TypeMappingChanged;

            return item;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            return mMappings.Values.ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public ITypeMapping? GetTypeMapping<T>()
        {
            if (mMappings.TryGetValue(typeof(T), out ITypeMapping regItem))
                return regItem;

            return null;
        }

        #endregion

        #region Event Handling

        private void Item_TypeMappingChanged(object sender, EventArgs e)
        {
            if (!(sender is INotifyOnMappingChangedTypeMapping mapping))
                return;

            mMappings.TryAdd(mapping.SourceType, mapping);
            mapping.MappingChanged -= Item_TypeMappingChanged;
        }

        #endregion
    }
}
