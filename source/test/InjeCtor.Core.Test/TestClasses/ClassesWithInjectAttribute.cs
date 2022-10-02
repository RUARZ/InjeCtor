using InjeCtor.Core.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TestClasses
{
    class SingleInjectProperty
    {
        public bool NoInject { get; set; }

        [Inject]
        public bool Inject { get; set; }
    }

    class NotValidInjectProperty
    {
        [Inject]
        public bool Inject { get; }
    }

    class MultipleInjectProperty
    {
        [Inject]
        public bool Inject1 { get; set; }
        [Inject]
        public int Inject2 { get; set; }
        [Inject]
        public string? Inject3 { get; set; }
    }

    class MultipleInjectSamePropertyTypes
    {
        [Inject]
        public bool Inject1 { get; set; }
        [Inject]
        public bool Inject2 { get; set; }
        [Inject]
        public bool Inject3 { get; set; }
        [Inject]
        public string? Inject4 { get; set; }
        [Inject]
        public string? Inject5 { get; set; }
    }
}
