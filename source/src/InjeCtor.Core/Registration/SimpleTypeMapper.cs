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
        public ITypeMapping<T> Add<T>()
        {
            return mMappingList.Add(new TypeMapping<T>());
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

        #endregion
    }
}
