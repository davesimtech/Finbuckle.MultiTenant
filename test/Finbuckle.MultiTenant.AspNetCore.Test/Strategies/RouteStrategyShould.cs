// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies
{
    public class RouteStrategyShould
    {
        [Theory]
        [InlineData("/initech", "1d350a71-1904-456d-b677-26da8e6b76e8", "initech", "1d350a71-1904-456d-b677-26da8e6b76e8")]
        [InlineData("/", "1d350a71-1904-456d-b677-26da8e6b76e8", "initech", "")]
        public async Task ReturnExpectedIdentifier(string path, string id, string identifier, string expected)
        {
            IWebHostBuilder hostBuilder = GetTestHostBuilder(Guid.Parse(id), identifier, "{__tenant__=}");

            using (var server = new TestServer(hostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetStringAsync(path);
                Assert.Equal(expected, response);
            }
        }

        [Fact]
        public void ThrowIfContextIsNotHttpContext()
        {
            var context = new Object();
            var strategy = new RouteStrategy("__tenant__");

            Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
        }

        [Fact]
        public async Task ReturnNullIfNoRouteParamMatch()
        {
            IWebHostBuilder hostBuilder = GetTestHostBuilder(Guid.NewGuid(), "test_tenant", "{controller}");

            using (var server = new TestServer(hostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetStringAsync("/test_tenant");
                Assert.Equal("", response);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowIfRouteParamIsNullOrWhitespace(string testString)
        {
            Assert.Throws<ArgumentException>(() => new RouteStrategy(testString));
        }

        private static IWebHostBuilder GetTestHostBuilder(Guid id, string identifier, string routePattern)
        {
            return new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMultiTenant<TenantInfo>().WithRouteStrategy().WithInMemoryStore();
                    services.AddMvc();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseMultiTenant();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.Map(routePattern, async context =>
                        {
                            if (context.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
                            {
                                await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>()!.TenantInfo!.Id!.ToString());
                            }
                        });
                    });

                    var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                    store.TryAddAsync(new TenantInfo { Id = id, Identifier = identifier }).Wait();
                });
        }
    }
}