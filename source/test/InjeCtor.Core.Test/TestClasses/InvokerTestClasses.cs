using InjeCtor.Core.Test.Interfaces;

namespace InjeCtor.Core.Test.TestClasses
{
    class MethodInvocations
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

    static class StaticMethodInvocations
    {
        public static string? LastGreeting { get; set; }

        public static int Add(ICalculator calculation, int number1, int number2)
        {
            return calculation.Add(number1, number2);
        }

        public static int MultipleDifferentParameters(int number1, ICalculator calculator, string name, int number2, IGreeter greeter, int number3)
        {
            int result = calculator.Add(number1, number2);
            result = calculator.Multiply(result, number3);

            LastGreeting = greeter.Greet(name);

            return result;
        }
    }
}
