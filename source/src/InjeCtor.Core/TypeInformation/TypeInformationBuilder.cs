using InjeCtor.Core.Attribute;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace InjeCtor.Core.TypeInformation
{
    /// <summary>
    /// Implementation for <see cref="ITypeInformationBuilder"/> and <see cref="ITypeInformationProvider"/>
    /// </summary>
    public class TypeInformationBuilder : ITypeInformationBuilder, ITypeInformationProvider
    {
        #region Private Fields

        private readonly ConcurrentDictionary<Type, ITypeInformation> mTypeInformations = new ConcurrentDictionary<Type, ITypeInformation>();

        #endregion

        #region ITypeInformationBuilder

        /// <inheritdoc/>
        public bool Add<T>()
        {
            return Add(typeof(T));
        }

        /// <inheritdoc/>
        public bool Add(Type type)
        {
            if (mTypeInformations.ContainsKey(type))
                return false;

            mTypeInformations.TryAdd(type, CreateTypeInformation(type));

            return true;
        }

        #endregion

        #region ITypeInformationProvider

        /// <inheritdoc/>
        public ITypeInformation? Get<T>()
        {
            return Get(typeof(T));
        }

        /// <inheritdoc/>
        public ITypeInformation? Get(Type type)
        {
            if (mTypeInformations.TryGetValue(type, out ITypeInformation result))
                return result;

            return null;
        }

        #endregion

        #region Private Methods

        private ITypeInformation CreateTypeInformation(Type type)
        {
            TypeInformation info = new TypeInformation(type);
            AddInjectPropertyInfos(info);
            return info;
        }

        private void AddInjectPropertyInfos(TypeInformation info)
        {
            foreach (PropertyInfo pInfo in info.Type.GetProperties())
            {
                if (pInfo.GetCustomAttribute<Inject>() is null || !pInfo.CanWrite)
                    continue;

                info.AddPropertyInfoForType(pInfo.PropertyType, pInfo);
            }
        }

        #endregion
    }
}
