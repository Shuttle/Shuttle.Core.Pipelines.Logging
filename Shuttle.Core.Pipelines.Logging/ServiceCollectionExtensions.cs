using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPipelineLogging(Action<PipelineLoggingBuilder>? builder = null)
        {
            var pipelineLoggingBuilder = new PipelineLoggingBuilder(Guard.AgainstNull(services));

            builder?.Invoke(pipelineLoggingBuilder);

            services.AddOptions<PipelineLoggingOptions>().Configure(options =>
            {
                options.PipelineTypeFullNames = pipelineLoggingBuilder.Options.PipelineTypeFullNames;
                options.EventTypeFullNames = pipelineLoggingBuilder.Options.EventTypeFullNames;
                options.StageNames = pipelineLoggingBuilder.Options.StageNames;

                options.Logging = pipelineLoggingBuilder.Options.Logging;
            });

            services.AddHostedService<PipelineLogger>();

            return services;
        }
    }
}