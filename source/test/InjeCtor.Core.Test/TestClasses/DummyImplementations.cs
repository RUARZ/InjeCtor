using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    class DummyTypeMapping : ITypeMapping
    {
        public Type SourceType { get; set; }

        public Type? MappedType { get; set; }

        public CreationInstruction CreationInstruction { get; set; }
    }

    class DummyTypeMapper : ITypeMapper
    {
        private CreationInstruction? mInstruction;

        public void SetCreationInstruction(CreationInstruction instruction)
        {
            mInstruction = instruction;
        }

        public ITypeMapping<T> Add<T>()
        {
            throw new NotImplementedException();
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

            if (mInstruction.HasValue)
                mapping.CreationInstruction = mInstruction.Value;

            return mapping;
        }

        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            throw new NotImplementedException();
        }
    }

    class DummyTypeInformationProvider : ITypeInformationProvider
    {
        public ITypeInformation? Get<T>()
        {
            return null;
        }

        public ITypeInformation? Get(Type type)
        {
            return null;
        }
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
            bool isSingleton = MappingProvider.GetTypeMapping(type).CreationInstruction == CreationInstruction.Singleton;

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
