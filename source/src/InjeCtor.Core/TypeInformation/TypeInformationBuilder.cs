using InjeCtor.Core.Attribute;
using InjeCtor.Core.Expression;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
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
        public void Add<T>()
        {
            Add(typeof(T));
        }

        /// <inheritdoc/>
        public void Add(Type type)
        {
            TypeInformation info = GetTypeInformation(type);
            AddInjectPropertyInfos(info);
        }

        /// <inheritdoc/>
        public bool AddPropertyInjection<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            PropertyInfo? pInfo = ExpressionParser.GetPropertyInfo(expression);
            if (pInfo is null)
                return false;

            Type type = typeof(T);
            TypeInformation info = GetTypeInformation(type);

            info.AddPropertyInfoForType(pInfo.PropertyType, pInfo);
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

        #region Private 

        private void AddInjectPropertyInfos(TypeInformation info)
        {
            foreach (PropertyInfo pInfo in info.Type.GetProperties())
            {
                if (pInfo.GetCustomAttribute<Inject>() is null || !pInfo.CanWrite)
                    continue;

                info.AddPropertyInfoForType(pInfo.PropertyType, pInfo);
            }
        }

        private TypeInformation GetTypeInformation(Type type)
        {
            if (!mTypeInformations.TryGetValue(type, out ITypeInformation typeInformation))
            {
                typeInformation = new TypeInformation(type);
                mTypeInformations.TryAdd(type, typeInformation);
            }

            return (TypeInformation)typeInformation;
        }

        

        #endregion
    }
}
