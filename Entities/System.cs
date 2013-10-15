﻿using Neon.Collections;
using Neon.Utility;

namespace Neon.Entities {
    /// <summary>
    /// Internal storage format for ISystems that perform better with caching.
    /// </summary>
    internal class System {
        private MetadataKey _metadataKey;
        private Filter _filter;

        /// <summary>
        /// The list of entities which are currently in the system.
        /// </summary>
        public UnorderedList<IEntity> CachedEntities;

        /// <summary>
        /// The trigger used for the system.
        /// </summary>
        public ITriggerBaseFilter Trigger;

        /// <summary>
        /// Creates a new system. Entities are added to the system based on if they
        /// pass the given filter.
        /// </summary>
        public System(ITriggerBaseFilter trigger) {
            _filter = new Filter(trigger.ComputeEntityFilter());
            _metadataKey = Entity.MetadataRegistry.GetKey();

            Trigger = trigger;
            CachedEntities = new UnorderedList<IEntity>();
        }

        /// <summary>
        /// Updates the status of the entity inside of the cache; ie, if the entity is now passing
        /// the filter but was not before, then it will be added to the cache.
        /// </summary>
        /// <returns>The change in cache status for the entity</returns>
        public void UpdateCache(IEntity entity) {
            // get our unordered list metadata or create it
            UnorderedListMetadata metadata = (UnorderedListMetadata)entity.Metadata[_metadataKey];
            if (metadata == null) {
                metadata = new UnorderedListMetadata();
                entity.Metadata[_metadataKey] = metadata;
            }

            bool passed = _filter.Check(entity);
            bool contains = CachedEntities.Contains(entity, metadata);

            // The entity is not in the cache it now passes the filter, so add it to the cache
            if (contains == false && passed) {
                CachedEntities.Add(entity, metadata);
                if (OnAddedToCache != null) {
                    OnAddedToCache(entity);
                }
            }

            // The entity is in the cache but it no longer passes the filter, so remove it
            if (contains && passed == false) {
                CachedEntities.Remove(entity, metadata);
                if (OnRemovedFromCache != null) {
                    OnRemovedFromCache(entity);
                }
            }

            // no change to the cache
        }

        /// <summary>
        /// Ensures that an Entity is not in the cache.
        /// </summary>
        public void Remove(IEntity entity) {
            if (CachedEntities.Remove(entity, (UnorderedListMetadata)entity.Metadata[_metadataKey])) {
                if (OnRemovedFromCache != null) {
                    OnRemovedFromCache(entity);
                }
            }
        }

        public delegate void OnCacheChangeDelegate(IEntity entity);

        /// <summary>
        /// Called when the given entity has been added to the cache.
        /// </summary>
        public event OnCacheChangeDelegate OnAddedToCache;

        /// <summary>
        /// Called when the given entity has been removed from the cache.
        /// </summary>
        public event OnCacheChangeDelegate OnRemovedFromCache;
    }
}
