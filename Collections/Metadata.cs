﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Collections {
    /// <summary>
    /// Used to create metadata keys that can access data in MetadataContainers.
    /// </summary>
    public class MetadataRegistry {
        private int _nextKey;

        //private static Dictionary<object, MetadataKey> _keys = new Dictionary<object, MetadataKey>();

        public MetadataKey GetKey() {
            MetadataKey key;
            //if (_keys.TryGetValue(reference, out key)) {
            //   return key;
            //}

            key = new MetadataKey() {
                Index = Interlocked.Increment(ref _nextKey)
            };
            //_keys.Add(reference, key);
            return key;
        }
    }

    /// <summary>
    /// Stores metadata.
    /// </summary>
    public class MetadataContainer<T> {
        /// <summary>
        /// The actual data storage
        /// </summary>
        private SparseArray<T> _container = new SparseArray<T>();

        /// <summary>
        /// Returns the stored metadata value for the given key.
        /// </summary>
        public T Get(MetadataKey key) {
            return _container[key.Index];
        }

        /// <summary>
        /// Updates the stored metadata value for the given key.
        /// </summary>
        public void Set(MetadataKey key, T value) {
            _container[key.Index] = value;
        }

        /// <summary>
        /// Attempts to remove the object store at the key.
        /// </summary>
        /// <remarks>
        /// This just calls Set(key, default(T)).
        /// </remarks>
        public void Remove(MetadataKey key) {
            Set(key, default(T));
        }

        /// <summary>
        /// Store or retrieve a metadata value.
        /// </summary>
        /// <param name="key">The key used. Used CreateKey to create a new one.</param>
        /// <returns>The stored metadata.</returns>
        public T this[MetadataKey key] {
            get {
                return Get(key);
            }

            set {
                Set(key, value);
            }
        }
    }

    /// <summary>
    /// Stores a slot inside of a metadata container.
    /// </summary>
    public struct MetadataKey {
        /// <summary>
        /// The index into the internal array that the key maps to
        /// </summary>
        internal int Index;
    }
}
