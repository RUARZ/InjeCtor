using InjeCtor.Core.Creation;
using InjeCtor.Core.Scope;
using System;
using System.Linq;
using System.Reflection;

namespace InjeCtor.Core.TypeInformation
{
    /// <summary>
    /// Class to handle the injection of properties due to <see cref="TypeInformation"/>s.
    /// </summary>
    internal class TypeInformationInjector
    {
        #region Public Methods

        /// <summary>
        /// Injects properties to the passed <paramref name="instance"/> if a <see cref="ITypeInformation"/> could be found for it.
        /// </summary>
        /// <param name="instance">The instance to inject the requested properties.</param>
        /// <param name="typeInformationProvider">Implementation of <see cref="ITypeInformationProvider"/> to check for <see cref="ITypeInformation"/>s.</param>
        /// <param name="creator">Implementation of <see cref="IScopeAwareCreator"/> to create instances to inject.</param>
        /// <param name="scope">Implementation of <see cref="IScope"/> to get scope singletons if requested.</param>
        public void InjectProperties(object? instance, ITypeInformationProvider? typeInformationProvider, IScopeAwareCreator? creator, IScope? scope)
        {
            if (instance == null || typeInformationProvider == null || creator is null)
                return;

            Type instanceType = instance.GetType();
            ITypeInformation? info = typeInformationProvider.Get(instanceType);
            if (info is null || info.PropertiesToInject?.Any() != true)
                return;

            foreach (var propertiesToInject in info.PropertiesToInject)
            {
                Type typeToInject = propertiesToInject.Key;
                foreach (PropertyInfo pInfo in propertiesToInject.Value)
                {
                    object? injectInstance = scope != null ?
                        creator.Create(typeToInject, scope) :
                        creator.Create(typeToInject);
                    pInfo.SetValue(instance, injectInstance);
                }
            }
        }

        #endregion
    }
}
