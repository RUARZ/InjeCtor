using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InjeCtor.Core.Scope
{
    internal class Scope : IScope
    {
        #region Private Fields

        private IReadOnlyDictionary<Type, object>? mGlobalSingletons;
        private readonly ConcurrentDictionary<Type, object> mScopeSingletons = new ConcurrentDictionary<Type, object>();

        private readonly ICreator mCreator;

        #endregion

        #region Constructor

        public Scope(ICreator creator)
        {
            mCreator = creator;
        }

        #endregion

        #region IScope

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider { get; set; }
        
        /// <inheritdoc/>
        public ITypeInformationProvider? TypeInformationProvider { get; set; }

        /// <inheritdoc/>
        public object Create(Type type)
        {
            ITypeMapping? mapping = MappingProvider?.GetTypeMapping(type);

            if (mapping is null)
                throw new InvalidOperationException($"No mapping found for the type '{type.FullName}'!");

            if (mapping.MappedType is null)
                throw new InvalidOperationException($"The mapped type is null for '{mapping.SourceType.FullName}'!");

            object instance;
            switch (mapping.CreationInstruction)
            {
                case CreationInstruction.Always:
                    instance = mCreator.Create(type);
                    break;
                case CreationInstruction.Scope:
                    if (!mScopeSingletons.TryGetValue(type, out instance))
                    {
                        instance = mCreator.Create(type);
                        mScopeSingletons[type] = instance;
                    }
                    break;
                default:
                    throw new NotSupportedException($"The creation type '{mapping.CreationInstruction}' is not allowed to be handled by '{typeof(IScope).FullName}'!");
            }

            //TODO: further processing for maybe further injection depending in type informations!
            return instance;
        }

        /// <inheritdoc/>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (IDisposable disposable in mScopeSingletons.Values.Where(x => x is IDisposable))
            {
                disposable.Dispose();
            }

            mScopeSingletons.Clear();
        }

        /// <inheritdoc/>
        public object? GetSingleton<T>()
        {
            return GetSingleton(typeof(T));
        }

        /// <inheritdoc/>
        public object? GetSingleton(Type type)
        {
            if (mScopeSingletons.TryGetValue(type, out var singleton))
                return singleton;

            if (mGlobalSingletons?.TryGetValue(type, out singleton) == true)
                return singleton;

            return null;
        }

        /// <inheritdoc/>
        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {
            mGlobalSingletons = singletons;
        }

        #endregion
    }
}
