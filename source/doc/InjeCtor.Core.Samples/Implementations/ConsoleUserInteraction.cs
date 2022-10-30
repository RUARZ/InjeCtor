using InjeCtor.Core.Samples.Interfaces;

namespace InjeCtor.Core.Samples.Implementations
{
    internal class ConsoleUserInteraction : IUserInteraction
    {
        public T Ask<T>(string question)
        {
            Console.WriteLine(new string('=', 10));
            Console.WriteLine(question);
            Console.WriteLine(new string('=', 10));
            string? input = Console.ReadLine();

            if (input == null)
                return default(T);

            return (T)Convert.ChangeType(input, typeof(T));
        }

        public T GetInput<T>(string message)
        {
            Console.WriteLine(message);
            string? input = Console.ReadLine();

            if (input == null)
                return default(T);

            return (T)Convert.ChangeType(input, typeof(T));
        }

        public void Inform(string message)
        {
            Console.WriteLine(new string('=', 10));
            Console.WriteLine(message);
            Console.WriteLine(new string('=', 10));
        }
    }
}
