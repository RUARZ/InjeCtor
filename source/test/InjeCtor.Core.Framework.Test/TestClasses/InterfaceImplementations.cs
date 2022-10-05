using InjeCtor.Core.Test.Interfaces;

namespace InjeCtor.Core.Framework.Test.TestClasses
{
    class Calculator : ICalculator
    {
        public int Add(int number1, int number2)
        {
            return number1 + number2;
        }

        public int Divide(int number1, int number2)
        {
            return number1 / number2;
        }

        public int Multiply(int number1, int number2)
        {
            return (number1 * number2);
        }

        public int Subtract(int number1, int number2)
        {
            return number1 - number2;
        }
    }

    class Greeter : IGreeter
    {
        public string Greet(string name)
        {
            return $"Greetings to '{name}'!";
        }
    }
}
