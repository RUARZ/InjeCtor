using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core
{
    /// <summary>
    /// The core / kernel methods.
    /// </summary>
    public interface IInjeCtor : ICreator, IDisposable
    {
        /// <summary>
        /// The mapper to create the <see cref="ITypeMapping"/>s.
        /// </summary>
        public ITypeMapper Mapper { get; }

        /// <summary>
        /// Creates a new <see cref="IScope"/>.
        /// </summary>
        /// <returns>The new created <see cref="IScope"/>.</returns>
        public IScope CreateScope();

        /// <summary>
        /// The implementation of <see cref="ITypeInformationProvider"/> used to get informations for created types to inject properties after creation.
        /// </summary>
        public ITypeInformationProvider TypeInformationProvider { get; }

        /// <summary>
        /// Implementation of <see cref="ITypeInformationBuilder"/> to use to add type informations. May be <see langword="null"/>!
        /// </summary>
        public ITypeInformationBuilder? TypeInformationBuilder { get; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IScope"/> of all created scopes which are not disposed yet except the default scope!
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of all <see cref="IScope"/> which were created manually but not disposed yet.</returns>
        public IEnumerable<IScope> GetScopes();
    }
}
