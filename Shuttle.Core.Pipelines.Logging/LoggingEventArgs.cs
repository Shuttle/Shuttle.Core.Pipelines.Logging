using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public class LoggingEventArgs(PipelineEventArgs pipelineEventArgs)
{
    public PipelineEventArgs PipelineEventArgs { get; } = Guard.AgainstNull(pipelineEventArgs);
    public bool ShouldSkip { get; private set; }

    public LoggingEventArgs Skip()
    {
        ShouldSkip = true;
        return this;
    }
}