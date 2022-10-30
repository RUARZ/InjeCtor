using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Interfaces
{
    internal interface IPlayer
    {
        void StartRunning();

        void StandStill();

        void Hide();

        void Attack();

        void DecreaseHP(int value);

        void IncreaseHP(int value);

        int HP { get; }

        bool IsAlive { get; }

        PlayerState State { get; }
    }

    enum PlayerState
    {
        Running,
        StandStill,
        Hiding,
        Attacking,
        Dead
    }
}
