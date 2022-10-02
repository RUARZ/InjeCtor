﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Generic dictionary for type mappings.
    /// </summary>
    internal class TypeMappingList
    {
        #region Private Fields

        private readonly object mMappedTypesLockObj = new object();
        private readonly HashSet<Type> mMappedTypes = new HashSet<Type>();
        private readonly ConcurrentDictionary<Type, ITypeMapping> mNotFinishedMappings = new ConcurrentDictionary<Type, ITypeMapping>();
        private readonly ConcurrentDictionary<Type, ITypeMapping> mFinishedMappings = new ConcurrentDictionary<Type, ITypeMapping>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Add's the passed <paramref name="mapping"/> to the mapping list.
        /// </summary>
        /// <typeparam name="T">The type of the type mapping to add.</typeparam>
        /// <param name="mapping">The <see cref="ITypeMapping"/> implementation to add.</param>
        /// <returns>The added <see cref="ITypeMapping"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the requested source type to add was already added to the list!</exception>
        public T Add<T>(T mapping) where T : ITypeMapping, INotifyOnMappingChangedTypeMapping
        {
            if (mMappedTypes.Contains(mapping.SourceType))
            {
                if (mNotFinishedMappings.TryGetValue(mapping.SourceType, out ITypeMapping? existingMapping))
                    return (T)existingMapping;

                throw new InvalidOperationException($"The type '{typeof(T).Name}' was alread added!");
            }

            lock (mMappedTypesLockObj)
            {
                mMappedTypes.Add(mapping.SourceType);
                if (mapping.MappedType != null)
                {
                    mFinishedMappings.TryAdd(mapping.SourceType, mapping);
                }
                else
                {
                    mNotFinishedMappings.TryAdd(mapping.SourceType, mapping);
                    mapping.MappingChanged += Mapping_MappingChanged;
                }
            }
            
            return mapping;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> for the already finished mappings.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> for the finishded mappings.</returns>
        public IEnumerable<ITypeMapping> GetFinishedTypeMappings()
        {
            return mFinishedMappings.Values;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> for the not finished mappings.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> for the not finishded mappings.</returns>
        public IEnumerable<ITypeMapping> GetNotFinishedTypeMappings()
        {
            return mNotFinishedMappings.Values;
        }

        /// <summary>
        /// Tries to get the finished mapping for the <typeparamref name="T"/>.
        /// If not found then <see langword="null"/> will be returned.
        /// </summary>
        /// <typeparam name="T">The type to try to get a mapping.</typeparam>
        /// <returns>The found <see cref="ITypeMapping"/> or <see langword="null"/> if no <see cref="ITypeMapping"/> could be found.</returns>
        public ITypeMapping? GetMapping<T>()
        {
            if (mFinishedMappings.TryGetValue(typeof(T), out ITypeMapping mapping))
                return mapping;

            return null;
        }

        #endregion

        #region Event handling

        private void Mapping_MappingChanged(object sender, EventArgs e)
        {
            ITypeMapping? mapping = sender as ITypeMapping;

            if (mapping is null)
                return;

            if (mapping.MappedType is null)
            {
                mFinishedMappings.TryRemove(mapping.SourceType, out ITypeMapping _);
                mNotFinishedMappings.TryAdd(mapping.SourceType, mapping);
            }
            else
            {
                mFinishedMappings.TryAdd(mapping.SourceType, mapping);
                mNotFinishedMappings.TryRemove(mapping.SourceType, out ITypeMapping _);
            }
        }

        #endregion
    }
}
