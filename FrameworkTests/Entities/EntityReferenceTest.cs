using System;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave;
using Unisave.Arango;
using Unisave.Entities;
using Unisave.Facades;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityReferenceTest : BackendTestCase
    {
        private class StubPlayerEntity : Entity
        { }
        
        private class StubMatchEntity : Entity
        {
            [X] public EntityReference<StubPlayerEntity> Owner { get; set; }
        }

        private StubPlayerEntity player;
        private StubMatchEntity match;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            player = new StubPlayerEntity();
            match = new StubMatchEntity();
            player.Save();
            match.Save();
        }

        [Test]
        public void ReferenceCanBeStored()
        {
            Assert.IsTrue(match.Owner.IsNull);

            match.Owner = player;
            Assert.AreEqual(player.EntityId, match.Owner.TargetId);
            Assert.AreEqual(player.EntityId, match["Owner"].AsString);
            
            match.Save();
            match.Refresh();
            
            Assert.AreEqual(player.EntityId, match.Owner.TargetId);
            Assert.AreEqual(player.EntityId, match["Owner"].AsString);
        }

        [Test]
        public void ReferenceTargetCanBeFound()
        {
            match.Owner = player;
            match.Save();

            var found = match.Owner.Find();
            
            Assert.False(object.ReferenceEquals(player, found));
            Assert.AreEqual(player.EntityId, found.EntityId);
        }

        [Test]
        public void ReferenceTargetCanBeTested()
        {
            Assert.IsFalse(match.Owner == null); // not a valid comparison
            Assert.IsTrue(match.Owner.TargetId == null); // ok
            Assert.IsTrue(match.Owner.IsNull); // ok
            
            match.Owner = player;
            
            Assert.IsTrue(match.Owner == player);
            Assert.AreEqual(match.Owner, player);
        }

        [Test]
        public void ReferenceCanBeUsedInEntityQuery()
        {
            match.Owner = player;
            match.Save();

            var results = DB.TakeAll<StubMatchEntity>()
                .Filter(entity => entity.Owner == player)
                .Get();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(match.EntityId, results[0].EntityId);
        }

        [Test]
        public void ItChecksIdValidity()
        {
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>("foobar");
            });
            
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>("foobar/asd");
            });
            
            Assert.Throws<ArgumentException>(() => {
                new EntityReference<StubPlayerEntity>("entities_Nope/asd");
            });
            
            Assert.DoesNotThrow(() => {
                new EntityReference<StubPlayerEntity>(
                    "entities_StubPlayerEntity/asd"
                );
            });
        }
    }
}