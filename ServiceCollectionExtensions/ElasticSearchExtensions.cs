using System;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;

namespace ElasticSearchLowLevelClientDotNetCore3Sample.ServiceCollectionExtensions
{
    public static class ElasticSearchExtensions
    {
        public static IServiceCollection RegisterElasticEndpoint(this IServiceCollection services, string indexName)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"))
                .RequestTimeout(TimeSpan.FromMinutes(2));

            var lowlevelClient = new ElasticLowLevelClient(settings);

            //settings.DefaultIndex(indexName);

            services.AddTransient<IElasticLowLevelClient>(e => lowlevelClient);

            return services;
        }
    }
}
