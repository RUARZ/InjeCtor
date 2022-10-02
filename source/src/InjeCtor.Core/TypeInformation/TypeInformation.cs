using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InjeCtor.Core.TypeInformation
{
    internal class TypeInformation : ITypeInformation
    {
        #region Private Fields

        private readonly ConcurrentDictionary<Type, List<PropertyInfo>> mPropertiesToInject = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        #endregion

        #region Constructor

        public TypeInformation(Type type)
        {
            Type = type;
        }

        #endregion

        #region Public Methods

        public void AddPropertyInfoForType(Type type, PropertyInfo pInfo)
        {
            if (!mPropertiesToInject.TryGetValue(type, out List<PropertyInfo>? list))
            {
                list = new List<PropertyInfo>();
                mPropertiesToInject.TryAdd(type, list);
            }

            list.Add(pInfo);
        }

        #endregion

        #region ITypeInformation

        /// <inheritdoc/>
        public Type Type { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, IReadOnlyList<PropertyInfo>> PropertiesToInject => 
            mPropertiesToInject.ToDictionary(k => k.Key, v => (IReadOnlyList<PropertyInfo>)v.Value.AsReadOnly());

        #endregion
    }
}
