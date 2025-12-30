using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines.Logging;

public class PipelineLogger(ILogger<PipelineLogger> logger, IOptions<PipelineOptions> pipelineOptions, IOptions<PipelineLoggingOptions> pipelineLoggingOptions)
    : IHostedService
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, int> _callCounts = new();
    private readonly ILogger<PipelineLogger> _logger = Guard.AgainstNull(logger);
    private readonly PipelineLoggingOptions _pipelineLoggingOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineLoggingOptions).Value);
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineAborted += PipelineAborted;
        _pipelineOptions.PipelineCreated += PipelineCreated;
        _pipelineOptions.PipelineStarting += PipelineStarting;
        _pipelineOptions.PipelineCompleted += PipelineCompleted;
        _pipelineOptions.StageStarting += StageStarting;
        _pipelineOptions.StageCompleted += StageCompleted;
        _pipelineOptions.EventStarting += EventStarting;
        _pipelineOptions.EventCompleted += EventCompleted;

        _pipelineOptions.TransactionScopeIgnored += TransactionScopeIgnored;
        _pipelineOptions.TransactionScopeStarting += TransactionScopeStarting;

        return Task.CompletedTask;
    }

    private async Task TransactionScopeIgnored(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task TransactionScopeStarting(TransactionScopeEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineAborted -= PipelineAborted;
        _pipelineOptions.PipelineCreated -= PipelineCreated;
        _pipelineOptions.PipelineStarting -= PipelineStarting;
        _pipelineOptions.PipelineCompleted -= PipelineCompleted;
        _pipelineOptions.StageStarting -= StageStarting;
        _pipelineOptions.StageCompleted -= StageCompleted;
        _pipelineOptions.EventStarting -= EventStarting;
        _pipelineOptions.EventCompleted -= EventCompleted;

        _pipelineOptions.TransactionScopeIgnored -= TransactionScopeIgnored;
        _pipelineOptions.TransactionScopeStarting -= TransactionScopeStarting;

        return Task.CompletedTask;
    }

    private async Task EventCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task EventStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task IncrementAsync(string key)
    {
        await _lock.WaitAsync();

        try
        {
            _callCounts.TryAdd(key, 0);
            _callCounts[key] += 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task PipelineAborted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task PipelineCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task PipelineCreated(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task PipelineStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task StageCompleted(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }

    private async Task StageStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        await TraceAsync(eventArgs, cancellationToken);
    }


    protected async Task TraceAsync(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var pipelineName = eventArgs.Pipeline.GetType().FullName;
        var stageName = !string.IsNullOrWhiteSpace(eventArgs.Pipeline.StageName) ? eventArgs.Pipeline.StageName : "(no stage))";
        var eventName = eventArgs.Pipeline.EventType?.FullName ?? "(no event)";

        var loggingEventArgs = new LoggingEventArgs(eventArgs);

        await _pipelineLoggingOptions.Logging.InvokeAsync(loggingEventArgs, cancellationToken);

        if (loggingEventArgs.ShouldSkip)
        {
            return;
        }
    
        if (_pipelineLoggingOptions.Filters.Any())
        {
            var filter = _pipelineLoggingOptions.Filters.FirstOrDefault(item => item.PipelineName.Equals(pipelineName, StringComparison.InvariantCultureIgnoreCase));

            if (filter == null)
            {
                return;
            }

            if ((filter.StageNames.Any() && filter.StageNames.All(type => type != stageName)) ||
                (filter.EventNames.Any() && filter.EventNames.All(type => type != eventName)))
            {
                return;
            }
        }

        var key = $"{pipelineName}:{stageName}:{eventName}";

        await IncrementAsync(key);

        _logger.LogTrace("[{EventTypeName}] : pipeline = '{Pipeline}' / stage = '{StageName}' / call count = {CallCount} / managed thread id = {CurrentManagedThreadId}", eventName, pipelineName, stageName, _callCounts[key], Environment.CurrentManagedThreadId);

        await Task.CompletedTask;
    }
}