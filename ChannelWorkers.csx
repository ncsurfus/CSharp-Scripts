#r "nuget: System.Threading.Channels,4.5.0"

using System.Threading;
using System.Threading.Channels;

// Set high max worker count assuming worker is not CPU bound.
var workerCount = 200;
var workCount = 1000;

var channel = Channel.CreateUnbounded<string>();

var workers = Enumerable
    .Range(1, workerCount)
    .Select(async workerId =>
    {
        // Random number generator to create a random delay later on.
        var rgen = new Random(workerId);
        // await until there is data. If WaitToReadAsync returns false the channel is closed.
        while (await channel.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            // WaitToReadAsync completed, which indicates messages are available.
            // Try to get the message or continue the loop if this Task didn't get it.
            if (!channel.Reader.TryRead(out var message))
            {
                continue;
            }

            // Random async processing work...
            var delay = rgen.Next(0,5);
            await Task.Delay(delay).ConfigureAwait(false);
            Console.WriteLine($"W-{workerId} T-{Thread.CurrentThread.ManagedThreadId} D-{delay}: {message}");
        }
    }).ToArray();

// Queue work items
for(int i = 0; i != workCount; i++)
{
    await channel.Writer.WriteAsync($"TestMessage{i}").ConfigureAwait(false);
}

// Let the channel know we're closing down.
channel.Writer.Complete();

// Wait for all workers to finish.
await Task.WhenAll(workers).ConfigureAwait(false);