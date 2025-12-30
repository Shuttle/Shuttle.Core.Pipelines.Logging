namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLoggingFilter
{
    public string PipelineName { get; set; } = string.Empty;

    public List<string> EventNames { get; set; } = [];
    public List<string> StageNames { get; set; } = [];
}