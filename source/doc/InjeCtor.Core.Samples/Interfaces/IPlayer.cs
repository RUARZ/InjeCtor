using InjeCtor.Core.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Interfaces
{
    public interface IPlayer
    {
        void StartRunning();

        void StandStill();

        void Hide();

        void Attack();

        void DecreaseHP(int value);

        void IncreaseHP(int value);

        int HP { get; }

        bool IsAlive { get; }

        IWeapon Weapon { get; }

        IShield Shield { get; }

        PlayerState State { get; }
    }

    public enum PlayerState
    {
        Running,
        StandStill,
        Hiding,
        Attacking,
        Dead
    }
}
