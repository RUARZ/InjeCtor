using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Interfaces
{
    public interface IUserInteraction
    {
        T Ask<T>(string question);

        void Inform(string message);

        T GetInput<T>(string message);
    }
}
