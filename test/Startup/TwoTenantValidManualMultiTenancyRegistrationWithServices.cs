﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenant.AspNetCore.Tests.Startup
{
    public class TwoTenantValidManualMultiTenancyRegistrationWithServices
    {
        public void ConfigureServices(IServiceCollection services)
        {

            //Add routing
            services.AddRouting();

            //Add multi-tenant services
            services.AddMultiTenancy<TestTenant>(o => { o.DisableAutomaticPipelineRegistration = true; })
                .WithHostResolutionStrategy()
                .WithInMemoryTenantLookupService(new List<TestTenant>
                {
                    new() { Id = "1", Identifier = "tenant1.local" },
                    new() { Id = "2", Identifier = "tenant2.local" }
                }).WithTenantedServices((s, t) =>
                {
                    s.AddSingleton(new OperationIdService());
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.Map("/before-context", app =>
            {
                app.Run(async (context) =>
                {
                    var tenantLookUpService = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TestTenant>>();
                    await context.Response.WriteAsync(tenantLookUpService.TenantInfo?.Identifier ?? String.Empty);
                });
            });
            app.Map("/before-context-services", app =>
            {
                app.Run(async (context) =>
                {
                    var service = context.RequestServices.GetService<OperationIdService>();
                    var tenantLookUpService = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TestTenant>>();
                    await context.Response.WriteAsync(service?.Id.ToString()??"none");
                });
            });

            app.UseMultiTenancy<TestTenant>();
            app.Map("/after-context", app =>
            {
                app.Run(async (context) =>
                {
                    var tenantLookUpService = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TestTenant>>();
                    await context.Response.WriteAsync(tenantLookUpService.TenantInfo?.Identifier ?? String.Empty);
                });
            });

            app.Map("/after-context-services", app =>
            {
                app.Run(async (context) =>
                {
                    var service = context.RequestServices.GetService<OperationIdService>();
                    var tenantLookUpService = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TestTenant>>();
                    await context.Response.WriteAsync(service?.Id.ToString());
                });
            });
        }
    }
}
