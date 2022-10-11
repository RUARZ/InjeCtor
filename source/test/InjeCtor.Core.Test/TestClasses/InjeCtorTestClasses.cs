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
}
