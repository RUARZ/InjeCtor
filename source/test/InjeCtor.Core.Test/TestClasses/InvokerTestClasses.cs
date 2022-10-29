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

        public int Subtract(ICalculator calculator, int a, int b)
        {
            LastCalculationResult = calculator.Subtract(a, b);
            return LastCalculationResult;
        }

        public int MultipleDifferentParameters(int number1, ICalculator calculator, string name, int number2, IGreeter greeter, int number3)
        {
            int result = calculator.Add(number1, number2);
            result = calculator.Multiply(result, number3);

            LastGreeting = greeter.Greet(name);

            return result;
        }
    }
}
