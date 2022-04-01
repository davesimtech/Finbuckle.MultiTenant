// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Finbuckle.MultiTenant.Test.Stores
{
    public abstract class MultiTenantStoreTestBase
    {
        private readonly Guid initechId = Guid.Parse("b2dd86f7-ced4-449f-a514-d216ef7172a8");
        private readonly Guid lolId = Guid.Parse("0aeee0d0-aa7b-44a2-9314-1e18c05cba1b");
        private readonly Guid fake = Guid.Parse("27bc8c3c-5fde-4a91-ab77-63f6d1d68ae7");
        private readonly Guid id = Guid.Parse("a0276139-df65-4906-9071-44788775a2fd");
        
        protected abstract IMultiTenantStore<TenantInfo> CreateTestStore();

        protected virtual IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
        {
            store.TryAddAsync(new TenantInfo { Id = initechId, Identifier = "initech", Name = "Initech", ConnectionString = "ConnString" }).Wait();
            store.TryAddAsync(new TenantInfo { Id = lolId, Identifier = "lol", Name = "Lol, Inc.", ConnectionString = "ConnString2" }).Wait();

            return store;
        }

        //[Fact]
        public virtual void GetTenantInfoFromStoreById()
        {
            var store = CreateTestStore();

            Assert.Equal("initech", store.TryGetAsync(initechId).Result!.Identifier);
        }

        //[Fact]
        public virtual void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
        {
            var store = CreateTestStore();

            Assert.Null(store.TryGetAsync(fake).Result);
        }

        //[Fact]
        public virtual void GetTenantInfoFromStoreByIdentifier()
        {
            var store = CreateTestStore();

            Assert.Equal("initech", store.TryGetByIdentifierAsync("initech").Result!.Identifier);
        }

        //[Fact]
        public virtual void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
        {
            var store = CreateTestStore();
            Assert.Null(store.TryGetByIdentifierAsync("fake123").Result);
        }

        //[Fact]
        public virtual void AddTenantInfoToStore()
        {
            var store = CreateTestStore();

            Assert.Null(store.TryGetByIdentifierAsync("identifier").Result);
            Assert.True(store.TryAddAsync(new TenantInfo { Id = id, Identifier = "identifier", Name = "name", ConnectionString = "cs" }).Result);
            Assert.NotNull(store.TryGetByIdentifierAsync("identifier").Result);
        }

        //[Fact]
        public virtual void UpdateTenantInfoInStore()
        {
            var store = CreateTestStore();

            var result = store.TryUpdateAsync(new TenantInfo { Id = initechId, Identifier = "initech2", Name = "Initech2", ConnectionString = "connstring2" }).Result;
            Assert.True(result);
        }

        //[Fact]
        public virtual void RemoveTenantInfoFromStore()
        {
            var store = CreateTestStore();
            Assert.NotNull(store.TryGetByIdentifierAsync("initech").Result);
            Assert.True(store.TryRemoveAsync("initech").Result);
            Assert.Null(store.TryGetByIdentifierAsync("initech").Result);
        }

        //[Fact]
        public virtual void GetAllTenantsFromStoreAsync()
        {
            var store = CreateTestStore();
            Assert.Equal(2, store.GetAllAsync().Result.Count());
        }
    }
}