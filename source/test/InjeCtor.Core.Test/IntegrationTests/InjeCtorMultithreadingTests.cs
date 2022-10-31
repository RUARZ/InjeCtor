using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.IntegrationTests
{
    [TestFixture]
    public class InjeCtorMultithreadingTests
    {
        #region Consts

        private const int TASK_COUNT = 5;

        #endregion

        #region Private Fields

        private IInjeCtor mInjeCtor;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mInjeCtor = new InjeCtor();

            mInjeCtor.Mapper.Add<IGreeter>().As<Greeter>();
            mInjeCtor.Mapper.AddScopeSingleton<ScopeCreationCounter>();
            mInjeCtor.Mapper.Add<BaseClassForSingleton>().AsSingleton<SingletonClass>();

            SingletonClass.ResetCounter();
            ScopeCreationCounter.ResetCounter();
        }

        #endregion

        #region Tests

        [Test]
        public void Create_SingletonMultipleThreads_SameInstanceReturned()
        {
            BaseClassForSingleton[] singletons = new BaseClassForSingleton[TASK_COUNT];
            Task[] tasks = new Task[TASK_COUNT];

            for (int i = 0; i < TASK_COUNT; i++)
            {
                tasks[i] = new Task((state) =>
                {
                    int index = (int)state;
                    singletons[index] = mInjeCtor.Get<BaseClassForSingleton>();
                }, i);
            }

            Parallel.ForEach(tasks, t => t.Start());

            Task.WaitAll(tasks);

            foreach (Task task in tasks)
                task.Dispose();

            Assert.That(singletons, Is.All.Not.Null);
            Assert.That(singletons, Is.All.SameAs(singletons[0]));

            // this test should prove that, in case of multiple same accesses, the instance may be created multiple times
            // BUT we always get the same instance back! Therefore also a check for multiple creations of the singleton class.
            Assert.That(SingletonClass.CreationCounter, Is.GreaterThan(1));
        }

        [Test]
        public void Create_ScopeSingletonMultipleThreads_SameInstanceReturned()
        {
            IScope scope = mInjeCtor.CreateScope();

            ScopeCreationCounter[] scopeSingletons = new ScopeCreationCounter[TASK_COUNT];
            Task[] tasks = new Task[TASK_COUNT];

            for (int i = 0; i < TASK_COUNT; i++)
            {
                tasks[i] = new Task((state) =>
                {
                    int index = (int)state;
                    scopeSingletons[index] = scope.Get<ScopeCreationCounter>();
                }, i);
            }

            Parallel.ForEach(tasks, t => t.Start());

            Task.WaitAll(tasks);

            foreach (Task task in tasks)
                task.Dispose();

            Assert.That(scopeSingletons, Is.All.Not.Null);
            Assert.That(scopeSingletons, Is.All.SameAs(scopeSingletons[0]));

            // this test should prove that, in case of multiple same accesses, the instance may be created multiple times
            // BUT we always get the same instance back! Therefore also a check for multiple creations of the singleton class.
            Assert.That(ScopeCreationCounter.CreationCounter, Is.GreaterThan(1));
        }

        #endregion
    }
}
