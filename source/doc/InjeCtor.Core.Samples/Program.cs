using System;
using InjeCtor.Core;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Samples.Implementations;
using InjeCtor.Core.Samples.Interfaces;

namespace Samples
{
    public static class Program
    {
        static void Main(string[] args)
        {
            //MappingDirectlyOnInjeCtor();
            DynamicMappingResolve();
        }

        private static void MappingDirectlyOnInjeCtor()
        {
            using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor();

            injeCtor.Mapper.Add<IPlayer>().As<Player>();
            injeCtor.Mapper.Add<IUserInteraction>().AsSingleton<ConsoleUserInteraction>();
            injeCtor.Mapper.Add<IRequestContext>().AsScopeSingleton<RequestContext>();

            IUserInteraction userInteraction = injeCtor.Create<IUserInteraction>();

            userInteraction.Inform("Hello World");

            string response = userInteraction.Ask<string>("Shut down application (y/n)?");

            if (response == "y")
                return;

            int value = userInteraction.GetInput<int>("Enter a number:");
            userInteraction.Inform($"Input number: {value}");

            IPlayer player = injeCtor.Create<IPlayer>();

            player.StartRunning();
            player.Hide();
            player.Attack();
            player.DecreaseHP(80);
            player.IncreaseHP(55);
            player.DecreaseHP(111);
            player.Hide();

            GetParametersForContext(injeCtor);
            SetResultForContext(injeCtor);

            IRequestContext context = injeCtor.Create<IRequestContext>();
            userInteraction.Inform($"Parameters: {string.Join(", ", context.Parameters)}");
            userInteraction.Inform($"Results: {string.Join(", ", context.ResultValues)}");
        }

        private static void DynamicMappingResolve()
        {
            DynamicTypeMapper mapper = new DynamicTypeMapper();
            mapper.Add<IPlayer>();
            mapper.Add<IUserInteraction>().AsSingleton();
            mapper.Add<IRequestContext>().AsScopeSingleton();
            mapper.Resolve();

            using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor(mapper);

            IUserInteraction userInteraction = injeCtor.Create<IUserInteraction>();

            userInteraction.Inform("Hello World");

            string response = userInteraction.Ask<string>("Shut down application (y/n)?");

            if (response == "y")
                return;

            int value = userInteraction.GetInput<int>("Enter a number:");
            userInteraction.Inform($"Input number: {value}");

            IPlayer player1 = injeCtor.Create<IPlayer>();
            IPlayer player2 = injeCtor.Create<IPlayer>();

            userInteraction.Inform($"Player1 and Player2 reference equals: {ReferenceEquals(player1, player2)}");

            player1.StartRunning();
            player1.Hide();
            player1.Attack();
            player1.DecreaseHP(80);
            player1.IncreaseHP(55);
            player1.DecreaseHP(111);
            player1.Hide();

            GetParametersForContext(injeCtor);
            SetResultForContext(injeCtor);

            IRequestContext context = injeCtor.Create<IRequestContext>();
            userInteraction.Inform($"Parameters: {string.Join(", ", context.Parameters)}");
            userInteraction.Inform($"Results: {string.Join(", ", context.ResultValues)}");
        }

        private static void GetParametersForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Create<IRequestContext>();

            context.Parameters = new object[] { "one", 2, 3, "four", 5.67 };
        }

        private static void SetResultForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Create<IRequestContext>();

            context.ResultValues = new object[] { "SUCCESS" };
        }
    }
}