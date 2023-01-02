using System;
using InjeCtor.Core;
using InjeCtor.Core.Samples.Implementations;
using InjeCtor.Core.Samples.Interfaces;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeMapping;

namespace Samples
{
    public static class Program
    {
        static void Main(string[] args)
        {
            MappingDirectlyOnInjeCtor();
            //DynamicMappingResolve();
            //PropertyInjection();
        }

        private static void MappingDirectlyOnInjeCtor()
        {
            using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor();

            // add mappings for interfaces to InjeCtor and set their creation instruction
            injeCtor.Mapper.Add<IPlayer>().As<Player>(); // mapped to always create a new instance on creation
            injeCtor.Mapper.Add<IUserInteraction>().AsSingleton<ConsoleUserInteraction>();
            injeCtor.Mapper.Add<IRequestContext>().AsScopeSingleton<RequestContext>();
            injeCtor.Mapper.Add<IShield>().As<RoundShield>();
            injeCtor.Mapper.Add<IWeapon>().As<Sword>();

            IUserInteraction userInteraction = injeCtor.Get<IUserInteraction>();

            userInteraction.Inform("Hello World");

            string response = userInteraction.Ask<string>("Shut down application (y/n)?");

            if (response == "y")
                return;

            int value = userInteraction.GetInput<int>("Enter a number:");
            userInteraction.Inform($"Input number: {value}");

            IPlayer player = injeCtor.Get<IPlayer>();

            player.StartRunning();
            player.Hide();
            player.Attack();
            player.DecreaseHP(80);
            player.IncreaseHP(55);
            player.DecreaseHP(111);
            player.Hide();

            GetParametersForContext(injeCtor);
            SetResultForContext(injeCtor);

            IRequestContext context = injeCtor.Get<IRequestContext>();
            userInteraction.Inform($"Parameters: {string.Join(", ", context.Parameters)}");
            userInteraction.Inform($"Results: {string.Join(", ", context.ResultValues)}");

            using IScope scope = injeCtor.CreateScope();

            IRequestContext context2 = scope.Get<IRequestContext>();

            userInteraction.Inform($"Context and Context2 reference equals: {ReferenceEquals(context, context2)}");
        }

        private static void DynamicMappingResolve()
        {
            DynamicTypeMapper mapper = new DynamicTypeMapper();
            // just add the interfaces we want to use within the mapper / InjeCtor and set their creation instruction
            mapper.Add<IPlayer>(); // set the creation instruction to always -> each request will create a new instance
            mapper.Add<IUserInteraction>().AsSingleton();
            mapper.Add<IRequestContext>().AsScopeSingleton();
            mapper.Add<IShield>();
            mapper.Add<IWeapon>();
            // with resolve the current app domain / assembly load context will be searched for a matching
            // class which implements the passed interfaces
            // NOTE: the mappings will only be finished if only one matching implementation for the interface is found!
            // if there are more implementations then the mapping will not be finshed.
            // the return value indicates if the resolving was successfull => All types where found and could be finished.
            bool success = mapper.Resolve();

            using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor(mapper);

            IUserInteraction userInteraction = injeCtor.Get<IUserInteraction>();

            userInteraction.Inform("Hello World");

            string response = userInteraction.Ask<string>("Shut down application (y/n)?");

            if (response == "y")
                return;

            int value = userInteraction.GetInput<int>("Enter a number:");
            userInteraction.Inform($"Input number: {value}");

            IPlayer player1 = injeCtor.Get<IPlayer>();
            IPlayer player2 = injeCtor.Get<IPlayer>();

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

            IRequestContext context = injeCtor.Get<IRequestContext>();
            userInteraction.Inform($"Parameters: {string.Join(", ", context.Parameters)}");
            userInteraction.Inform($"Results: {string.Join(", ", context.ResultValues)}");
        }

        private static void PropertyInjection()
        {
            using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor();

            injeCtor.Mapper.Add<IPlayer>().As<Player>();
            injeCtor.Mapper.Add<IUserInteraction>().AsSingleton<ConsoleUserInteraction>();
            injeCtor.Mapper.Add<IShield>().As<RoundShield>();
            injeCtor.Mapper.Add<IWeapon>().As<Sword>();

            // if you don't want to use a attribute over the property to inject then you can provide
            // the property to inject with an expression for the concrete type as follow:
            injeCtor.TypeInformationBuilder?.AddPropertyInjection((Player p) => p.Shield);

            // you can add a type to the information builder to register it to scan for [Inject] attributes
            // NOTE: in case the passed 'ITypeInformationProvider' implements 'ITypeInformationBuilder' as well then
            // InjeCtor will add new mappings to the TypeInformationBuilder automatically IF the mapping is added after
            // instantiating InjeCtor. If The ITypeMappingProvider was instantiated before InjeCtor then you need
            // to add the types which should be added to the TypeInformationBuilder before.
            injeCtor.TypeInformationBuilder?.Add<Player>();

            IPlayer player = injeCtor.Get<IPlayer>();
            IUserInteraction userInteraction = injeCtor.Get<IUserInteraction>();

            userInteraction.Inform($"Weapon damage: {player.Weapon.Damage}");
            userInteraction.Inform($"Shield defense: {player.Shield.Defense}");
        }

        private static void GetParametersForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Get<IRequestContext>();

            context.Parameters = new object[] { "one", 2, 3, "four", 5.67 };
        }

        private static void SetResultForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Get<IRequestContext>();

            context.ResultValues = new object[] { "SUCCESS" };
        }
    }
}