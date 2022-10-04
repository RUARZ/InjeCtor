using InjeCtor.Core.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Resolve
{
    /// <summary>
    /// Class for resolving the most suitable <see cref="ConstructorInfo"/> for creations of classes.
    /// </summary>
    internal class ConstructorResolver
    {
        #region Public Methods

        /// <summary>
        /// Tries to resolve the most suitable <see cref="ConstructorInfo"/> for <paramref name="type"/>.
        /// The 'most suitable' <see cref="ConstructorInfo"/> is the info with the most parameters that can be passed
        /// with the known mappings.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> which should be created.</param>
        /// <param name="mappingProvider">The <see cref="ITypeMappingProvider"/> to check for known / mapped types.</param>
        /// <returns>The found <see cref="ConstructorInfo"/> to use.</returns>
        public ConstructorInfo? ResolveConstructorInfo(Type type, ITypeMappingProvider mappingProvider)
        {
            List<ConstructorInfo> constructorInfos = GetConstructors(type);
            return FindMostSuitableConstructor(constructorInfos, mappingProvider);
        }

        #endregion

        #region Private Methods

        private List<ConstructorInfo>  GetConstructors(Type type)
        {
            List<ConstructorInfo> constructors = new List<ConstructorInfo>();

            foreach (ConstructorInfo info in type.GetConstructors())
            {
                if (info.IsStatic || !info.IsPublic)
                    continue;

                constructors.Add(info);
            }

            return constructors;
        }

        private ConstructorInfo? FindMostSuitableConstructor(List<ConstructorInfo> constructors, ITypeMappingProvider mappingProvider)
        {
            if (!constructors.Any())
                return null;
            if (constructors.Count == 1)
                return constructors[0];

            ConstructorInfo? currentInfo = null;
            int knownParameterTypeCount = 0;

            foreach (ConstructorInfo info in constructors)
            {
                int knownTypeCount = info.GetParameters().Select(x => x.ParameterType)
                    .Count(x => mappingProvider.GetTypeMapping(x) != null);

                if (knownTypeCount > knownParameterTypeCount)
                {
                    knownParameterTypeCount = knownTypeCount;
                    currentInfo = info;
                }
            }

            return currentInfo;
        }

        #endregion
    }
}
