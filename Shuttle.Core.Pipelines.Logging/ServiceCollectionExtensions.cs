using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPipelineLogging(Action<PipelineLoggingBuilder>? builder = null)
        {
            services.AddOptions<PipelineLoggingOptions>();

            var pipelineLoggingBuilder = new PipelineLoggingBuilder(Guard.AgainstNull(services));

            builder?.Invoke(pipelineLoggingBuilder);

            services.AddHostedService<PipelineLogger>();

            return services;
        }
    }
}