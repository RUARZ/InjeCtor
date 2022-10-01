namespace InjeCtor.Core.Test.Interfaces
{
    public interface ICalculator
    {
        int Add(int number1, int number2);
        int Subtract(int number1, int number2);
        int Multiply(int number1, int number2);
        int Divide(int number1, int number2);
    }

    public interface IGreeter
    {
        string Greet(string name);
    }
}
