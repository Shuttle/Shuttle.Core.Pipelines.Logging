using Shuttle.Extensions.Options;

namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLoggingOptions
{
    public const string SectionName = "Shuttle:Pipelines.Logging";
    public AsyncEvent<LoggingEventArgs> Logging { get; set; } = new();
    public List<PipelineLoggingFilter> Filters { get; set; } = [];
}