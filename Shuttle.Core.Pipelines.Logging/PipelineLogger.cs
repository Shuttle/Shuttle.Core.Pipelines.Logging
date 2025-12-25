using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLogger(ILogger<PipelineLogger> logger, IOptions<PipelineOptions> pipelineOptions, IOptions<PipelineLoggingOptions> pipelineLoggingOptions)
    : IHostedService
{
    private readonly Dictionary<string, int> _callCounts = new();
    private readonly ILogger<PipelineLogger> _logger = Guard.AgainstNull(logger);
    private readonly PipelineLoggingOptions _pipelineLoggingOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineLoggingOptions).Value);
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated += PipelineCreated;
        _pipelineOptions.PipelineObtained += PipelineObtained;
        _pipelineOptions.PipelineStarting += PipelineStarting;
        _pipelineOptions.PipelineCompleted += PipelineCompleted;
        _pipelineOptions.PipelineReleased += PipelineReleased;
        _pipelineOptions.StageStarting += StageStarting;
        _pipelineOptions.StageCompleted += StageCompleted;
        _pipelineOptions.EventStarting += EventStarting;
        _pipelineOptions.EventCompleted += EventCompleted;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated += PipelineCreated;
        _pipelineOptions.PipelineObtained += PipelineObtained;
        _pipelineOptions.PipelineStarting += PipelineStarting;
        _pipelineOptions.PipelineCompleted += PipelineCompleted;
        _pipelineOptions.PipelineReleased += PipelineReleased;
        _pipelineOptions.StageStarting += StageStarting;
        _pipelineOptions.StageCompleted += StageCompleted;
        _pipelineOptions.EventStarting += EventStarting;
        _pipelineOptions.EventCompleted += EventCompleted;

        return Task.CompletedTask;
    }

    private async Task EventCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipEventAsync(eventArgs, nameof(EventStarting), cancellationToken))
        {
            return;
        }

        await TraceEventAsync(eventArgs, nameof(EventCompleted));
    }

    private async Task EventStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipEventAsync(eventArgs, nameof(EventStarting), cancellationToken))
        {
            return;
        }

        await TraceEventAsync(eventArgs, nameof(EventStarting));
    }

    private void Increment(string key)
    {
        _callCounts.TryAdd(key, 0);
        _callCounts[key] += 1;
    }

    private async Task PipelineCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipPipelineAsync(eventArgs, nameof(PipelineCompleted), cancellationToken))
        {
            return;
        }

        await TracePipelineAsync(eventArgs, nameof(PipelineCompleted));
    }

    private async Task PipelineCreated(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipPipelineAsync(eventArgs, nameof(PipelineCreated), cancellationToken))
        {
            return;
        }

        await TracePipelineAsync(eventArgs, nameof(PipelineCreated));
    }

    private async Task PipelineObtained(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipPipelineAsync(eventArgs, nameof(PipelineObtained), cancellationToken))
        {
            return;
        }

        await TracePipelineAsync(eventArgs, nameof(PipelineObtained));
    }

    private async Task PipelineReleased(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipPipelineAsync(eventArgs, nameof(PipelineReleased), cancellationToken))
        {
            return;
        }

        await TracePipelineAsync(eventArgs, nameof(PipelineReleased));
    }

    private async Task PipelineStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipPipelineAsync(eventArgs, nameof(PipelineStarting), cancellationToken))
        {
            return;
        }

        await TracePipelineAsync(eventArgs, nameof(PipelineStarting));
    }

    private async ValueTask<bool> ShouldSkipAsync(PipelineEventArgs eventArgs, string eventName, string candidate, List<string> collection, CancellationToken cancellationToken)
    {
        var loggingEventArgs = new LoggingEventArgs(eventArgs, eventName);

        await _pipelineLoggingOptions.Logging.InvokeAsync(loggingEventArgs, cancellationToken);

        if (loggingEventArgs.ShouldSkip)
        {
            return true;
        }

        return collection.Any() && collection.All(type => type != candidate);
    }

    private async ValueTask<bool> ShouldSkipEventAsync(PipelineEventArgs eventArgs, string eventName, CancellationToken cancellationToken)
    {
        return await ShouldSkipAsync(eventArgs, eventName, eventArgs.Pipeline.EventType?.FullName ?? "unknown", _pipelineLoggingOptions.EventTypeFullNames, cancellationToken);
    }

    private async ValueTask<bool> ShouldSkipPipelineAsync(PipelineEventArgs eventArgs, string eventName, CancellationToken cancellationToken)
    {
        return await ShouldSkipAsync(eventArgs, eventName, Guard.AgainstEmpty(eventArgs.Pipeline.GetType().FullName), _pipelineLoggingOptions.PipelineTypeFullNames, cancellationToken);
    }

    private async ValueTask<bool> ShouldSkipStageAsync(PipelineEventArgs eventArgs, string eventName, CancellationToken cancellationToken)
    {
        return await ShouldSkipAsync(eventArgs, eventName, eventArgs.Pipeline.StageName, _pipelineLoggingOptions.StageNames, cancellationToken);
    }

    private async Task StageCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipStageAsync(eventArgs, nameof(StageCompleted), cancellationToken))
        {
            return;
        }

        await TraceEventAsync(eventArgs, nameof(StageCompleted));
    }

    private async Task StageStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (await ShouldSkipStageAsync(eventArgs, nameof(StageStarting), cancellationToken))
        {
            return;
        }

        await TraceEventAsync(eventArgs, nameof(StageStarting));
    }

    protected async Task TraceEventAsync(PipelineEventArgs eventArgs, string eventName)
    {
        var eventTypeName = eventArgs.Pipeline.EventType?.FullName ?? "unknown";
        var key = $"{eventName}:{eventTypeName}";

        Increment(key);

        _logger.LogTrace("[{EventName}:{EventTypeName}] : pipeline = {Pipeline} / call count = {CallCount} / managed thread id = {CurrentManagedThreadId}", eventName, eventTypeName, eventArgs.Pipeline.GetType().FullName, _callCounts[key], Environment.CurrentManagedThreadId);

        await Task.CompletedTask;
    }

    protected async Task TracePipelineAsync(PipelineEventArgs eventArgs, string eventName)
    {
        var pipelineTypeName = eventArgs.Pipeline.GetType().FullName;
        var key = $"{eventName}:{pipelineTypeName}";

        Increment(key);

        _logger.LogTrace("[{EventName}] : pipeline = {Pipeline} / call count = {CallCount} / managed thread id = {CurrentManagedThreadId}", eventName, pipelineTypeName, _callCounts[key], Environment.CurrentManagedThreadId);

        await Task.CompletedTask;
    }

    protected async Task TraceStageAsync(PipelineEventArgs eventArgs, string eventName)
    {
        var stageName = eventArgs.Pipeline.StageName;

        if (string.IsNullOrWhiteSpace(stageName))
        {
            stageName = "unknown";
        }

        var key = $"{eventName}:{stageName}";

        Increment(key);

        _logger.LogTrace("[{EventName}:{StageName}] : pipeline = {Pipeline} / call count = {CallCount} / managed thread id = {CurrentManagedThreadId}", eventName, stageName, eventArgs.Pipeline.GetType().FullName, _callCounts[key], Environment.CurrentManagedThreadId);

        await Task.CompletedTask;
    }
}