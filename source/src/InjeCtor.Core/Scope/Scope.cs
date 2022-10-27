﻿using System.Runtime.CompilerServices;
using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

[assembly: InternalsVisibleTo("InjeCtor.Core.Test")]
namespace InjeCtor.Core.Scope
{
    internal class Scope : IScope
    {
        #region Private Fields

        private readonly TypeInformationInjector mTypeInformationInjector = new TypeInformationInjector();
        private IReadOnlyDictionary<Type, object>? mGlobalSingletons;
        private readonly ConcurrentDictionary<Type, object> mScopeSingletons = new ConcurrentDictionary<Type, object>();

        private IScopeAwareCreator? mCreator;

        #endregion

        #region IScope

        /// <inheritdoc/>
        public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;

        /// <inheritdoc/>
        public event EventHandler? Disposing;

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider { get; set; }

        /// <inheritdoc/>
        public IScopeAwareCreator? Creator
        {
            get => mCreator;
            set
            {
                if (mCreator != null)
                    mCreator.RequestSingletonCreationInstance -= MCreator_RequestSingletonCreationInstance;

                mCreator = value;

                if (mCreator != null)
                    mCreator.RequestSingletonCreationInstance += MCreator_RequestSingletonCreationInstance;
            }
        }

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

            if (Creator is null)
                throw new InvalidOperationException($"The instance of '{typeof(ICreator).FullName}' to use was not set!");

            object instance;
            switch (mapping.CreationInstruction)
            {
                case CreationInstruction.Always:
                    instance = Creator.Create(type, this);
                    InjectProperties(instance);
                    return instance;
                case CreationInstruction.Scope:
                    object? scopeSingleton = CreateScopeSingleton(type);

                    if (scopeSingleton is null)
                        throw new InvalidOperationException($"Failed to create scope singleton instance for type '{type}'!");

                    return scopeSingleton;
                case CreationInstruction.Singleton:
                    if (mGlobalSingletons?.TryGetValue(type, out object singletonInstance) == true)
                        return singletonInstance;

                    RequestSingletonCreationEventArgs args = new RequestSingletonCreationEventArgs(type);
                    RequestSingletonCreationInstance?.Invoke(this, args);

                    if (args.Instance is null)
                        throw new InvalidOperationException($"Failed to create global singleton for type '{type}'!");

                    // since this instance was created from a creation request no further processing needed!
                    return args.Instance;
                default:
                    throw new NotSupportedException($"The creation type '{mapping.CreationInstruction}' is not allowed to be handled by '{typeof(IScope).FullName}'!");
            }
        }

        /// <inheritdoc/>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

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

        #region Private Methods

        private void InjectProperties(object instance)
        {
            mTypeInformationInjector.InjectProperties(instance, TypeInformationProvider, mCreator, this);
        }

        private object? CreateScopeSingleton(Type type)
        {
            if (!mScopeSingletons.TryGetValue(type, out object? instance))
            {
                instance = Creator?.Create(type, this);

                if (instance != null && mScopeSingletons.TryAdd(type, instance))
                    InjectProperties(instance);
                else if (!mScopeSingletons.TryGetValue(type, out instance))
                    throw new InvalidOperationException($"Failed to add / get scope singleton for type '{type}'!");
            }

            return instance;
        }

        #endregion

        #region Request Singleton Event Handling

        private void MCreator_RequestSingletonCreationInstance(object sender, RequestSingletonCreationEventArgs e)
        {
            ITypeMapping? mapping = MappingProvider?.GetTypeMapping(e.Type);

            if (mapping is null)
                throw new InvalidOperationException($"No mapping found for type '{e.Type}'!");

            if (mapping.CreationInstruction == CreationInstruction.Singleton)
            {
                RequestSingletonCreationInstance?.Invoke(sender, e);
                return;
            }

            if (mapping.CreationInstruction == CreationInstruction.Always)
                throw new InvalidOperationException($"Singleton request for type '{e.Type}' with creation instruction '{nameof(CreationInstruction.Always)}' is not valid!");

            if (mScopeSingletons.TryGetValue(e.Type, out object? instance))
            {
                e.Instance = instance;
                return;
            }

            instance = Creator?.CreateDirect(e.Type, this);

            if (instance is null)
                throw new InvalidCastException($"Failed to create instance for '{e.Type}'!");

            if (!mScopeSingletons.TryAdd(e.Type, instance) && !mScopeSingletons.TryGetValue(e.Type, out instance))
                throw new InvalidOperationException($"Failed to add / get scope singleton for type '{e.Type}'!");
            else
                InjectProperties(instance);

            e.Instance = instance;
        }

        #endregion
    }
}
