using InjeCtor.Core.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Configuration.Test.Interfaces
{
    interface IInterfaceA
    {

    }

    interface IInterfaceB
    {

    }

    interface IInterfaceC
    {

    }

    interface IInterfaceWithInjection
    {
        IInterfaceA SomeImpl { get; }
    }

    class ImplA : IInterfaceA { }

    class ImplB : IInterfaceB { }

    class ImplC : IInterfaceC { }

    class DirectClass { }

    class ImplClassWithInjection : IInterfaceWithInjection
    {
        [Inject]
        public IInterfaceA SomeImpl { get; set; }
    }

    class ClassWithInjectionDirect
    {
        [Inject]
        public IInterfaceA Injected { get; set; }
    }
}
