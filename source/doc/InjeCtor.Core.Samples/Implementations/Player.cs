using InjeCtor.Core.Attribute;
using InjeCtor.Core.Samples.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Implementations
{
    public class Player : IPlayer
    {
        IUserInteraction mUserInteraction;

        public Player(IUserInteraction userInteraction)
        {
            mUserInteraction = userInteraction;

            HP = 100;
            State = PlayerState.StandStill;
        }

        public int HP { get; private set; }

        public bool IsAlive { get; private set; }

        public PlayerState State { get; private set; }

        [Inject]
        public IWeapon Weapon { get; set; }

        public IShield Shield { get; set; }

        public void Attack()
        {
            ExecuteActionIfPossible(() =>
            {
                mUserInteraction.Inform("Start attacking...");
                State = PlayerState.Attacking;
            });
        }

        public void DecreaseHP(int value)
        {
            if (HP <= 0)
            {
                mUserInteraction.Inform("Player already dead.");
                return;
            }

            HP -= value;

            if (HP > 0)
            {
                mUserInteraction.Inform($"{HP} HP remaining.");
                return;
            }

            mUserInteraction.Inform("HP <= 0, Player died...");

            State = PlayerState.Dead;
        }

        public void Hide()
        {
            ExecuteActionIfPossible(() =>
            {
                mUserInteraction.Inform("Hiding somewhere...");

                State = PlayerState.Hiding;
            });
        }

        public void IncreaseHP(int value)
        {
            ExecuteActionIfPossible(() =>
            {
                HP += value;
                mUserInteraction.Inform($"Some HP restored, now {HP} HP remaining.");
            });
        }

        public void StandStill()
        {
            ExecuteActionIfPossible(() =>
            {
                mUserInteraction.Inform("Stoped last action, now standing still...");

                State = PlayerState.StandStill;
            });
        }

        public void StartRunning()
        {
            ExecuteActionIfPossible(() =>
            {
                mUserInteraction.Inform("Started running...");

                State = PlayerState.Running;
            });
        }

        private void ExecuteActionIfPossible(Action action)
        {
            if (State == PlayerState.Dead)
            {
                mUserInteraction.Inform("Player is dead, no action possible...");
                return;
            }

            action.Invoke();
        }
    }
}
