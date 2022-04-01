// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores
{
    public class InMemoryStoreShould : MultiTenantStoreTestBase
    {
        private readonly Guid initechId = Guid.Parse("b2dd86f7-ced4-449f-a514-d216ef7172a8");
        private readonly Guid lol = Guid.NewGuid();
        private IMultiTenantStore<TenantInfo> CreateCaseSensitiveTestStore()
        {
            var services = new ServiceCollection();
            services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(o => o.IsCaseSensitive = true);
            var sp = services.BuildServiceProvider();

            var store = new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>());

            var ti1 = new TenantInfo
            {
                Id = initechId,
                Identifier = "initech",
                Name = "initech"
            };
            var ti2 = new TenantInfo
            {
                Id =lol,
                Identifier = "lol",
                Name = "lol"
            };
            store.TryAddAsync(ti1).Wait();
            store.TryAddAsync(ti2).Wait();

            return store;
        }

        [Fact]
        public void GetTenantInfoFromStoreCaseInsensitiveByDefault()
        {
            var store = CreateTestStore();
            Assert.Equal("initech", store.TryGetByIdentifierAsync("iNitEch").Result!.Identifier);
        }

        [Fact]
        public void GetTenantInfoFromStoreCaseSensitive()
        {
            var store = CreateCaseSensitiveTestStore();
            Assert.Equal("initech", store.TryGetByIdentifierAsync("initech").Result!.Identifier);
            Assert.Null(store.TryGetByIdentifierAsync("iNitEch").Result);
        }

        [Fact]
        public void FailIfAddingDuplicateCaseSensitive()
        {
            var store = CreateCaseSensitiveTestStore();
            var ti1 = new TenantInfo
            {
                Id = initechId,
                Identifier = "initech",
                Name = "initech"
            };
            var ti2 = new TenantInfo
            {
                Id = initechId,
                Identifier = "iNiTEch",
                Name = "Initech"
            };
            Assert.False(store.TryAddAsync(ti1).Result);
            Assert.True(store.TryAddAsync(ti2).Result);
        }

        [Fact]
        public void FailIfAddingWithoutTenantIdentifier()
        {
            var store = CreateCaseSensitiveTestStore();
            var ti = new TenantInfo
            {
                Id = Guid.Empty,
                Name = "NullTenant"
            };

            Assert.False(store.TryAddAsync(ti).Result);
        }

        [Fact]
        public void ThrowIfDuplicateIdentifierInOptionsTenants()
        {
            var services = new ServiceCollection();
            services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(options =>
            {
                options.Tenants.Add(new TenantInfo { Id = lol, Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db" });
                options.Tenants.Add(new TenantInfo { Id = lol, Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db" });
            });
            var sp = services.BuildServiceProvider();

            Assert.Throws<MultiTenantException>(() => new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>()));
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "")]
        [InlineData("00000000-0000-0000-0000-000000000000", null)]
        [InlineData("00000000-0000-0000-0000-000000000000", null)]
        [InlineData("c990a9d7-f001-4233-82e4-aa5bcd37185b", "")]
        [InlineData("c990a9d7-f001-4233-82e4-aa5bcd37185b", null)]
        [InlineData("00000000-0000-0000-0000-000000000000", "a")]
        [InlineData("00000000-0000-0000-0000-000000000000", "a")]
        public void ThrowIfMissingIdOrIdentifierInOptionsTenants(string idString, string identifier)
        {
            var id = Guid.Parse(idString);
            var services = new ServiceCollection();
            services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(options =>
            {
                options.Tenants.Add(new TenantInfo { Id = id, Identifier = identifier, Name = "LOL", ConnectionString = "Datasource=lol.db" });
            });
            var sp = services.BuildServiceProvider();

            Assert.Throws<MultiTenantException>(() => new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>()));
        }

        // Basic store functionality tested in MultiTenantStoresShould.cs

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            var store = new InMemoryStore<TenantInfo>(null!);

            return PopulateTestStore(store);
        }

        [Fact]
        public override void GetTenantInfoFromStoreById()
        {
            base.GetTenantInfoFromStoreById();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
        }

        [Fact]
        public override void GetTenantInfoFromStoreByIdentifier()
        {
            base.GetTenantInfoFromStoreByIdentifier();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
        }

        [Fact]
        public override void AddTenantInfoToStore()
        {
            base.AddTenantInfoToStore();
        }

        [Fact]
        public override void RemoveTenantInfoFromStore()
        {
            base.RemoveTenantInfoFromStore();
        }

        [Fact]
        public override void UpdateTenantInfoInStore()
        {
            base.UpdateTenantInfoInStore();
        }

        [Fact]
        public override void GetAllTenantsFromStoreAsync()
        {
            base.GetAllTenantsFromStoreAsync();
        }
    }
}