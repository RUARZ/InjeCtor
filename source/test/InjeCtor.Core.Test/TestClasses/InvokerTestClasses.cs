using InjeCtor.Core.Test.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    class MethodInvokations
    {
        public IGreeter Greeter { get; private set; }
        public string LastGreeting { get; private set; }
        public int LastCalculationResult { get; private set; }

        public void Greet(IGreeter greeter)
        {
            Greeter = greeter;
            LastGreeting = greeter.Greet("Herbert");
        }

        public void Add(ICalculator calcultator)
        {
            LastCalculationResult = calcultator.Add(10, 20);
        }

        public void Subtract(ICalculator calculator, int a, int b)
        {
            LastCalculationResult = calculator.Subtract(a, b);
        }
    }
}
