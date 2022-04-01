// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options
{
    public class MultiTenantOptionsCacheShould
    {
        private Guid _tenantId = Guid.Parse("c0ae0ac2-5407-462b-8536-96f6f87dbbc8");
        private Guid _diffTenantId = Guid.Parse("1d350a71-1904-456d-b677-26da8e6b76e8");
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void AddNamedOptionsForCurrentTenantOnlyOnAdd(string name)
        {
            var ti = new TenantInfo { Id = _tenantId };
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options.
            var result = cache.TryAdd(name, options);
            Assert.True(result);

            // Fail adding options under same name.
            result = cache.TryAdd(name, options);
            Assert.False(result);

            // Change the tenant id and confirm options can be added again.
            ti.Id = _diffTenantId;
            result = cache.TryAdd(name, options);
            Assert.True(result);
        }

        [Fact]
        public void HandleNullMultiTenantContextOnAdd()
        {
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options, ensure no exception caused by null MultiTenantContext.
            var result = cache.TryAdd("", options);
            Assert.True(result);
        }

        [Fact]
        public void HandleNullMultiTenantContextOnGetOrAdd()
        {
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options, ensure no exception caused by null MultiTenantContext.
            var result = cache.GetOrAdd("", () => options);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void GetOrAddNamedOptionForCurrentTenantOnly(string name)
        {
            var ti = new TenantInfo { Id = _tenantId};
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();
            var options2 = new TestOptions();

            // Add new options.
            var result = cache.GetOrAdd(name, () => options);
            Assert.Same(options, result);

            // Get the existing options if exists.
            result = cache.GetOrAdd(name, () => options2);
            Assert.NotSame(options2, result);

            // Confirm different tenant on same object is an add (ie it didn't exist there).
            ti.Id = _diffTenantId;
            result = cache.GetOrAdd(name, () => options2);
            Assert.Same(options2, result);
        }

        [Fact]
        public void ThrowsIfGetOrAddFactoryIsNull()
        {
            var tc = new MultiTenantContext<TenantInfo>();
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("", null!));
        }

        [Fact]
        public void ThrowIfContructorParamIsNull()
        {
            var tc = new MultiTenantContext<TenantInfo>();
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;

            Assert.Throws<ArgumentNullException>(() => new MultiTenantOptionsCache<TestOptions, TenantInfo>(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void RemoveNamedOptionsForCurrentTenantOnly(string name)
        {
            var ti = new TenantInfo { Id = _tenantId };
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options.
            var result = cache.TryAdd(name, options);
            Assert.True(result);

            // Add under a different tenant.
            ti.Id = _diffTenantId;
            result = cache.TryAdd(name, options);
            Assert.True(result);
            result = cache.TryAdd("diffname", options);
            Assert.True(result);

            // Remove named options for current tenant.
            result = cache.TryRemove(name);
            Assert.True(result);
            var tenantCache = (ConcurrentDictionary<Guid, IOptionsMonitorCache<TestOptions>>?)cache.GetType().
                GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.
                GetValue(cache);

            dynamic? tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);

            // Assert named options removed and other options on tenant left as-is.
            Assert.False(tenantInternalCache!.Keys.Contains(name ?? ""));
            Assert.True(tenantInternalCache.Keys.Contains("diffname"));

            // Assert other tenant not affected.
            ti.Id = _tenantId;
            tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);
            Assert.True(tenantInternalCache!.ContainsKey(name ?? ""));
        }

        [Fact]
        public void ClearOptionsForCurrentTenantOnly()
        {
            var ti = new TenantInfo { Id = _tenantId };
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options.
            var result = cache.TryAdd("", options);
            Assert.True(result);

            // Add under a different tenant.
            ti.Id = _diffTenantId;
            result = cache.TryAdd("", options);
            Assert.True(result);

            // Clear options on first tenant.
            ti.Id = _tenantId;
            cache.Clear();

            // Assert options cleared on this tenant.
            var tenantCache = (ConcurrentDictionary<Guid, IOptionsMonitorCache<TestOptions>>?)cache.GetType().
                GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.
                GetValue(cache);

            dynamic? tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);
            Assert.True(tenantInternalCache!.IsEmpty);

            // Assert options still exist on other tenant.
            ti.Id = _diffTenantId;
            tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);
            Assert.False(tenantInternalCache!.IsEmpty);
        }

        [Fact]
        public void ClearOptionsForTenantIdOnly()
        {
            var ti = new TenantInfo { Id = _tenantId };
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options.
            var result = cache.TryAdd("", options);
            Assert.True(result);

            // Add under a different tenant.
            ti.Id = _diffTenantId;
            result = cache.TryAdd("", options);
            Assert.True(result);

            // Clear options on first tenant.
            cache.Clear(_tenantId);

            // Assert options cleared on this tenant.
            var tenantCache = (ConcurrentDictionary<Guid, IOptionsMonitorCache<TestOptions>>?)cache.GetType().
                GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.
                GetValue(cache);

            dynamic? tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[_tenantId]);
            Assert.True(tenantInternalCache!.IsEmpty);

            // Assert options still exist on other tenant.
            tenantInternalCache = tenantCache?[_diffTenantId].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[_diffTenantId]);
            Assert.False(tenantInternalCache!.IsEmpty);
        }

        [Fact]
        public void ClearAllOptionsForClearAll()
        {
            var ti = new TenantInfo { Id = _tenantId };
            var tc = new MultiTenantContext<TenantInfo>();
            tc.TenantInfo = ti;
            var tca = new MultiTenantContextAccessor<TenantInfo>();
            tca.MultiTenantContext = tc;
            var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

            var options = new TestOptions();

            // Add new options.
            var result = cache.TryAdd("", options);
            Assert.True(result);

            // Add under a different tenant.
            ti.Id = _diffTenantId;
            result = cache.TryAdd("", options);
            Assert.True(result);

            // Clear all options.
            cache.ClearAll();

            // Assert options cleared on this tenant.
            var tenantCache = (ConcurrentDictionary<Guid, IOptionsMonitorCache<TestOptions>>?)cache.GetType().
                GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.
                GetValue(cache);

            dynamic? tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);
            Assert.True(tenantInternalCache!.IsEmpty);

            // Assert options cleared on other tenant.
            ti.Id = _diffTenantId;
            tenantInternalCache = tenantCache?[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(tenantCache[ti.Id]);
            Assert.True(tenantInternalCache!.IsEmpty);
        }
    }
}