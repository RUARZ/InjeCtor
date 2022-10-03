using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.TypeInformation
{
    /// <summary>
    /// Interface for providing / adding special type informations to mark what should be injected.
    /// </summary>
    public interface ITypeInformationProvider
    {
        /// <summary>
        /// Get's a <see cref="ITypeInformation"/> for <typeparamref name="T"/> and returns it. If no information found
        /// then <see langword="null"/> will be returned.
        /// </summary>
        /// <typeparam name="T">The type for which a information should be retrieved.</typeparam>
        /// <returns>The found <see cref="ITypeInformation"/> or <see langword="null"/> if no info could be found.</returns>
        ITypeInformation? Get<T>();

        /// <summary>
        /// Get's a <see cref="ITypeInformation"/> for <paramref name="type"/> and returns it. If no information found
        /// then <see langword="null"/> will be returned.
        /// </summary>
        /// <returns>The found <see cref="ITypeInformation"/> or <see langword="null"/> if no info could be found.</returns>
        ITypeInformation? Get(Type type);
    }

    /// <summary>
    /// Interface for adding type Informations.
    /// </summary>
    public interface ITypeInformationBuilder
    {
        /// <summary>
        /// Add's information for the passed <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which informations should be added.</typeparam>
        void Add<T>();

        /// <summary>
        /// Add's information for the passed <paramref name="type"/>.
        /// </summary>
        void Add(Type type);

        /// <summary>
        /// Add's a property injection info for the type <typeparamref name="T"/> based on the passed expression <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">The type for which a property should be injected.</typeparam>
        /// <typeparam name="TProperty">The type of the property to be injected.</typeparam>
        /// <param name="expression">The expression to dertermine the property to inject.</param>
        /// <returns><see langword="True"/> if the <paramref name="expression"/> expression could be evaluated and the info could be created / appended, otherwise <see langword="false"/>.</returns>
        bool AddPropertyInjection<T, TProperty>(Expression<Func<T, TProperty>> expression);
    }

    /// <summary>
    /// Informations gathered or set for specific types about injections.
    /// </summary>
    public interface ITypeInformation
    {
        /// <summary>
        /// The type to which this information belongs.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// A dictionary of the Properties which are expecting a type to be injected.
        /// </summary>
        IReadOnlyDictionary<Type, IReadOnlyList<PropertyInfo>> PropertiesToInject { get; }
    }
}
