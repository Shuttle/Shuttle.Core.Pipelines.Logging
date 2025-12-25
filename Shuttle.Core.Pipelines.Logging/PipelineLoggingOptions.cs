using Shuttle.Extensions.Options;

namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLoggingOptions
{
    public const string SectionName = "Shuttle:Pipelines.Logging";
    public List<string> EventTypeFullNames { get; set; } = [];
    public AsyncEvent<LoggingEventArgs> Logging { get; set; } = new();
    public List<string> PipelineTypeFullNames { get; set; } = [];
    public List<string> StageNames { get; set; } = [];
}