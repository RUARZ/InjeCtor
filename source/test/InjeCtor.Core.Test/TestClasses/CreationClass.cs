using InjeCtor.Core.Attribute;
using InjeCtor.Core.Test.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    public abstract class AbractCreationBaseClass { }

    internal class CreationClass : AbractCreationBaseClass
    {
        public CreationClass()
        {

        }

        public CreationClass(IGreeter greeter)
        {
            Greeter = greeter;
        }

        public CreationClass(ICalculator calc)
        {
            Calculator = calc;
        }

        public CreationClass(IGreeter greeter, ICalculator calculator)
        {
            Greeter = greeter;
            Calculator = calculator;
        }

        public IGreeter Greeter { get; }
        public ICalculator Calculator { get; }
    }

    internal class CreationClassWithSomeDefaultPara : CreationClass
    {
        public CreationClassWithSomeDefaultPara(IGreeter greeter, ICalculator calculator, object obj, bool bit, int number)
            : base(greeter, calculator)
        {
            Obj = obj;
            Bit = bit;
            Number = number;
        }

        public bool Bit { get; } = true;

        public object Obj { get; } = new object();

        public int Number { get; } = 42;
    }

    internal class CreationClassWithSomeDefaultParaAndNullable : CreationClass
    {
        public CreationClassWithSomeDefaultParaAndNullable(IGreeter greeter, ICalculator calculator, object? obj, bool? bit, int number = 13)
            : base(greeter, calculator)
        {
            Obj = obj;
            Bit = bit;
            Number = number;
        }

        public bool? Bit { get; } = true;

        public object? Obj { get; } = new object();

        public int Number { get; } = 42;
    }

    class NotMappedClass
    {
        public NotMappedClass(ICalculator calculator, IGreeter greeter)
        {
            Calculator = calculator;
            Greeter = greeter;
        }

        public ICalculator Calculator { get; }
        public IGreeter Greeter { get; }
    }

    class NotMappedClassWithAnotherNotMappedClassForCtor
    {
        public NotMappedClassWithAnotherNotMappedClassForCtor(NotMappedClass otherClass)
        {
            OtherClass = otherClass;
        }

        public NotMappedClass OtherClass { get; }
    }

    class NotMappedClassWithInjections
    {
        public NotMappedClassWithInjections()
        {
        }

        [Inject]
        public ICalculator Calculator { get; set; }
        public IGreeter Greeter { get; set; }
    }

    abstract class NotMappedAbstractClass { }

    interface INotMappedInterface { }
}
