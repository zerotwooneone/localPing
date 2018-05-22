using System;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var program = new ProgramViewmodel();
            program.MessageObservable.Subscribe(Console.WriteLine);
            program.Configure();
            program.Start().Wait();

        }
    }
}
