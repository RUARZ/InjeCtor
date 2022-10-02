using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace InjeCtor.Core.Resolve
{
    /// <summary>
    /// Resolver for getting suitable types for mapping from other assemblies.
    /// </summary>
    internal class TypeResolver
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="TypeResolver"/> and set's its <paramref name="typeToSearch"/> to define
        /// for which type a suitable type should be searched from assemblies.
        /// </summary>
        /// <param name="typeToSearch">The <see cref="Type"/> which should be searched./param>
        public TypeResolver(Type typeToSearch)
        {
            TypeToResolve = typeToSearch;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The type which is to resolve by this <see cref="TypeResolver"/>.
        /// </summary>
        public Type TypeToResolve { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tries to resolve the passed type and search a suitable type within <paramref name="assemblies"/>.
        /// Returns a <see cref="IReadOnlyList{T}"/> of the <see cref="Type"/>s which is suitable.
        /// </summary>
        /// <param name="assemblies">The <see cref="Assembly"/>s to search.</param>
        /// <returns>A <see cref="IReadOnlyList{T}"/> of the <see cref="Type"/>s which is suitable.</returns>
        public IEnumerable<Type> Resolve(params Assembly[] assemblies)
        {
            ConcurrentBag<Type> types = new ConcurrentBag<Type>();
            Parallel.ForEach(assemblies, a => SearchInAssembly(a, types));

            return types;
        }

        #endregion

        #region Private Methods

        private void SearchInAssembly(Assembly assembly, ConcurrentBag<Type> suitableTypes)
        {
            Parallel.ForEach(assembly.GetTypes(), t =>
            {
                if (TypeToResolve.IsAssignableFrom(t) && t != TypeToResolve)
                    suitableTypes.Add(t);
            });
        }

        #endregion
    }
}
