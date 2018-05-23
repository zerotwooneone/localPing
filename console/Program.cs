using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using zh.LocalPing;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ProgramViewmodel>()
                .AddScoped<IPingService, PingService>()
                .AddScoped<IPingTaskFactory, PingTaskFactory>()
                .AddScoped<IPingResponseUtil, PingResponseUtil>()
                .BuildServiceProvider();


            var cancellationTokenSource = new CancellationTokenSource();
            void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
            {
                cancellationTokenSource.Cancel();
            }

            Console.CancelKeyPress += Console_CancelKeyPress;

            var program = serviceProvider.GetService<ProgramViewmodel>();
            program.MessageObservable.Subscribe(Console.WriteLine);
            program.Configure();
            program.Start(cancellationTokenSource.Token).Wait();
        }

        
    }
}
