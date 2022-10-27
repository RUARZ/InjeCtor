namespace InjeCtor.Core.Test.TestClasses
{
    interface ICircularReferenceInterfaceA {}
    interface ICircularReferenceInterfaceB { }
    interface ICircularReferenceInterfaceC { }
    interface ICircularReferenceInterfaceD { }

    class CircularReferenceClassA : ICircularReferenceInterfaceA
    {
        public CircularReferenceClassA(ICircularReferenceInterfaceB instance)
        {

        }
    }

    class CircularReferenceClassB : ICircularReferenceInterfaceB
    {
        public CircularReferenceClassB(ICircularReferenceInterfaceC instance)
        {

        }
    }

    class CircularReferenceClassC : ICircularReferenceInterfaceC
    {
        public CircularReferenceClassC(ICircularReferenceInterfaceD instance)
        {

        }
    }

    class CircularReferenceClassD : ICircularReferenceInterfaceD
    {
        public CircularReferenceClassD(ICircularReferenceInterfaceA instance)
        {

        }
    }

    class SimpleCircularReferenceClassA : ICircularReferenceInterfaceA
    {
        public SimpleCircularReferenceClassA(ICircularReferenceInterfaceB instance)
        {

        }
    }

    class SimpleCircularReferenceClassB : ICircularReferenceInterfaceB
    {
        public SimpleCircularReferenceClassB(ICircularReferenceInterfaceA instance)
        {

        }
    }
}
