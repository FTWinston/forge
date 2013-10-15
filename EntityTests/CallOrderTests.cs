﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;
using System;
using System.Collections.Generic;

namespace EntityTests {
    enum TriggerEvent {
        OnAdded,
        OnRemoved,
        OnModified,
        OnUpdate,
        OnGlobalPreUpdate,
        OnGlobalPostUpdate,
        OnInput,
        OnGlobalInput
    }

    class TriggerEventLogger : ITriggerLifecycle, ITriggerModified, ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate, ITriggerInput, ITriggerGlobalInput {
        public virtual DataAccessor[] ComputeEntityFilter() {
            return new DataAccessor[] { };
        }

        public List<TriggerEvent> _events = new List<TriggerEvent>();

        public TriggerEvent[] Events {
            get {
                return _events.ToArray();
            }
        }

        public void ClearEvents() {
            _events.Clear();
        }

        public void OnAdded(IEntity entity) {
            _events.Add(TriggerEvent.OnAdded);
        }

        public void OnRemoved(IEntity entity) {
            _events.Add(TriggerEvent.OnRemoved);
        }

        public void OnModified(IEntity entity) {
            _events.Add(TriggerEvent.OnModified);
        }

        public void OnUpdate(IEntity entity) {
            _events.Add(TriggerEvent.OnUpdate);
        }

        public void OnGlobalPreUpdate() {
            _events.Add(TriggerEvent.OnGlobalPreUpdate);
        }

        public void OnGlobalPostUpdate() {
            _events.Add(TriggerEvent.OnGlobalPostUpdate);
        }

        public Type IStructuredInputType {
            get { return typeof(int); }
        }

        public void OnInput(IStructuredInput input, IEntity entity) {
            _events.Add(TriggerEvent.OnInput);
        }

        public void OnGlobalInput(IStructuredInput input) {
            _events.Add(TriggerEvent.OnGlobalInput);
        }
    }

    class TriggerEventLoggerFilterRequiresData0 : TriggerEventLogger {
        public override DataAccessor[] ComputeEntityFilter() {
            return new DataAccessor[] { DataMap<TestData0>.Accessor };
        }
    }

    class TestData0 : Data {
        public override bool SupportsMultipleModifications {
            get { return false; }
        }

        public override void CopyFrom(Data source) {
        }

        public override int HashCode {
            get { return 0; }
        }
    }

    class TestData1 : Data {
        public override bool SupportsMultipleModifications {
            get { return false; }
        }

        public override void CopyFrom(Data source) {
        }

        public override int HashCode {
            get { return 0; }
        }
    }

    [TestClass]
    public class CallOrderTests {
        [TestMethod]
        public void Basic() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            em.AddEntity(entity);

            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate
            }, trigger.Events);
            trigger.ClearEvents();


            for (int i = 0; i < 20; ++i) {
                em.UpdateWorld();
                CollectionAssert.AreEqual(new TriggerEvent[] {
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }


            em.RemoveEntity(entity);
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }

        [TestMethod]
        public void InitializeWithAddingData() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            em.AddEntity(entity);

            entity.AddData(new TestData0());
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding new data shouldn't impact the filter, so it should be like a regular update
            entity.AddData(new TestData1());
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void EntityModifyBeforeUpdate() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            TestData0 data = new TestData0();
            entity.AddData(data);
            entity.Modify<TestData0>();
            em.AddEntity(entity);

            // even though we modified we shouldn't care -- it should be considered
            // part of the initialization
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void EntityModifyAfterUpdate() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            TestData0 data = new TestData0();
            entity.AddData(data);
            em.AddEntity(entity);

            // do the add
            em.UpdateWorld();
            trigger.ClearEvents();

            // modify the data
            entity.Modify<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnModified,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // add a Data1 instance
            entity.AddData(new TestData1());
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // modify the Data1 instance
            entity.Modify<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }


        [TestMethod]
        public void InitializeBeforeAddingDataFilter() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            TestData0 data = new TestData0();
            entity.AddData(data);
            em.AddEntity(entity);

            // entity now has the data
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding random data should not trigger a modification notification
            entity.AddData(new TestData1());
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity no longer has the data, it should get removed
            entity.RemoveData<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }

        [TestMethod]
        public void InitializeAfterAddingDataFilter() {
            EntityManager em = new EntityManager();
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddTrigger(trigger);
            Entity entity = new Entity();
            em.AddEntity(entity);

            // entity doesn't have required data
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity now has the data
            TestData0 data = new TestData0();
            entity.AddData(data);
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding random data should not trigger a modification notification
            entity.AddData(new TestData1());
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity no longer has the data, it should get removed
            entity.RemoveData<TestData0>();
            em.UpdateWorld();

            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }
    }
}
