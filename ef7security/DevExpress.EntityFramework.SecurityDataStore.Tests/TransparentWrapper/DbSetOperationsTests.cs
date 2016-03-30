﻿using DevExpress.EntityFramework.SecurityDataStore.Tests.DbContexts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.EntityFramework.SecurityDataStore.Tests.TransparentWrapper {
    [TestFixture]
    public class DbSet_Operations {
        [SetUp]
        public void SetUp() {
          var dbContext = new DbContextMultiClass().MakeRealDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();     
        }
        [Test]
        public void AddNative() {
            Add(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void AddDXProvider() {
            Add(() => new DbContextMultiClass());
        }
        private void Add(Func<DbContextMultiClass> createDbContext) {
            using(var context = createDbContext()) {
                context.Add(new DbContextObject1());
                context.SaveChanges();
            }
            using(var context = createDbContext()) {
                Assert.IsNotNull(context.dbContextDbSet1.Single());
            }
        }
        [Test]
        public void AddObjectsOfDifferentTypesAtOnceNative() {
            AddObjectsOfDifferentTypesAtOnce(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void AddObjectsOfDifferentTypesAtOnceDXProvider() {
            AddObjectsOfDifferentTypesAtOnce(() => new DbContextMultiClass());
        }
        private static void AddObjectsOfDifferentTypesAtOnce(Func<DbContextMultiClass> createDbContext) {
            using(var context = createDbContext()) {
                var testValuse = new DbContextObject1();
                context.Add(new DbContextObject1());
                context.Add(new DbContextObject1());
                context.Add(new DbContextObject1());
                context.Add(new DbContextObject2());
                context.Add(new DbContextObject2());
                context.Add(new DbContextObject2());
                context.Add(new DbContextObject3());
                context.Add(new DbContextObject3());
                context.Add(new DbContextObject3());
                context.SaveChanges();
            }
            using(var context = createDbContext()) {
                Assert.AreEqual(3, context.dbContextDbSet1.Count());
                Assert.AreEqual(3, context.dbContextDbSet2.Count());
                Assert.AreEqual(3, context.dbContextDbSet3.Count());
            }
        }
        [Test]
        public void IntKeyPropertyAutoGenerateNative() {
            IntKeyPropertyAutoGenerate(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void IntKeyPropertyAutoGenerateDXProvider() {
            IntKeyPropertyAutoGenerate(() => new DbContextMultiClass());
        }
        public void IntKeyPropertyAutoGenerate(Func<DbContextMultiClass> createDbContext) {
            using(DbContextMultiClass context = createDbContext()) {

                DbContextObject1 obj1 = new DbContextObject1() { };
                context.Add(obj1);
                int installID = obj1.ID;
                Assert.IsTrue(installID != 0); //with Native test is passed is started only this test but failed when entire fixture is started: Expected: -1 But was:  -22
                context.SaveChanges();
                Assert.AreEqual(installID, obj1.ID); //what is generated and added to
            }
            int installID2;
            int installID3;
            using(DbContextMultiClass context = createDbContext()) {
                DbContextObject1 obj2 = new DbContextObject1();
                DbContextObject1 obj3 = new DbContextObject1();
                context.Add(obj2);
                context.Add(obj3);
                installID2 = obj2.ID;
                installID3 = obj3.ID;
                Assert.IsTrue(installID2 != 0); //with Native test is passed is started only this test but failed when entire fixture is started: Expected: -2 But was:  -22
                Assert.IsTrue(installID3 != 0); //with Native test is passed is started only this test but failed when entire fixture is started: Expected: -3 But was:  -22
                context.SaveChanges();
                Assert.IsTrue(
                    ((obj2.ID == installID2) && (obj3.ID == installID3))
                    ||
                    ((obj2.ID == installID3) && (obj3.ID == installID2)), "obj2.ID=" + installID2.ToString() + ", obj3.ID=" + installID3.ToString());
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.IsNotNull(context.dbContextDbSet1.Where(o => o.ID == installID2).Single());
                Assert.IsNotNull(context.dbContextDbSet1.Where(o => o.ID == installID3).Single());
            }
        }
        [Test]
        public void GuidKeyPropertyAutoGenerateNative() {
            GuidKeyPropertyAutoGenerate(() => new DbContextDbSetKeyIsGuid().MakeRealDbContext());
        }
        [Test]
        public void GuidKeyPropertyAutoGenerateDXProvider() {
            GuidKeyPropertyAutoGenerate(() => new DbContextDbSetKeyIsGuid());
        }
        public void GuidKeyPropertyAutoGenerate(Func<DbContextDbSetKeyIsGuid> createDbContext) {          
            Guid id1;
            using(DbContextDbSetKeyIsGuid context = createDbContext()) {
                DbContextObjectKeyIsGuid obj1 = new DbContextObjectKeyIsGuid();
                Assert.AreEqual(Guid.Empty, obj1.ID);
                context.Add(obj1);
                id1 = obj1.ID;
                Assert.IsTrue(id1 != Guid.Empty);
                context.SaveChanges();
                Assert.AreEqual(id1, obj1.ID);
            }

            using(DbContextDbSetKeyIsGuid context = createDbContext()) {
                Assert.IsNotNull(context.DbSet1.Where(o => o.ID == id1).Single());
            }
            Guid id2;
            Guid id3;
            using(DbContextDbSetKeyIsGuid context = createDbContext()) {
                DbContextObjectKeyIsGuid obj2 = new DbContextObjectKeyIsGuid();
                DbContextObjectKeyIsGuid obj3 = new DbContextObjectKeyIsGuid();
                context.Add(obj2);
                context.Add(obj3);
                Assert.IsTrue(obj2.ID != obj3.ID);
                Assert.IsTrue(obj2.ID != Guid.Empty);
                Assert.IsTrue(obj3.ID != Guid.Empty);
                id2 = obj2.ID;
                id3 = obj3.ID;
                context.SaveChanges();
            }
            using(DbContextDbSetKeyIsGuid context = createDbContext()) {
                Assert.IsNotNull(context.DbSet1.Where(o => o.ID == id2).Single());
                Assert.IsNotNull(context.DbSet1.Where(o => o.ID == id3).Single());
            }
        }
        [Test]
        public void IntKeyPropertyExplicitInitializeNative() {
            IntKeyPropertyExplicitInitialize(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void IntKeyPropertyExplicitInitializeDXProvider() {
            IntKeyPropertyExplicitInitialize(() => new DbContextMultiClass());
        }
        public void IntKeyPropertyExplicitInitialize(Func<DbContextMultiClass> createDbContext) {
            using(DbContextMultiClass context = createDbContext()) {
                DbContextObject4 obj = new DbContextObject4() { ID = 4 };
                context.Add(obj);
                Assert.AreEqual(4, obj.ID);
                context.SaveChanges();
                Assert.AreEqual(4, obj.ID);
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.IsNotNull(context.dbContextDbSet1.Where(o => o.ID == 4));
            }

            using(DbContextMultiClass context = createDbContext()) {
                context.Add(new DbContextObject4() { ID = 5 });
                try {
                    context.Add(new DbContextObject4() { ID = 5 });
                    Assert.Fail();
                }
                catch(NUnit.Framework.AssertionException) { }
                catch {
                    //System.InvalidOperationException : The instance of entity type 'DevExpress.EntityFramework.DbContextDataStore.Tests.DbContextObject4' cannot be tracked because another instance of this type with the same key is already being tracked. For new entities consider using an IIdentityGenerator to generate unique key values.;
                }
            }
        }
        [Test]
        public void RemoveNative() {
            Remove(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void RemoveDXProvider() {
            Remove(() => new DbContextMultiClass());
        }
        private void Remove(Func<DbContextMultiClass> createDbContext) {
            using(var context = createDbContext()) {
                context.Add(new DbContextObject1());
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                context.Remove(context.dbContextDbSet1.Single());
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.AreEqual(0, context.dbContextDbSet1.Count());
            }
        }
        [Test]
        public void RemoveMultiClassNative() {
            RemoveMultiClass(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void RemoveMultiClassDXProvider() {
            RemoveMultiClass(() => new DbContextMultiClass());
        }
        private static void RemoveMultiClass(Func<DbContextMultiClass> createDbContext) {
            using(DbContextMultiClass context = createDbContext()) {
                context.Add(new DbContextObject1() { ItemName = "1" });
                context.Add(new DbContextObject2() { User = "2" });
                context.Add(new DbContextObject3() { Notes = "3" });
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                var itemDbContextObject1 = context.dbContextDbSet1.Single();
                var itemDbContextObject2 = context.dbContextDbSet2.Single();
                var itemDbContextObject3 = context.dbContextDbSet3.Single();
                DbContextObject1.Count = 0;
                context.Remove(itemDbContextObject1);
                context.Remove(itemDbContextObject2);
                context.Remove(itemDbContextObject3);
                context.SaveChanges();
                Assert.AreEqual(0, DbContextObject1.Count);
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.AreEqual(0, context.dbContextDbSet1.Count());
                Assert.AreEqual(0, context.dbContextDbSet2.Count());
                Assert.AreEqual(0, context.dbContextDbSet3.Count());
            }
        }
        [Test]
        public void UpdateNative() {
            Update(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void UpdateDXProvider() {
            Update(() => new DbContextMultiClass());
        }
        private void Update(Func<DbContextMultiClass> createDbContext) {
            using(var context = createDbContext()) {
                context.Add(new DbContextObject1() { ItemCount = 1 });
                context.Add(new DbContextObject1() { ItemCount = 2 });
                context.SaveChanges();
            }
            using(var context = createDbContext()) {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                var item = context.dbContextDbSet1.Single(p => p.ItemCount == 1);
                item.ItemCount = 4;
                context.Update(item);
                var item2 = context.dbContextDbSet1.Single(p => p.ItemCount == 2);
                item2.ItemCount = 5;
                context.Update(item2);
                context.SaveChanges();
            }
            using(var context = createDbContext()) {
                Assert.AreEqual(1, context.dbContextDbSet1.Where(p => p.ItemCount == 4).Count());
                Assert.AreEqual(1, context.dbContextDbSet1.Where(p => p.ItemCount == 5).Count());
                Assert.AreEqual(2, context.dbContextDbSet1.Count());
            }
        }
        [Test]
        public void UpdateMultiClassNative() {
            UpdateMultiClass(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void Update_MultiClass_DXProvider() {
            UpdateMultiClass(() => new DbContextMultiClass());
        }
        private static void UpdateMultiClass(Func<DbContextMultiClass> createDbContext) {
            using(DbContextMultiClass context = createDbContext()) {
                context.Add(new DbContextObject1() { ItemName = "1" });
                context.Add(new DbContextObject2() { User = "2" });
                context.Add(new DbContextObject3() { Notes = "3" });
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                foreach(var item in context.dbContextDbSet1) {
                    item.ItemName += "+";
                }
                foreach(var item in context.dbContextDbSet2) {
                    item.User += "+";
                }
                foreach(var item in context.dbContextDbSet3) {
                    item.Notes += "+";
                }
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                DbContextObject1.Count = 0;
                foreach(var item in context.dbContextDbSet1) {
                    Assert.AreEqual(item.ItemName.Length - 1, item.ItemName.IndexOf("+"));
                }

                foreach(var item in context.dbContextDbSet2) {
                    Assert.AreEqual(item.User.Length - 1, item.User.IndexOf("+"));
                }
                foreach(var item in context.dbContextDbSet3) {
                    Assert.AreEqual(item.Notes.Length - 1, item.Notes.IndexOf("+"));
                }
//                Assert.AreEqual(DbContextObject1.Count, 1);
            }
        }
        [Test]
        public void AttachNative() {
            Attach(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void AttachDXProvider() {
            Attach(() => new DbContextMultiClass());
        }
        private void Attach(Func<DbContextMultiClass> createDbContext) {
            using(DbContextMultiClass context = createDbContext()) {
                context.Add(new DbContextObject1() { ItemCount = 1 });
                context.Add(new DbContextObject1() { ItemCount = 2 });
                context.SaveChanges();
            }
            using(var context = createDbContext()) {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                var entry1 = context.Attach(new DbContextObject1() { ID = 1, ItemCount = 4 });
                Assert.AreEqual(4, entry1.Entity.ItemCount);
                var entry2 = context.Attach(new DbContextObject1() { ID = 2, ItemCount = 5 });
                Assert.AreEqual(5, entry2.Entity.ItemCount);
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.AreEqual(1, context.dbContextDbSet1.Single(p => p.ItemCount == 1).ItemCount);
                Assert.AreEqual(2, context.dbContextDbSet1.Single(p => p.ItemCount == 2).ItemCount);
                Assert.AreEqual(2, context.dbContextDbSet1.Count());
            }
        }
        [Test]
        public void ModyfyNative() {
            Modyfy(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void ModyfyDXProvider() {
            Modyfy(() => new DbContextMultiClass());
        }
        private void Modyfy(Func<DbContextMultiClass> createDbContext) {
            using(var context = createDbContext()) {
                context.Add(new DbContextObject1());
                context.SaveChanges();
            }
            using(DbContextMultiClass context = createDbContext()) {
                DbContextObject1.Count = 0;
                var item = context.dbContextDbSet1.Single();
                item.ItemCount = 5;
                context.SaveChanges();
//                Assert.AreEqual(1, DbContextObject1.Count);
            }
            using(DbContextMultiClass context = createDbContext()) {
                Assert.AreEqual(1, context.dbContextDbSet1.Where(p => p.ItemCount == 5).Count());
            }
        }
        [Test]
        public void EntryNative() {
            Entry(() => new DbContextMultiClass().MakeRealDbContext());
        }
        [Test]
        public void EntryDXProvider() {
            Entry(() => new DbContextMultiClass());
        }
        private void Entry(Func<DbContextMultiClass> createDbContext) {
            var itemMold = new DbContextObject1() { ItemName = "1" };

            using(DbContextMultiClass context = createDbContext()) {
                var item = context.Entry(itemMold);
                Assert.AreNotEqual(item, null);
            }
        }
    }
}
