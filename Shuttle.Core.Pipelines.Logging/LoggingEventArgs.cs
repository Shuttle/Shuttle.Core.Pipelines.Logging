using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public class LoggingEventArgs(PipelineEventArgs pipelineEventArgs, string eventName)
{
    public string EventName { get; } = Guard.AgainstEmpty(eventName);
    public PipelineEventArgs PipelineEventArgs { get; } = Guard.AgainstNull(pipelineEventArgs);
    public bool ShouldSkip { get; private set; }

    public LoggingEventArgs Skip()
    {
        ShouldSkip = true;
        return this;
    }
}