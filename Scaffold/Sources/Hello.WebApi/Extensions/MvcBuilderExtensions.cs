namespace Scaffold.WebApi.Extensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddJsonOptions(options =>
            {
            });

            return builder;
        }

        public static IMvcBuilder AddCustomXmlFormatters(this IMvcBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddXmlDataContractSerializerFormatters(options =>
            {
            });

            return builder;
        }
    }
}
