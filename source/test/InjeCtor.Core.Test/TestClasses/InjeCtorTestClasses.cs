using InjeCtor.Core.Attribute;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    class ClassWithoutMappedType { }

    abstract class BaseClassForSingleton
    {
        public bool IsDisposed { get; set; }
    }

    abstract class BaseClassForNonDisposableSingleton
    {
        public bool IsDisposed { get; set; }
    }

    class NonDisposableSingleton : BaseClassForNonDisposableSingleton
    {
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    class SingletonClass : BaseClassForSingleton, IDisposable
    {
        public static int CreationCounter { get; private set; }

        public static void ResetCounter() => CreationCounter = 0;

        public SingletonClass()
        {
            CreationCounter++;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    class ScopeCreationCounter
    {
        public static int CreationCounter { get; private set; }

        public ScopeCreationCounter()
        {
            CreationCounter++;
        }

        public static void ResetCounter() => CreationCounter = 0;
    }

    interface IDummyInterface { }

    class DummyClassWithInjectAttributes : IDummyInterface
    {
        public IGreeter Greeter { get; set; }

        [Inject]
        public IGreeter GreeterWithAttribute { get; set; }
    }

    class ScopeAndInjeCtorInjectionsCtor
    {
        public ScopeAndInjeCtorInjectionsCtor(IInjeCtor? injeCtor, IScope? scope)
        {
            Injector = injeCtor;
            Scope = scope;
        }

        public IInjeCtor? Injector { get; }

        public IScope? Scope { get; }
    }

    class ScopeAndInjeCtorInjectionsProperties : IDummyInterface
    {
        [Inject]
        public IInjeCtor? Injector { get; set; }

        [Inject]
        public IScope? Scope { get; set; }
    }
}
