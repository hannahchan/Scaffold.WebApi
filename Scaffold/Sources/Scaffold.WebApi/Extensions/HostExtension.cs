namespace Scaffold.WebApi.Extensions
{
    using System;
    using Jaeger;
    using Jaeger.Senders;
    using Jaeger.Senders.Thrift;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using OpenTracing.Util;
    using Scaffold.Repositories.PostgreSQL;

    public static class HostExtension
    {
        public static IHost EnsureCreatedDatabase(this IHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                IHostEnvironment hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

                if (hostEnvironment.IsDevelopment())
                {
                    BucketContext context = serviceProvider.GetRequiredService<BucketContext>();
                    context.Database.EnsureCreated();
                }
            }

            return host;
        }

        public static IHost MigrateDatabase(this IHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                IHostEnvironment hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

                if (hostEnvironment.IsDevelopment())
                {
                    BucketContext context = serviceProvider.GetRequiredService<BucketContext>();
                    context.Database.Migrate();
                }
            }

            return host;
        }

        public static IHost RegisterGlobalTracer(this IHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                    .RegisterSenderFactory<ThriftSenderFactory>();

                GlobalTracer.Register(Configuration.FromEnv(loggerFactory).GetTracer());
            }

            return host;
        }
    }
}
