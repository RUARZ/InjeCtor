using InjeCtor.Core.Samples.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Implementations
{
    internal class Sword : IWeapon
    {
        public int Damage => 50;
    }
}
