using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public static class PipelineLoggingOptionsExtensions
{
    extension(PipelineLoggingOptions pipelineLoggingOptions)
    {
        public PipelineLoggingOptions AddPipelineEventType<T>()
        {
            return pipelineLoggingOptions.AddPipelineEventType(typeof(T));
        }

        public PipelineLoggingOptions AddPipelineEventType(Type type)
        {
            Guard.AgainstNull(Guard.AgainstNull(pipelineLoggingOptions).EventTypeFullNames);

            pipelineLoggingOptions.EventTypeFullNames.Add(Guard.AgainstEmpty(Guard.AgainstNull(type).AssemblyQualifiedName));

            return pipelineLoggingOptions;
        }
    }
}