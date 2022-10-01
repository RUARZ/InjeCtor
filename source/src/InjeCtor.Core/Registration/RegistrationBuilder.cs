using System;
using System.Collections.Generic;
using System.Linq;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Builder to add registrations and set definitions for them.
    /// </summary>
    public class RegistrationBuilder : IRegistrationBuilder
    {
        #region Private Fields

        private readonly Dictionary<Type, IRegistrationItem> mRegistrations = new Dictionary<Type, IRegistrationItem>();
        private readonly Dictionary<Type, IRegisteredItem> mRegisteredItems = new Dictionary<Type, IRegisteredItem>();

        #endregion

        #region IRegistrationBuilder

        /// <inheritdoc/>
        public bool IsBuild { get; private set; }

        /// <inheritdoc/>
        public IRegistrationItem<T> Add<T>()
        {
            if (mRegistrations.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"The type '{typeof(T).Name}' was alread added!");

            RegistrationItem<T> item = new RegistrationItem<T>();
            mRegistrations.Add(typeof(T), item);

            IsBuild = false;

            return item;
        }

        /// <inheritdoc/>
        public void Build()
        {
            mRegisteredItems.Clear();

            foreach (IRegistrationItem item in mRegistrations.Values)
            {
                RegisteredItem regItem = new RegisteredItem(item.RegistrationType, item.TargetType);
                
                mRegisteredItems.Add(regItem.RegisteredType, regItem);
            }

            IsBuild = true;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IRegisteredItem> GetRegisteredItems()
        {
            return mRegisteredItems.Values.ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public IRegisteredItem? GetRegistration<T>()
        {
            if (!IsBuild)
                throw new InvalidOperationException("Getting a registration item is only possible if the build was already performed!");

            if (mRegisteredItems.TryGetValue(typeof(T), out IRegisteredItem regItem))
                return regItem;

            return null;
        }

        #endregion
    }
}
