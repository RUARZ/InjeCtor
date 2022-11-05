using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test
{
    [TestFixture]
    public class DelayedInjeCtorInitializerTest
    {
        #region Private Fields

        private DummyInjeCtorBuilder mBuilder;
        private DelayedInjeCtorInitializer mInitializer;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mBuilder = new DummyInjeCtorBuilder();
            mInitializer = new DelayedInjeCtorInitializer(mBuilder);
        }

        #endregion

        #region Tests

        [Test]
        public void Mapper_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.Mapper, Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void MappingProvider_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.MappingProvider, Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void TypeInformationProvider_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.TypeInformationProvider, Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void TypeInformationBuilder_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.TypeInformationBuilder, Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateScope_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.CreateScope, Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void Get_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.Get<Greeter>(), Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void GetFromType_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.Get(typeof(Greeter)), Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void GetScopes_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.GetScopes(), Is.Not.Null);

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void Invoke_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            MethodInvocations obj = new MethodInvocations();

            Assert.That(mInitializer.Invoke(obj, o => o.Greet), Is.Null);
            Assert.That(obj.LastGreeting, Is.EqualTo("Greetings to 'Herbert'!"));

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void InvokeStatic_InjeCtorInitialized_Success()
        {
            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            Assert.That(mInitializer.Invoke(() => StaticMethodInvocations.Add, 5, 12), Is.EqualTo(17));

            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        [Test]
        public void Mapper_MultithreadedAccess_OnlyOneBuild()
        {
            const int TASK_COUNT = 15;

            Assert.That(mBuilder.BuildCount, Is.EqualTo(0));

            ITypeMapper[] mappers = new ITypeMapper[TASK_COUNT];
            Task[] tasks = new Task[TASK_COUNT];

            for (int i = 0; i < TASK_COUNT; i++)
            {
                tasks[i] = new Task((state) =>
                {
                    int index = (int)state;
                    mappers[index] = mInitializer.Mapper;
                }, i);
            }

            Parallel.ForEach(tasks, t => t.Start());

            Task.WaitAll(tasks);

            foreach (Task task in tasks)
                task.Dispose();

            Assert.That(mappers, Is.All.SameAs(mappers[0]));
            Assert.That(mBuilder.BuildCount, Is.EqualTo(1));
        }

        #endregion
    }
}
