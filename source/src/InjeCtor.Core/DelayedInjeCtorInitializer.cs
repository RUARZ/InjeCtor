using InjeCtor.Core.Builder;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace InjeCtor.Core
{
    /// <summary>
    /// Implementation of <see cref="IInjeCtor"/> which initializes it on first request of <see cref="IInjeCtor"/> api.
    /// </summary>
    public class DelayedInjeCtorInitializer : IInjeCtor
    {
        #region Private Fields

        private readonly IInjeCtorBuilder mBuilder;
        private Lazy<IInjeCtor> mInjeCtor;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DelayedInjeCtorInitializer"/> and set's its implementation for build
        /// the <see cref="IInjeCtor"/> instance if requested.
        /// </summary>
        /// <param name="builder">Implementation of <see cref="IInjeCtorBuilder"/> to use for creating the <see cref="IInjeCtor"/> instance
        /// when requested.</param>
        public DelayedInjeCtorInitializer(IInjeCtorBuilder builder)
        {
            mBuilder = builder;
            mInjeCtor = new Lazy<IInjeCtor>(() => mBuilder.Build());
        }

        #endregion

        #region IInjeCtor

        /// <inheritdoc/>
        public ITypeMapper Mapper => mInjeCtor.Value.Mapper;

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider => mInjeCtor.Value.MappingProvider;

        /// <inheritdoc/>
        public ITypeInformationProvider TypeInformationProvider => mInjeCtor.Value.TypeInformationProvider;

        /// <inheritdoc/>
        public ITypeInformationBuilder? TypeInformationBuilder => mInjeCtor.Value.TypeInformationBuilder;

        /// <inheritdoc/>
        public IScope CreateScope()
        {
            return mInjeCtor.Value.CreateScope();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            mInjeCtor.Value.Dispose();
        }

        /// <inheritdoc/>
        public T Get<T>()
        {
            return mInjeCtor.Value.Get<T>();
        }

        /// <inheritdoc/>
        public object? Get(Type type)
        {
            return mInjeCtor.Value.Get(type);
        }

        /// <inheritdoc/>
        public IEnumerable<IScope> GetScopes()
        {
            return mInjeCtor.Value.GetScopes();
        }

        /// <inheritdoc/>
        public object? Invoke<TObj>(TObj obj, Expression<Func<TObj, Delegate>> expression, params object?[] parameters)
        {
            return mInjeCtor.Value.Invoke(obj, expression, parameters);
        }

        /// <inheritdoc/>
        public object? Invoke(Expression<Func<Delegate>> expression, params object?[] parameters)
        {
            return mInjeCtor.Value.Invoke(expression, parameters);
        }

        #endregion
    }
}
