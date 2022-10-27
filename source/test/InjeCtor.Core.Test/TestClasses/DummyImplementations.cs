﻿using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    class DummyTypeMapping : ITypeMapping
    {
        public Type SourceType { get; set; }

        public Type? MappedType { get; set; }

        public CreationInstruction CreationInstruction { get; set; }

        public object? Instance { get; set; }
    }

    class DummyTypeMapper : ITypeMapper
    {
        private CreationInstruction? mInstruction;

        private Dictionary<string, object> mDirectSingletons = new Dictionary<string, object>();

        public event EventHandler<MappingAddedEventArgs>? MappingAdded;

        public void SetCreationInstruction(CreationInstruction instruction)
        {
            mInstruction = instruction;
        }

        public void SetDirectSingleton(Type type, object instance)
        {
            mDirectSingletons.Add(type.FullName, instance);
        }

        public ITypeMapping<T> Add<T>()
        {
            DummyTypeMapping mapping = new DummyTypeMapping();
            mapping.SourceType = typeof(T);
            mapping.MappedType = typeof(T);
            MappingAdded?.Invoke(this, new MappingAddedEventArgs(mapping));
            return null;
        }

        public ITypeMapping? GetTypeMapping<T>()
        {
            return GetTypeMapping(typeof(T));
        }

        public ITypeMapping? GetTypeMapping(Type type)
        {
            DummyTypeMapping mapping = new DummyTypeMapping
            {
                SourceType = type,
            };

            string typeName = type.FullName;
            if (typeName == typeof(ICalculator).FullName)
                mapping.MappedType = typeof(Calculator);
            else if (typeName == typeof(IGreeter).FullName)
                mapping.MappedType = typeof(Greeter);
            else if (typeName == typeof(IScope).FullName)
                mapping.MappedType = typeof(DummyScope);
            else if (typeName == typeof(ClassWithoutMappedType).FullName)
                mapping.MappedType = null;
            else if (typeName == typeof(BaseClassForSingleton).FullName)
            {
                mapping.MappedType = typeof(SingletonClass);
                mapping.CreationInstruction = CreationInstruction.Singleton;
            }
            else if (typeName == typeof(BaseClassForNonDisposableSingleton).FullName)
            {
                mapping.MappedType = typeof(NonDisposableSingleton);
                mapping.CreationInstruction = CreationInstruction.Singleton;
            }
            else if (typeName == typeof(NotMappedClass).FullName)
                return null;

            if (mInstruction.HasValue)
                mapping.CreationInstruction = mInstruction.Value;

            if (mDirectSingletons.TryGetValue(typeName, out var instance))
                mapping.Instance = instance;

            return mapping;
        }

        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            throw new NotImplementedException();
        }
    }

    class DummyTypeInformationProvider : ITypeInformationProvider, ITypeInformationBuilder
    {
        private Dictionary<Type, Dictionary<Type, List<PropertyInfo>>>? mTypeInformations;
        private HashSet<Type> mAddedTypes = new HashSet<Type>();

        public void SetTypeInformations(Dictionary<Type, Dictionary<Type, List<PropertyInfo>>> typeInformations)
        {
            mTypeInformations = typeInformations;
        }

        public ITypeInformation? Get<T>()
        {
            return Get(typeof(T));
        }

        public ITypeInformation? Get(Type type)
        {
            if (mTypeInformations?.TryGetValue(type, out Dictionary<Type, List<PropertyInfo>> injectDict) != true)
                return null;

            Dictionary<Type, IReadOnlyList<PropertyInfo>> dictToUse = new Dictionary<Type, IReadOnlyList<PropertyInfo>>();
            foreach (KeyValuePair<Type, List<PropertyInfo>> kvp in injectDict)
            {
                dictToUse.Add(kvp.Key, kvp.Value.AsReadOnly());
            }

            ITypeInformation info = new DummyTypeInformation
            {
                Type = type,
                PropertiesToInject = dictToUse
            };

            return info;
        }

        public void Add<T>()
        {
            Add(typeof(T));
        }

        public void Add(Type type)
        {
            mAddedTypes.Add(type);
        }

        public HashSet<Type> AddedTypes => mAddedTypes;

        public bool AddPropertyInjection<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            throw new NotImplementedException();
        }
    }

    class DummyTypeInformation : ITypeInformation
    {
        public Type Type { get; set; }
        public IReadOnlyDictionary<Type, IReadOnlyList<PropertyInfo>> PropertiesToInject { get; set; }
    }

    class DummyCreator : IScopeAwareCreator
    {
        public ITypeMappingProvider? MappingProvider { get; set; }

        public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;

        public object Create(Type type, IScope? scope)
        {
            string typeName = type.FullName;
            if (typeName == typeof(ICalculator).FullName)
                return new Calculator();
            else if (typeName == typeof(IGreeter).FullName)
                return new Greeter();
            else if (typeName == typeof(IScope).FullName)
                return new DummyScope();
            else if (typeName == typeof(BaseClassForSingleton).FullName)
                return new SingletonClass();
            else if (typeName == typeof(BaseClassForNonDisposableSingleton).FullName)
                return new NonDisposableSingleton();
            else if (typeName == typeof(NotMappedClass).FullName)
                return new NotMappedClass(new Calculator(), new Greeter());

            throw new InvalidOperationException($"unknown type '{typeName}'!");
        }

        public T Create<T>(IScope? scope)
        {
            return (T)Create(typeof(T), scope);
        }

        public object Create(Type type)
        {
            return Create(type, null);
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T), null);
        }

        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {
            CreatedSingletons = singletons;
        }

        public object CreateDirect(Type type, IScope? scope)
        {
            return Create(type, scope);
        }

        public T CreateDirect<T>(IScope? scope)
        {
            return (T)Create(typeof(T), scope);
        }

        public IReadOnlyDictionary<Type, object> CreatedSingletons { get; private set; }
    }

    class DummyScope : IScope
    {
        private Dictionary<Type, object> mScopeSingletons = new Dictionary<Type, object>();

        public static int CreationCounter { get; private set; }
        public static int DisposeCounter { get; private set; }

        public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;
        public event EventHandler? Disposing;

        public static void ResetCounter()
        {
            CreationCounter = 0;
            DisposeCounter = 0;
        }

        public DummyScope()
        {
            CreationCounter++;
        }

        public ITypeInformationProvider? TypeInformationProvider { get; set; }
        public IScopeAwareCreator? Creator { get; set; }
        public ITypeMappingProvider? MappingProvider { get; set; }

        public object Create(Type type)
        {
            var mapping = MappingProvider.GetTypeMapping(type);
            if (mapping is null)
                return Creator.Create(type);

            bool isSingleton = mapping.CreationInstruction == CreationInstruction.Singleton;

            object instance;
            if (!isSingleton)
            {
                instance = Creator.Create(type);
            }
            else
            {
                RequestSingletonCreationEventArgs args = new RequestSingletonCreationEventArgs(type);
                RequestSingletonCreationInstance?.Invoke(this, args);
                instance = args.Instance;
            }

            return instance;
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public void Dispose()
        {
            DisposeCounter++;
        }

        public object? GetSingleton<T>()
        {
            return null;
        }

        public object? GetSingleton(Type type)
        {
            return null;
        }

        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {

        }
    }
}
