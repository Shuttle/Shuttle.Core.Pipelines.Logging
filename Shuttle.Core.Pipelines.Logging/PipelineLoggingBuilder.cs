using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLoggingBuilder(IServiceCollection services)
{
    public PipelineLoggingBuilder Configure(Action<PipelineLoggingOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}