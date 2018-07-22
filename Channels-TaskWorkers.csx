#r "nuget: System.Threading.Channels,4.5.0"

using System.Threading.Channels;

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
            // Attempt to get a message
            if (!channel.Reader.TryRead(out var message))
            {
                // We were woken up for data, but failed to get a message.
                continue;
            }

            // Random async processing work...
            await Task.Delay(rgen.Next(0, 5)).ConfigureAwait(false);
            Console.WriteLine(message);
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